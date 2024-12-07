using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.IntegrationTests;

public abstract class AssertExtensions : Assert
{
    public static void Equal(PostPaymentResponse paymentResponseExpected, PostPaymentResponse paymentResponse)
    {
        Assert.Equal(paymentResponseExpected.Status, paymentResponse.Status);
        Assert.Equal(paymentResponseExpected.CardNumberLastFour, paymentResponse.CardNumberLastFour);
        Assert.Equal(paymentResponseExpected.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(paymentResponseExpected.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(paymentResponseExpected.Currency, paymentResponse.Currency);
        Assert.Equal(paymentResponseExpected.Amount, paymentResponse.Amount);
    }
}