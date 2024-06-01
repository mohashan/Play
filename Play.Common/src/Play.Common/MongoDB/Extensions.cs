using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Play.Common.Entities;
using Play.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Play.Common.Repositories;

namespace Play.Common.MongoDB;

public static class Extensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            ServiceSettings serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()?? new ServiceSettings();

            var mongoDbSettings = configuration?.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>() ?? new MongoDbSettings();

            MongoClient mongoClient = new MongoClient(mongoDbSettings.ConnectionString);

            return mongoClient.GetDatabase(serviceSettings.ServiceName);
        });



        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        return services;
    }

    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services,string collectionName)
        where T : IEntity
    {
        services.AddSingleton<IRepository<T>>(serviceProvider =>
        {
            var database = serviceProvider.GetService<IMongoDatabase>()!;
            return new MongoRepository<T>(database, collectionName);
        });

        return services;
    }
}
