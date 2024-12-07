using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("currencyCodes.json");
builder.Configuration.AddJsonFile("appsettings.json", false);
builder.Configuration.AddJsonFile($"appsettings{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
    true);
builder.Services.Configure<BankConfig>(builder.Configuration.GetRequiredSection(BankConfig.Name));
builder.Services.Configure<CurrencyCodes>(builder.Configuration.GetRequiredSection(CurrencyCodes.Name));

builder.Services.AddHttpClient();
builder.Services.AddTransient<IBankClient, BankClient>();
builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();