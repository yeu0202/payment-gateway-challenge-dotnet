using System.Net;

using AutoFixture.Xunit2;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentsRepository> _mockPaymentsRepository = new();
    private readonly Mock<IBankClient> _mockBankClient = new();
    private readonly Mock<IOptions<CurrencyCodes>> _mockCurrencyCodes = new();
    private readonly PaymentsController _paymentsController;

    private readonly PostPaymentRequest _postPaymentRequest = new PostPaymentRequest
    {
        CardNumber = "1234098712340987",
        ExpiryMonth = 12,
        ExpiryYear = 2030,
        Currency = "GBP",
        Amount = 100,
        Cvv = 123
    };

    private GetPaymentResponse _getPaymentResponse = new GetPaymentResponse
    {
        Id = Guid.NewGuid(),
        Amount = 100,
        CardNumberLastFour = 1234,
        Currency = "GBP",
        ExpiryMonth = 12,
        ExpiryYear = 2030,
        Status = "Authorized"
    };

    public PaymentsControllerTests()
    {
        _paymentsController = new PaymentsController(_mockPaymentsRepository.Object, _mockBankClient.Object,
            _mockCurrencyCodes.Object);
    }

    private void Setup(GetPaymentResponse? paymentResponse = null,
        PaymentStatus paymentStatus = PaymentStatus.Authorized)
    {
        _mockPaymentsRepository
            .Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResponse);

        _mockBankClient
            .Setup(x => x.PostPayment(It.IsAny<PostPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentStatus);

        _mockCurrencyCodes
            .Setup(x => x.Value)
            .Returns(new CurrencyCodes { Codes = new List<string>() { "GBP", "USD", "EUR" } });
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsOkResult_WhenValidPaymentIsSent()
    {
        // Arrange
        Setup();

        // Act
        var result = await _paymentsController.PostPaymentAsync(_postPaymentRequest, default);

        var expectedResult = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            CardNumberLastFour = int.Parse(_postPaymentRequest.CardNumber[^4..]),
            Currency = _postPaymentRequest.Currency,
            ExpiryMonth = _postPaymentRequest.ExpiryMonth,
            ExpiryYear = _postPaymentRequest.ExpiryYear,
            Amount = _postPaymentRequest.Amount,
        };

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        Assert.NotNull(result.Value);
        var resultValue = result.Value!;
        Assert.Equal(resultValue.Status, expectedResult.Status);
        Assert.Equal(resultValue.CardNumberLastFour, expectedResult.CardNumberLastFour);
        Assert.Equal(resultValue.Currency, expectedResult.Currency);
        Assert.Equal(resultValue.ExpiryMonth, expectedResult.ExpiryMonth);
        Assert.Equal(resultValue.ExpiryYear, expectedResult.ExpiryYear);
        Assert.Equal(resultValue.Amount, expectedResult.Amount);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123478901234798012347890")]
    [InlineData("12345678901234A")]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidCardNumberIsGiven(string cardNumber)
    {
        
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(13)]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidMonthIsGiven(int month)
    {
        
    }

    [Theory]
    [InlineData(3, 2020)]
    [InlineData(12, 2024)]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenExpiredDateIsGiven(int month, int year)
    {
        
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidCurrencyIsGiven()
    {
        
    }

    [Theory]
    [InlineData(-111)]
    [InlineData(99)]
    [InlineData(12345)]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidCvvIsGiven(int cvv)
    {
        
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsAuthorizedStatus_WhenValidPaymentIsSent()
    {
        
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsDeclineStatus_WhenBankDeclinesCard()
    {
        
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsRejectedStatus_WhenBankRejectsCard()
    {
        
    }

    [Fact]
    public async Task GetPaymentAsync_ReturnsPayment_WhenPaymentExists()
    {
        
    }

    [Theory, AutoData]
    public async Task GetPaymentAsync_ReturnsNotFound_IfPaymentDoesNotExist(Guid paymentId)
    {
        // Arrange
        Setup();

        // Act
        var result = await _paymentsController.GetPaymentAsync(paymentId, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, GetStatusCode(result));
    }

    private static int? GetStatusCode<T>(ActionResult<T?> actionResult)
    {
        IConvertToActionResult convertToActionResult = actionResult; // ActionResult implements IConvertToActionResult
        var actionResultWithStatusCode = convertToActionResult.Convert() as IStatusCodeActionResult;
        return actionResultWithStatusCode?.StatusCode;
    }
}