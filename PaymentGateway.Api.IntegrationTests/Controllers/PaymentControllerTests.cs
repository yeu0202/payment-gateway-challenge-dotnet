using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.BankModels;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.IntegrationTests.Controllers;

public class PaymentControllerTests
{
    private readonly Random _random = new();

    private const int AuthorizedMonth = 4;
    private const int AuthorizedYear = 2025;

    private readonly PostBankRequest _authorizedPostBankRequest = new()
    {
        CardNumber = "2222405343248877",
        ExpiryDate = "04/2025",
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };

    private const int UnauthorizedMonth = 1;
    private const int UnauthorizedYear = 2026;

    private readonly PostBankRequest _unauthorizedPostBankRequest = new()
    {
        CardNumber = "2222405343248112",
        ExpiryDate = "01/2026",
        Currency = "USD",
        Amount = 60000,
        Cvv = "456"
    };

    private static HttpClient Setup()
    {
        var paymentsRepository = new PaymentsRepository();
        var httpClient = new HttpClient();
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BankClient>();
        var bankClient = new BankClient(httpClient,
            new OptionsWrapper<BankConfig>(new BankConfig { BankUrl = "http://localhost:8080" }), logger);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    services
                        .AddTransient<IBankClient>(_ => bankClient)
                        .AddSingleton(typeof(IPaymentsRepository), paymentsRepository)
                )
            )
            .CreateClient();

        return client;
    }

    [Fact]
    public async Task GetPaymentAsync_RetrievesAPaymentSuccessfully_WhenPaymentExists()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999).ToString(),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    services
                        .AddSingleton(typeof(IPaymentsRepository), paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsAuthorizedResponse_WhenPaymentIsAuthorized()
    {
        // Arrange
        var client = Setup();
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = _authorizedPostBankRequest.CardNumber,
            ExpiryMonth = AuthorizedMonth,
            ExpiryYear = AuthorizedYear,
            Currency = _authorizedPostBankRequest.Currency,
            Amount = _authorizedPostBankRequest.Amount,
            Cvv = _authorizedPostBankRequest.Cvv
        };
        var expectedResult = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            CardNumberLastFour = _authorizedPostBankRequest.CardNumber[^4..],
            ExpiryMonth = AuthorizedMonth,
            ExpiryYear = AuthorizedYear,
            Currency = _authorizedPostBankRequest.Currency,
            Amount = _authorizedPostBankRequest.Amount
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/PostPayment", paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(responseContent);
        AssertExtensions.Equal(expectedResult, responseContent!);
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsDeclinedResponse_WhenPaymentIsDeclined()
    {
        // Arrange
        var client = Setup();
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = _unauthorizedPostBankRequest.CardNumber,
            ExpiryMonth = UnauthorizedMonth,
            ExpiryYear = UnauthorizedYear,
            Currency = _unauthorizedPostBankRequest.Currency,
            Amount = _unauthorizedPostBankRequest.Amount,
            Cvv = _unauthorizedPostBankRequest.Cvv
        };
        var expectedResult = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Declined.ToString(),
            CardNumberLastFour = _unauthorizedPostBankRequest.CardNumber[^4..],
            ExpiryMonth = UnauthorizedMonth,
            ExpiryYear = UnauthorizedYear,
            Currency = _unauthorizedPostBankRequest.Currency,
            Amount = _unauthorizedPostBankRequest.Amount
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/PostPayment", paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(responseContent);
        AssertExtensions.Equal(expectedResult, responseContent!);
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsRejectedResponse_WhenPaymentIsRejected()
    {
        // Arrange
        var client = Setup();
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 1,
            ExpiryYear = 2300,
            Currency = "GBP",
            Amount = 13579,
            Cvv = "123"
        };
        var expectedResult = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Rejected.ToString(),
            CardNumberLastFour = paymentRequest.CardNumber[^4..],
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/PostPayment", paymentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(responseContent);
        AssertExtensions.Equal(expectedResult, responseContent!);
    }
}