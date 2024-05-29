using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.





//ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>() ?? new ServiceSettings();

builder.Services.AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryItems");

Random jitter = new Random();

builder.Services.AddHttpClient<CatalogClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:7065");
})
    .AddTransientHttpErrorPolicy(b => b.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5, 
        retryAttemp => TimeSpan.FromSeconds(Math.Pow(2,retryAttemp)) + TimeSpan.FromMilliseconds(jitter.Next(0,1000)),
        onRetry: (outcome,timespan,retryAttempt) =>
        {
            Console.WriteLine($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
        }
    ))
    .AddTransientHttpErrorPolicy(b => b.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            Console.WriteLine($"Breaking Circuit for {timespan.TotalSeconds} seconds");
        },
        onReset: () => Console.WriteLine($"Closing the Circuit ... ")
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(4));

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
}

// to make certificate truted : dotnet dev-certs https --trust

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
