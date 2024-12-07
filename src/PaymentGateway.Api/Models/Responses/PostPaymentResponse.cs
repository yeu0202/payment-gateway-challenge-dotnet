using JetBrains.Annotations;

namespace PaymentGateway.Api.Models.Responses;

[PublicAPI]
public class PostPaymentResponse
{
    public Guid Id { get; init; }
    public required string Status { get; init; }
    public int CardNumberLastFour { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public required string Currency { get; init; }
    public int Amount { get; init; }
}