namespace PaymentGateway.Api.Models.Common;

public class ErrorObj
{
    public short Status { get; set; }
    public List<ValidationCodes> ValidationCodes { get; set; }
    public List<string> ErrorMessages { get; set; }
}