using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using Newtonsoft.Json;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.BankModels;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Services;

public class BankClientTests
{
    private readonly Mock<HttpClientHandler> _mockHttpClientHandler = new();
    private readonly BankClient _bankClient;

    private readonly PostPaymentRequest _postPaymentRequest = new()
    {
        CardNumber = "1234567890123456",
        ExpiryMonth = 12,
        ExpiryYear = 2021,
        Currency = "GBP",
        Amount = 1,
        Cvv = "123"
    };

    public BankClientTests()
    {
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BankClient>();
        _bankClient = new BankClient(new HttpClient(_mockHttpClientHandler.Object),
            new OptionsWrapper<BankConfig>(new BankConfig { BankUrl = "http://test" }), logger);
    }

    [Fact]
    public async Task PostPayment_ReturnsAuthorized_IfBankAuthorizesPayment()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        response.Content =
            new StringContent(JsonConvert.SerializeObject(new PostBankResponse
            {
                Authorized = true, Authorization_Code = "authorization_code"
            }));
        
        _mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _bankClient.PostPayment(_postPaymentRequest, default);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, result);
    }

    [Fact]
    public async Task PostPayment_ReturnsDeclined_IfBankDeclinesPayment()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        response.Content =
            new StringContent(JsonConvert.SerializeObject(new PostBankResponse
            {
                Authorized = false, Authorization_Code = "authorization_code"
            }));

        _mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _bankClient.PostPayment(_postPaymentRequest, default);

        // Assert
        Assert.Equal(PaymentStatus.Declined, result);
    }

    [Fact]
    public async Task PostPayment_ReturnsRejected_IfBankIsUnableToProcessPayment()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

        _mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _bankClient.PostPayment(_postPaymentRequest, default);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, result);
    }
}