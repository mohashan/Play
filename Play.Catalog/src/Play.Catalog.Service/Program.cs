using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

ServiceSettings serviceSettings = builder.
    Configuration.
    GetSection(nameof(ServiceSettings)).
    Get<ServiceSettings>()
    ?? new ServiceSettings()
    {
        ServiceName = "Catalog"
    };

builder.Services.AddSingleton<IItemsRepository, ItemsRepository>();

builder.Services.AddSingleton(serviceProvider =>
{
    var mongoDbSettings = builder.
    Configuration.
    GetSection(nameof(MongoDbSettings)).
    Get<MongoDbSettings>()
    ?? new MongoDbSettings()
    {
        Host = "localhost",
        Port = "27017"
    };
    MongoClient mongoClient = new MongoClient(mongoDbSettings.ConnectionString);

    return mongoClient.GetDatabase(serviceSettings.ServiceName);
});

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
builder.Services.AddSwaggerGen(c=>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Catalog.Service", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c=>c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Catalog.Service v1"));
}

// to make certificate truted : dotnet dev-certs https --trust

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
