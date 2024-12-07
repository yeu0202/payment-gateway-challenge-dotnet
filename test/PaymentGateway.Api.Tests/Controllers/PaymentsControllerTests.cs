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

    private readonly PostPaymentRequest _postPaymentRequest = new()
    {
        CardNumber = "1234098712340987",
        ExpiryMonth = 12,
        ExpiryYear = 2030,
        Currency = "GBP",
        Amount = 100,
        Cvv = 123
    };

    private readonly GetPaymentResponse _getPaymentResponse = new()
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
        Setup();
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
            .Returns(new CurrencyCodes { Codes = new List<string> { "GBP", "USD", "EUR" } });
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
            Amount = _postPaymentRequest.Amount
        };

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((OkObjectResult)result.Result!).Value);
        var resultValue = (PostPaymentResponse)((OkObjectResult)result.Result).Value!;
        Assert.Equal(expectedResult.Status, resultValue.Status);
        Assert.Equal(expectedResult.CardNumberLastFour, resultValue.CardNumberLastFour);
        Assert.Equal(expectedResult.ExpiryMonth, resultValue.ExpiryMonth);
        Assert.Equal(expectedResult.ExpiryYear, resultValue.ExpiryYear);
        Assert.Equal(expectedResult.Currency, resultValue.Currency);
        Assert.Equal(expectedResult.Amount, resultValue.Amount);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123478901234798012347890")]
    [InlineData("12345678901234A")]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidCardNumberIsGiven(string cardNumber)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = _postPaymentRequest.ExpiryMonth,
            ExpiryYear = _postPaymentRequest.ExpiryYear,
            Currency = _postPaymentRequest.Currency,
            Amount = _postPaymentRequest.Amount,
            Cvv = _postPaymentRequest.Cvv
        };

        // Act
        var result = await _paymentsController.PostPaymentAsync(paymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((BadRequestObjectResult)result.Result!).Value);
        Assert.Equal("Invalid card number", ((BadRequestObjectResult)result.Result!).Value!.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(13)]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidMonthIsGiven(int month)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = _postPaymentRequest.CardNumber,
            ExpiryMonth = month,
            ExpiryYear = _postPaymentRequest.ExpiryYear,
            Currency = _postPaymentRequest.Currency,
            Amount = _postPaymentRequest.Amount,
            Cvv = _postPaymentRequest.Cvv
        };

        // Act
        var result = await _paymentsController.PostPaymentAsync(paymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((BadRequestObjectResult)result.Result!).Value);
        Assert.Equal("Invalid expiry month", ((BadRequestObjectResult)result.Result!).Value!.ToString());
    }

    [Theory]
    [InlineData(3, 2020)]
    [InlineData(12, 2024)]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenExpiredDateIsGiven(int month, int year)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = _postPaymentRequest.CardNumber,
            ExpiryMonth = month,
            ExpiryYear = year,
            Currency = _postPaymentRequest.Currency,
            Amount = _postPaymentRequest.Amount,
            Cvv = _postPaymentRequest.Cvv
        };

        // Act
        var result = await _paymentsController.PostPaymentAsync(paymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((BadRequestObjectResult)result.Result!).Value);
        Assert.Equal("Card is expired", ((BadRequestObjectResult)result.Result!).Value!.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("GBPGBP")]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidCurrencyIsGiven(string currency)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = _postPaymentRequest.CardNumber,
            ExpiryMonth = _postPaymentRequest.ExpiryMonth,
            ExpiryYear = _postPaymentRequest.ExpiryYear,
            Currency = currency,
            Amount = _postPaymentRequest.Amount,
            Cvv = _postPaymentRequest.Cvv
        };

        // Act
        var result = await _paymentsController.PostPaymentAsync(paymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((BadRequestObjectResult)result.Result!).Value);
        Assert.Equal("Invalid currency", ((BadRequestObjectResult)result.Result!).Value!.ToString());
    }

    [Theory]
    [InlineData(-111)]
    [InlineData(99)]
    [InlineData(12345)]
    public async Task PostPaymentAsync_ReturnsBadRequest_WhenInvalidCvvIsGiven(int cvv)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = _postPaymentRequest.CardNumber,
            ExpiryMonth = _postPaymentRequest.ExpiryMonth,
            ExpiryYear = _postPaymentRequest.ExpiryYear,
            Currency = _postPaymentRequest.Currency,
            Amount = _postPaymentRequest.Amount,
            Cvv = cvv
        };

        // Act
        var result = await _paymentsController.PostPaymentAsync(paymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((BadRequestObjectResult)result.Result!).Value);
        Assert.Equal("Invalid CVV", ((BadRequestObjectResult)result.Result!).Value!.ToString());
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsAuthorizedStatus_WhenValidPaymentIsSent()
    {
        // Arrange
        Setup(paymentStatus: PaymentStatus.Authorized);
        
        // Act
        var result = await _paymentsController.PostPaymentAsync(_postPaymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((OkObjectResult)result.Result!).Value);
        var resultValue = (PostPaymentResponse)((OkObjectResult)result.Result).Value!;
        Assert.Equal(PaymentStatus.Authorized.ToString(), resultValue.Status);
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsDeclineStatus_WhenBankDeclinesCard()
    {
        // Arrange
        Setup(paymentStatus: PaymentStatus.Declined);
        
        // Act
        var result = await _paymentsController.PostPaymentAsync(_postPaymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((OkObjectResult)result.Result!).Value);
        var resultValue = (PostPaymentResponse)((OkObjectResult)result.Result).Value!;
        Assert.Equal(PaymentStatus.Declined.ToString(), resultValue.Status);
    }

    [Fact]
    public async Task PostPaymentAsync_ReturnsRejectedStatus_WhenBankRejectsCard()
    {
        // Arrange
        Setup(paymentStatus: PaymentStatus.Rejected);
        
        // Act
        var result = await _paymentsController.PostPaymentAsync(_postPaymentRequest, default);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((OkObjectResult)result.Result!).Value);
        var resultValue = (PostPaymentResponse)((OkObjectResult)result.Result).Value!;
        Assert.Equal(PaymentStatus.Rejected.ToString(), resultValue.Status);
    }

    [Theory, AutoData]
    public async Task GetPaymentAsync_ReturnsPayment_WhenPaymentExists(Guid paymentId)
    {
        // Arrange
        Setup(paymentResponse: _getPaymentResponse);
        
        // Act
        var result = await _paymentsController.GetPaymentAsync(paymentId, default);
        
        // Assert
        Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(result));
        Assert.NotNull(result.Result);
        Assert.NotNull(((OkObjectResult)result.Result!).Value);
        var resultValue = (GetPaymentResponse)((OkObjectResult)result.Result).Value!;
        Assert.Equal(_getPaymentResponse.Status, resultValue.Status);
        Assert.Equal(_getPaymentResponse.CardNumberLastFour, resultValue.CardNumberLastFour);
        Assert.Equal(_getPaymentResponse.ExpiryMonth, resultValue.ExpiryMonth);
        Assert.Equal(_getPaymentResponse.ExpiryYear, resultValue.ExpiryYear);
        Assert.Equal(_getPaymentResponse.Currency, resultValue.Currency);
        Assert.Equal(_getPaymentResponse.Amount, resultValue.Amount);
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