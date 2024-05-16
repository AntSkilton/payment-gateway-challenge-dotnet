using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    private List<PostPaymentResponse> _paymentResponses;

    public PaymentsRepository()
    {
        _paymentResponses = PaymentsHelper.GeneratePaymentStubs();
    }

    public PostPaymentResponse GetPayment(Guid id)
    {
        return _paymentResponses.FirstOrDefault(p => p.Id == id);
    }
    
    public IEnumerable<PostPaymentResponse> GetAllPayments()
    {
        return _paymentResponses;
    }

    public PostPaymentResponse ProcessPayment(PostPaymentRequest request, out JObject errorJObj)
    {
        var errorObj = new ErrorObj
        {
            Status = 400,
            ValidationCodes = new List<ValidationCodes>(),
            ErrorMessages = new List<string>()
        };

        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Rejected,
            CardNumberLastFour = 0,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency.ToUpper(),
            Amount = request.Amount
        };

        #region Card Number Validation
        var cardNumberAsString = request.CardNumber.ToString();

        if (cardNumberAsString.Length < 1)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.CardNumberNull);
            errorObj.ErrorMessages.Add("Enter a card number.");
        }

        if (cardNumberAsString.ToCharArray().Length is < 14 or > 19)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.CardNumberIncorrectLength);
            errorObj.ErrorMessages.Add("Card number needs to be be between 14 and 19 characters.");
        }
        
        var cardNumAsFour = cardNumberAsString.Substring(cardNumberAsString.Length - 4);
        response.CardNumberLastFour = int.Parse(cardNumAsFour);

        var regex = new Regex(@"^[0-9]+$");
        foreach (var character in cardNumberAsString)
        {
            if (!regex.IsMatch(character.ToString()))
            {
                errorObj.ValidationCodes.Add(ValidationCodes.CardNumberNotStrictlyNumeric);
                errorObj.ErrorMessages.Add("Card number can only contain numbers 0-9.");
            }
        }
        #endregion

        #region ExpiryDate
        if (request.ExpiryMonth is < 1 or > 12)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.ExpiryDateInvalidMonth);
            errorObj.ErrorMessages.Add("Enter a valid calendar month between 1-12.");
        }
        
        if (request.ExpiryYear < DateTime.Today.Year)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.ExpiryDateInvalidYear);
            errorObj.ErrorMessages.Add("Enter a year in the future or the current year.");
        }
        
        var requestDate = DateTime.Parse(request.ExpiryMonth + "/" + request.ExpiryYear);
        var todayDate = DateTime.Parse(DateTime.Today.Month + "/" + DateTime.Today.Year);
        
        if (DateTime.Compare(todayDate, requestDate) == 1) // In the past
        {
            errorObj.ValidationCodes.Add(ValidationCodes.ExpiryDateInThePast);
            errorObj.ErrorMessages.Add("Enter a date which is this month/year or in the future.");
        }

        #endregion

        #region Currency
        if (request.Currency.Length != 3)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.CurrencyInvalidFormatIso);
            errorObj.ErrorMessages.Add("Enter a valid 3 digit ISO code for the currency.");
        }

        var isoCode = request.Currency.ToUpper();

        var codeMatch = false;
        foreach (var code in Enum.GetValues<IsoCurrencyCodes>())
        {
            if (isoCode.Equals(code.ToString()))
            {
                codeMatch = true;
                break;
            }
        }

        if (!codeMatch)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.CurrencyUnknownIso);
            errorObj.ErrorMessages.Add("ISO code not recognised for eligible transaction.");
        }

        #endregion

        #region CVV
        var cvvAsString = request.Cvv.ToString();
        if (cvvAsString.ToCharArray().Length is < 3 or > 4)
        {
            errorObj.ValidationCodes.Add(ValidationCodes.CvvInvalidLength);
            errorObj.ErrorMessages.Add("CVV needs to be between 3 and 4 characters.");
        }
        
        foreach (var character in cardNumberAsString)
        {
            if (!regex.IsMatch(character.ToString()))
            {
                errorObj.ValidationCodes.Add(ValidationCodes.CvvNotStrictlyNumeric);
                errorObj.ErrorMessages.Add("CVV can only contain numbers 0-9.");
            }
        }

        #endregion

        var json = JsonConvert.SerializeObject(errorObj);
        errorJObj = JObject.Parse(json);
        
        // Failed validation
        if (errorObj.ValidationCodes.Count > 0)
        {
            return response;
        }

        // Passed all validation
        errorObj.Status = 200;
        JsonConvert.SerializeObject(errorObj);
        
        response.Status = PaymentStatus.Authorized;
        _paymentResponses.Add(response);
        
        return response;
    }
}