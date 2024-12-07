using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(
    IPaymentsRepository paymentsRepository,
    IBankClient bankClient,
    IOptions<CurrencyCodes> currencyCodes)
    : Controller
{
    private readonly CurrencyCodes _currencyCodes = currencyCodes.Value;

    [HttpPost("PostPayment")]
    public async Task<ActionResult<PostPaymentResponse?>> PostPaymentAsync([FromBody] PostPaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.CardNumber.Length is > 19 or < 14)
            return BadRequest("Invalid card number");
        try
        {
            _ = ulong.Parse(request.CardNumber);
        }
        catch (Exception)
        {
            return BadRequest("Invalid card number");
        }

        if (request.ExpiryMonth is < 1 or > 12)
            return BadRequest("Invalid expiry month");

        var currentDate = DateTime.UtcNow.Date;
        var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1);
        if (expiryDate <= currentDate)
            return BadRequest("Card is expired");

        if (!_currencyCodes.Codes.Contains(request.Currency))
            return BadRequest("Invalid currency");

        if (request.Cvv.Length is > 4 or < 3 || int.Parse(request.Cvv) < 0)
            return BadRequest("Invalid CVV");

        var guid = Guid.NewGuid();

        var bankResponse = await bankClient.PostPayment(request, cancellationToken);

        // Creating the response here so we can pass it to the payments repository
        var response = new PostPaymentResponse
        {
            Id = guid,
            Status = bankResponse.ToString(),
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        paymentsRepository.Add(response);

        return new OkObjectResult(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await paymentsRepository.Get(id, cancellationToken);

        if (payment == null)
            return NotFound("Payment not found");

        return new OkObjectResult(payment);
    }
}