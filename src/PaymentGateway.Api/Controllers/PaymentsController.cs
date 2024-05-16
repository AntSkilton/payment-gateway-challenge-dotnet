using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;

    public PaymentsController(PaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }

    // GET api/Payments/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.GetPayment(id);

        return new OkObjectResult(payment);
    }
    
    // For demonstration purposes.
    [HttpGet]
    public async Task<ActionResult<PostPaymentResponse?>> GetAllPaymentsAsync()
    {
        return new OkObjectResult(_paymentsRepository.GetAllPayments());
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse?>> PostNewPaymentAsync(PostPaymentRequest request)
    {
        var response = _paymentsRepository.ProcessPayment(request, out JObject errorJObj);
        
        if (response.Status == PaymentStatus.Rejected)
        {
            return new BadRequestObjectResult(errorJObj.ToString());
        }

        return new OkObjectResult(response);
    }
}