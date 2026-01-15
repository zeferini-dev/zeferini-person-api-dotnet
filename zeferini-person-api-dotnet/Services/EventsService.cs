using ZeferiniPersonApi.Data;
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
    private readonly EventsDbContext _dbContext;
    private readonly ILogger<EventsService> _logger;

    public EventsService(EventsDbContext dbContext, ILogger<EventsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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

        _dbContext.Events.Add(eventEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogDebug("Event published: {EventType} for {AggregateType}/{AggregateId}",
            eventEntity.EventType, eventEntity.AggregateType, eventEntity.AggregateId);

        return eventEntity;
    }

    // Métodos de consulta usando LINQ
    public async Task<List<Event>> GetEventsByAggregateIdAsync(string aggregateId)
        => await _dbContext.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

    public async Task<List<Event>> GetEventsByAggregateTypeAsync(string aggregateType)
        => await _dbContext.Events
            .Where(e => e.AggregateType == aggregateType)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
