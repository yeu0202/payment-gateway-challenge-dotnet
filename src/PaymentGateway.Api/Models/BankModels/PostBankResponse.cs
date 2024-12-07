using JetBrains.Annotations;

namespace PaymentGateway.Api.Models.BankModels;

[PublicAPI]
public class PostBankResponse
{
    public bool Authorized { get; init; }

    // ReSharper disable once InconsistentNaming
    public required string Authorization_Code { get; init; }
}