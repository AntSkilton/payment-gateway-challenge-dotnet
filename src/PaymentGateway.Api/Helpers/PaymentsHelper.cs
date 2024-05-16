using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Helpers;

public static class PaymentsHelper
{
    public const string c_StaticGuidToTest = "33e3091a-7f72-4fa8-80e6-17a1bb80f0d0";
    public static List<PaymentResponse> GeneratePaymentStubs()
    {
        var stubResponses = new List<PaymentResponse>()
        {
            new PaymentResponse
            {
                Id = Guid.Parse(c_StaticGuidToTest),
                Status = PaymentStatus.Authorized,
                CardNumberLastFour = 5678,
                ExpiryMonth = 6,
                ExpiryYear = 2025,
                Currency = "GBP",
                Amount = 1000
            },
            new PaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = 3925,
                ExpiryMonth = 2,
                ExpiryYear = 2029,
                Currency = "USD",
                Amount = 19
            },
            new PaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Declined,
                CardNumberLastFour = 4293,
                ExpiryMonth = 9,
                ExpiryYear = 2032,
                Currency = "EUR",
                Amount = 43890
            },
        };

        return stubResponses;
    }
}