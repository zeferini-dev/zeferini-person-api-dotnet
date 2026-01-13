using Npgsql;
using System.Text.Json;
using ZeferiniPersonApi.Models;

namespace ZeferiniPersonApi.Services;

public interface IEventsService
{
    Task<Event> PublishEventAsync(EventPayload payload);
    Task<List<Event>> GetEventsByAggregateIdAsync(string aggregateId);
    Task<List<Event>> GetEventsByAggregateTypeAsync(string aggregateType);
}

public class EventPayload
{
    public required string AggregateId { get; set; }
    public required string AggregateType { get; set; }
    public required string EventType { get; set; }
    public required Dictionary<string, object> EventData { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
}

public class EventsService : IEventsService, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<EventsService> _logger;

    public EventsService(ILogger<EventsService> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("EVENTS_DATABASE_URL") 
            ?? "Host=localhost;Port=5433;Database=eventstore;Username=events;Password=events123";
        
        // Convert from URI format if needed
        if (connectionString.StartsWith("postgresql://"))
        {
            connectionString = ConvertConnectionString(connectionString);
        }

        _logger.LogInformation("Connecting to events database");
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    private static string ConvertConnectionString(string uri)
    {
        var uriObj = new Uri(uri);
        var userInfo = uriObj.UserInfo.Split(':');
        return $"Host={uriObj.Host};Port={uriObj.Port};Database={uriObj.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
    }

    public async Task<Event> PublishEventAsync(EventPayload payload)
    {
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = payload.AggregateId,
            AggregateType = payload.AggregateType,
            EventType = payload.EventType,
            EventData = payload.EventData,
            Metadata = payload.Metadata ?? new Dictionary<string, object?>(),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await SaveEventToDatabaseAsync(eventEntity);
        return eventEntity;
    }

    private async Task SaveEventToDatabaseAsync(Event eventEntity)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO eventstore.events 
            (id, ""aggregateId"", ""aggregateType"", ""eventType"", ""eventData"", metadata, version, timestamp, ""createdAt"")
            VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)";

        command.Parameters.AddWithValue(eventEntity.Id);
        command.Parameters.AddWithValue(eventEntity.AggregateId);
        command.Parameters.AddWithValue(eventEntity.AggregateType);
        command.Parameters.AddWithValue(eventEntity.EventType);
        command.Parameters.AddWithValue(JsonSerializer.Serialize(eventEntity.EventData));
        command.Parameters.AddWithValue(JsonSerializer.Serialize(eventEntity.Metadata));
        command.Parameters.AddWithValue(eventEntity.Version);
        command.Parameters.AddWithValue(eventEntity.Timestamp);
        command.Parameters.AddWithValue(eventEntity.CreatedAt);

        await command.ExecuteNonQueryAsync();

        _logger.LogDebug("Event published: {EventType} for {AggregateType}/{AggregateId}",
            eventEntity.EventType, eventEntity.AggregateType, eventEntity.AggregateId);
    }

    public async Task<List<Event>> GetEventsByAggregateIdAsync(string aggregateId)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT id, ""aggregateId"", ""aggregateType"", ""eventType"", ""eventData"", 
                   metadata, version, timestamp, ""createdAt""
            FROM eventstore.events
            WHERE ""aggregateId"" = $1
            ORDER BY ""createdAt"" ASC";

        command.Parameters.AddWithValue(aggregateId);

        return await ReadEventsAsync(command);
    }

    public async Task<List<Event>> GetEventsByAggregateTypeAsync(string aggregateType)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT id, ""aggregateId"", ""aggregateType"", ""eventType"", ""eventData"", 
                   metadata, version, timestamp, ""createdAt""
            FROM eventstore.events
            WHERE ""aggregateType"" = $1
            ORDER BY ""createdAt"" DESC";

        command.Parameters.AddWithValue(aggregateType);

        return await ReadEventsAsync(command);
    }

    private static async Task<List<Event>> ReadEventsAsync(NpgsqlCommand command)
    {
        var events = new List<Event>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            events.Add(new Event
            {
                Id = reader.GetGuid(0),
                AggregateId = reader.GetString(1),
                AggregateType = reader.GetString(2),
                EventType = reader.GetString(3),
                EventData = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(4)) ?? new(),
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object?>>(reader.GetString(5)) ?? new(),
                Version = reader.GetInt32(6),
                Timestamp = reader.GetDateTime(7),
                CreatedAt = reader.GetDateTime(8)
            });
        }

        return events;
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }
}
