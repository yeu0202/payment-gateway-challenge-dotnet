using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    public void Add(PostPaymentResponse paymentResponse);
    public Task<GetPaymentResponse?> Get(Guid id, CancellationToken cancellationToken);
}

public class PaymentsRepository : IPaymentsRepository
{
    // This is the database
    private readonly List<GetPaymentResponse> _payments = [];

    public void Add(PostPaymentResponse paymentResponse)
    {
        var paymentRecord = new GetPaymentResponse
        {
            Id = paymentResponse.Id,
            Status = paymentResponse.Status,
            CardNumberLastFour = paymentResponse.CardNumberLastFour,
            ExpiryMonth = paymentResponse.ExpiryMonth,
            ExpiryYear = paymentResponse.ExpiryYear,
            Currency = paymentResponse.Currency,
            Amount = paymentResponse.Amount
        };

        _payments.Add(paymentRecord);
    }

    public async Task<GetPaymentResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        return _payments.FirstOrDefault(p => p.Id == id);
    }
}