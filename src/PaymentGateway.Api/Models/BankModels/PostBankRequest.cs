using JetBrains.Annotations;

namespace PaymentGateway.Api.Models.BankModels;

[PublicAPI]
public class PostBankRequest
{
    public required string CardNumber { get; init; }
    public required string ExpiryDate { get; init; }
    public required string Currency { get; init; }
    public int Amount { get; init; }
    public int Cvv { get; init; }
}