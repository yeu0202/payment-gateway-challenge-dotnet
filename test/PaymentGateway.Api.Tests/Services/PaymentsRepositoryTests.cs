using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Services;

public class PaymentsRepositoryTests
{
    private readonly PostPaymentResponse _postPaymentResponse = new()
    {
        Id = Guid.NewGuid(),
        Status = PaymentStatus.Authorized.ToString(),
        CardNumberLastFour = "1234",
        ExpiryMonth = 12,
        ExpiryYear = 2021,
        Currency = "GBP",
        Amount = 100
    };
    
    [Fact]
    public async Task Get_ReturnsNull_WhenPaymentDoesNotExists()
    {
        // Arrange
        var paymentsRepository = new PaymentsRepository();
        
        // Act
        var result = await paymentsRepository.Get(_postPaymentResponse.Id, default);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task Get_ReturnsPaymentResponse_WhenPaymentExists()
    {
        // Arrange
        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(_postPaymentResponse);
        
        // Act
        var result = await paymentsRepository.Get(_postPaymentResponse.Id, default);
        var expectedResult = new GetPaymentResponse
        {
            Id = _postPaymentResponse.Id,
            Status = _postPaymentResponse.Status,
            CardNumberLastFour = _postPaymentResponse.CardNumberLastFour,
            ExpiryMonth = _postPaymentResponse.ExpiryMonth,
            ExpiryYear = _postPaymentResponse.ExpiryYear,
            Currency = _postPaymentResponse.Currency,
            Amount = _postPaymentResponse.Amount
        };
        
        // Assert
        Assert.NotNull(result);
        var nonNullResult = result!;
        Assert.Equal(expectedResult.Id, nonNullResult.Id);
        Assert.Equal(expectedResult.Status, nonNullResult.Status);
        Assert.Equal(expectedResult.CardNumberLastFour, nonNullResult.CardNumberLastFour);
        Assert.Equal(expectedResult.ExpiryMonth, nonNullResult.ExpiryMonth);
        Assert.Equal(expectedResult.ExpiryYear, nonNullResult.ExpiryYear);
        Assert.Equal(expectedResult.Currency, nonNullResult.Currency);
        Assert.Equal(expectedResult.Amount, nonNullResult.Amount);
    }

    [Fact]
    public async Task Add_AddsPaymentToRepository()
    {
        // Arrange
        var paymentsRepository = new PaymentsRepository();
        
        // Act & Assert
        var nullResult = await paymentsRepository.Get(_postPaymentResponse.Id, default);
        Assert.Null(nullResult);
        
        paymentsRepository.Add(_postPaymentResponse);
        var result = await paymentsRepository.Get(_postPaymentResponse.Id, default);
        var expectedResult = new GetPaymentResponse
        {
            Id = _postPaymentResponse.Id,
            Status = _postPaymentResponse.Status,
            CardNumberLastFour = _postPaymentResponse.CardNumberLastFour,
            ExpiryMonth = _postPaymentResponse.ExpiryMonth,
            ExpiryYear = _postPaymentResponse.ExpiryYear,
            Currency = _postPaymentResponse.Currency,
            Amount = _postPaymentResponse.Amount
        };
        
        Assert.NotNull(result);
        var nonNullResult = result!;
        Assert.Equal(expectedResult.Id, nonNullResult.Id);
        Assert.Equal(expectedResult.Status, nonNullResult.Status);
        Assert.Equal(expectedResult.CardNumberLastFour, nonNullResult.CardNumberLastFour);
        Assert.Equal(expectedResult.ExpiryMonth, nonNullResult.ExpiryMonth);
        Assert.Equal(expectedResult.ExpiryYear, nonNullResult.ExpiryYear);
        Assert.Equal(expectedResult.Currency, nonNullResult.Currency);
        Assert.Equal(expectedResult.Amount, nonNullResult.Amount);
    }
}