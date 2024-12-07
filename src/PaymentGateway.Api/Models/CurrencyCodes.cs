namespace PaymentGateway.Api.Models;

public class CurrencyCodes
{
    public static string Name => "CurrencyCodes";
    public required IReadOnlyCollection<string> Codes { get; init; }
}