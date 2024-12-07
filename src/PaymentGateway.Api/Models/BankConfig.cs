namespace PaymentGateway.Api.Models;

public class BankConfig
{
    public static string Name => "BankConfig";
    public required string BankUrl { get; init; }
}