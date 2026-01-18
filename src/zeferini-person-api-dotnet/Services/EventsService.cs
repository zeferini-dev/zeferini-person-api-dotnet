using ZeferiniPersonApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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
    private bool _disposed;

    public EventsService(EventsDbContext dbContext, ILogger<EventsService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Event> PublishEventAsync(EventPayload payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));

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

        await _dbContext.Events.AddAsync(eventEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogDebug("Event published: {EventType} for {AggregateType}/{AggregateId}",
            eventEntity.EventType, eventEntity.AggregateType, eventEntity.AggregateId);

        return eventEntity;
    }

    public async Task<List<Event>> GetEventsByAggregateIdAsync(string aggregateId)
    {
        if (string.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentException("AggregateId cannot be null or empty.", nameof(aggregateId));

        return await _dbContext.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Event>> GetEventsByAggregateTypeAsync(string aggregateType)
    {
        if (string.IsNullOrWhiteSpace(aggregateType)) throw new ArgumentException("AggregateType cannot be null or empty.", nameof(aggregateType));

        return await _dbContext.Events
            .Where(e => e.AggregateType == aggregateType)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
