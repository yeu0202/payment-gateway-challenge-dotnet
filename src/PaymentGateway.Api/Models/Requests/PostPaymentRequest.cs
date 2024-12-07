﻿using JetBrains.Annotations;

namespace PaymentGateway.Api.Models.Requests;

[PublicAPI]
public class PostPaymentRequest
{
    public required string CardNumber { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public required string Currency { get; init; }
    public int Amount { get; init; }
    public required string Cvv { get; init; }
}