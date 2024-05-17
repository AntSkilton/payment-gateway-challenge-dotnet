using System.Net;
using System.Net.Http.Json;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

    private PaymentRequest _validPaymentRequest;

    public PaymentsControllerTests()
    {
        var randomCardNum = "";
        for (int i = 0; i < 16; i++)
        {
            randomCardNum += _random.Next(0, 9);
        }

        _validPaymentRequest = new PaymentRequest
        {
            CardNumber = long.Parse(randomCardNum),
            ExpiryMonth = _random.Next(DateTime.Today.Month, 12),
            ExpiryYear = _random.Next(DateTime.Today.Year, DateTime.Today.Year + 10),
            Currency = "GBP",
            Amount = _random.Next(1, 10000),
            Cvv = _random.Next(000,999)
        };
    }

    [Fact]
    public async Task RetrieveAPayment_Successfully()
    {
        // Arrange
        var payment = new PaymentResponse
        {
            Id = Guid.Parse(PaymentsHelper.c_StaticGuidToTest),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.GetPayment(payment.Id);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns204IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddSingleton(paymentsRepository)))
            .CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CardNumberIsEmpty()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.CardNumber = 0;
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.CardNumberNull, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task CardNumberIsTooShort()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.CardNumber = 12345678;
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.CardNumberIncorrectLength, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task ExpiryDateHasInvalidMonth()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.ExpiryMonth = 13;
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.ExpiryDateInvalidMonth, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task ExpiryDateHasInvalidYear()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.ExpiryYear = 2020;
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.ExpiryDateInvalidYear, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task ExpiryDateHasInThePast()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.ExpiryMonth = 6;
        paymentRequest.ExpiryYear = 2020;
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.ExpiryDateInvalidYear, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task CurrencyHasInvalidIsoFormat()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.Currency = "GBBP";
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.CurrencyInvalidFormatIso, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task CurrencyHasUnknownIsoFormat()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.Currency = "CAD";
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.CurrencyUnknownIso, errorObj.ValidationCodes);
        }
    }
    
    [Fact]
    public async Task CvvHasInvalidLength()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var paymentsRepository = new PaymentsRepository();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository))).CreateClient();

        var paymentRequest = new PaymentsControllerTests()._validPaymentRequest;
        paymentRequest.Cvv = 12345;
        
        var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);
        var responseBody = await response.Content.ReadAsStringAsync(); 
        var errorObj = JsonConvert.DeserializeObject<ErrorObj>(responseBody);

        // Assert
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Assert.Contains(ValidationCodes.CvvInvalidLength, errorObj.ValidationCodes);
        }
    }
} 