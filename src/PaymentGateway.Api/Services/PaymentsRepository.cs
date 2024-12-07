using System.Collections.Concurrent;

using PaymentGateway.Api.Models;
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
    private readonly ConcurrentDictionary<Guid, PaymentRecord> _payments = [];

    public void Add(PostPaymentResponse paymentResponse)
    {
        var paymentRecord = new PaymentRecord
        {
            Status = paymentResponse.Status,
            CardNumberLastFour = paymentResponse.CardNumberLastFour,
            ExpiryMonth = paymentResponse.ExpiryMonth,
            ExpiryYear = paymentResponse.ExpiryYear,
            Currency = paymentResponse.Currency,
            Amount = paymentResponse.Amount
        };

        _payments.TryAdd(paymentResponse.Id, paymentRecord);
    }

    public async Task<GetPaymentResponse?> Get(Guid id, CancellationToken cancellationToken)
    {
        var isRecordAvailable = _payments.TryGetValue(id, out var storedRecord);
        if (!isRecordAvailable || storedRecord == null)
            return null;
        
        return new GetPaymentResponse()
        {
            Id = id,
            Status = storedRecord.Status,
            CardNumberLastFour = storedRecord.CardNumberLastFour,
            ExpiryMonth = storedRecord.ExpiryMonth,
            ExpiryYear = storedRecord.ExpiryYear,
            Currency = storedRecord.Currency,
            Amount = storedRecord.Amount
        };
    }
}