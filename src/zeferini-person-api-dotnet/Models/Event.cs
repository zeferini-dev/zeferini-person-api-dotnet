
using FluentValidation;
namespace ZeferiniPersonApi.Models;

public class Event
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public Dictionary<string, object> EventData { get; set; } = new();
    public Dictionary<string, object?> Metadata { get; set; } = new();
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }



    public void ValidateAndThrow()
    {
        var validator = new EventValidator();
        var result = validator.Validate(this);
        if (!result.IsValid)
            throw new FluentValidation.ValidationException(result.Errors);
    }
}
