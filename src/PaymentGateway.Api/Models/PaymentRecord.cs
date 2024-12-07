namespace PaymentGateway.Api.Models;

public class PaymentRecord
{
    public required string Status { get; init; }
    public required string CardNumberLastFour { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public required string Currency { get; init; }
    public int Amount { get; init; }
}