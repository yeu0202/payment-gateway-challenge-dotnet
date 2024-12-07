using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.BankModels;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IBankClient
{
    public Task<PaymentStatus> PostPayment(PostPaymentRequest request, CancellationToken cancellationToken);
}

public class BankClient(HttpClient httpClient, IOptions<BankConfig> options, ILogger<BankClient> logger) : IBankClient
{
    public async Task<PaymentStatus> PostPayment(PostPaymentRequest request, CancellationToken cancellationToken)
    {
        var month = request.ExpiryMonth.ToString();
        if (month.Length != 2)
            month = "0" + month;
        var bankRequest = new PostBankRequest
        {
            Amount = request.Amount,
            CardNumber = request.CardNumber,
            Currency = request.Currency,
            Cvv = request.Cvv,
            ExpiryDate = month + "/" + request.ExpiryYear
        };

        var bankResponse = await SendPostRequest(bankRequest, cancellationToken);

        return bankResponse;
    }

    private async Task<PaymentStatus> SendPostRequest(PostBankRequest request, CancellationToken cancellationToken)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                card_number = request.CardNumber,
                expiry_date = request.ExpiryDate,
                currency = request.Currency,
                amount = request.Amount,
                cvv = request.Cvv
            }),
            Encoding.UTF8,
            "application/json");

        httpClient.BaseAddress = new Uri(options.Value.BankUrl);
        try
        {
            var response = await httpClient.PostAsync("payments", jsonContent, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadFromJsonAsync<PostBankResponse>(cancellationToken: cancellationToken);

            return responseJson is { Authorized: true } ? PaymentStatus.Authorized : PaymentStatus.Declined;
        }
        catch (Exception ex)
        {
            logger.LogCritical("Bank request ended in failure with {Exception}", ex.Message);
            return PaymentStatus.Rejected;
        }
    }
}