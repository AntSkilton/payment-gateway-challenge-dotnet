namespace PaymentGateway.Api.Models;

public enum ValidationCodes
{
    CardNumberNull,
    CardNumberIncorrectLength,
    CardNumberNotStrictlyNumeric,
    ExpiryDateInvalidMonth,
    ExpiryDateInvalidYear,
    ExpiryDateInThePast,
    CurrencyInvalidFormatIso,
    CurrencyUnknownIso,
    CvvInvalidLength,
    CvvNotStrictlyNumeric,
}