using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Services;

public class BankClientTests
{
    private readonly Mock<HttpClientHandler> _mockHttpClientHandler = new();
    private readonly BankClient _bankClient;

    public BankClientTests()
    {
        _bankClient = new BankClient(new HttpClient(_mockHttpClientHandler.Object),
            new OptionsWrapper<BankConfig>(new BankConfig { BankUrl = "http://test" }));
    }

    [Fact]
    public async Task TestGetBanksAsync()
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        _mockHttpClientHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        
    }
}