using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.





//ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>() ?? new ServiceSettings();

builder.Services.AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryItems");

builder.Services.AddHttpClient<CatalogClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:7065");
});

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
