namespace Play.Common.Settings;

public class MongoDbSettings
{
    public string Host { get; init; } = default!;
    public string Port { get; init; } = default!;

    public string ConnectionString => $"mongodb://{Host}:{Port}";
}
