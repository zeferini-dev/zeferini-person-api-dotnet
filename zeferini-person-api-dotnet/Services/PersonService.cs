using System.Text.Json;
using ZeferiniPersonApi.DTOs;
using ZeferiniPersonApi.Models;

namespace ZeferiniPersonApi.Services;

public interface IPersonService
{
    Task<Person> CreateAsync(CreatePersonDto dto);
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(string id);
    Task<Person?> UpdateAsync(string id, UpdatePersonDto dto);
    Task<Person?> DeleteAsync(string id);
}

public class PersonService : IPersonService
{
    private readonly IEventsService _eventsService;

    public PersonService(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    public async Task<Person> CreateAsync(CreatePersonDto dto)
    {
        var personId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var person = new Person
        {
            Id = personId,
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _eventsService.PublishEventAsync(new EventPayload
        {
            AggregateId = person.Id.ToString(),
            AggregateType = "Person",
            EventType = "PersonCreated",
            EventData = new Dictionary<string, object>
            {
                { "id", person.Id.ToString() },
                { "name", person.Name },
                { "email", person.Email },
                { "createdAt", person.CreatedAt.ToString("O") }
            },
            Metadata = new Dictionary<string, object?>
            {
                { "source", "person-service-dotnet" },
                { "userId", null }
            }
        });

        return person;
    }

    public async Task<List<Person>> GetAllAsync()
    {
        var events = await _eventsService.GetEventsByAggregateTypeAsync("Person");
        var personsMap = new Dictionary<string, Person>();

        // Reconstruct state from events (reversed to process oldest first)
        foreach (var evt in events.AsEnumerable().Reverse())
        {
            var aggregateId = evt.AggregateId;

            switch (evt.EventType)
            {
                case "PersonCreated":
                    personsMap[aggregateId] = new Person
                    {
                        Id = Guid.Parse(GetEventDataValue(evt.EventData, "id")),
                        Name = GetEventDataValue(evt.EventData, "name"),
                        Email = GetEventDataValue(evt.EventData, "email"),
                        CreatedAt = DateTime.Parse(GetEventDataValue(evt.EventData, "createdAt")),
                        UpdatedAt = DateTime.Parse(GetEventDataValue(evt.EventData, "createdAt"))
                    };
                    break;

                case "PersonUpdated":
                    if (personsMap.TryGetValue(aggregateId, out var personToUpdate))
                    {
                        var newName = GetEventDataValueOrNull(evt.EventData, "name");
                        var newEmail = GetEventDataValueOrNull(evt.EventData, "email");
                        
                        if (newName != null) personToUpdate.Name = newName;
                        if (newEmail != null) personToUpdate.Email = newEmail;
                        personToUpdate.UpdatedAt = DateTime.Parse(GetEventDataValue(evt.EventData, "updatedAt"));
                    }
                    break;

                case "PersonDeleted":
                    personsMap.Remove(aggregateId);
                    break;
            }
        }

        return personsMap.Values
            .OrderBy(p => p.CreatedAt)
            .ToList();
    }

    public async Task<Person?> GetByIdAsync(string id)
    {
        var events = await _eventsService.GetEventsByAggregateIdAsync(id);

        if (events.Count == 0)
            return null;

        Person? person = null;

        foreach (var evt in events)
        {
            switch (evt.EventType)
            {
                case "PersonCreated":
                    person = new Person
                    {
                        Id = Guid.Parse(GetEventDataValue(evt.EventData, "id")),
                        Name = GetEventDataValue(evt.EventData, "name"),
                        Email = GetEventDataValue(evt.EventData, "email"),
                        CreatedAt = DateTime.Parse(GetEventDataValue(evt.EventData, "createdAt")),
                        UpdatedAt = DateTime.Parse(GetEventDataValue(evt.EventData, "createdAt"))
                    };
                    break;

                case "PersonUpdated":
                    if (person != null)
                    {
                        var newName = GetEventDataValueOrNull(evt.EventData, "name");
                        var newEmail = GetEventDataValueOrNull(evt.EventData, "email");
                        
                        if (newName != null) person.Name = newName;
                        if (newEmail != null) person.Email = newEmail;
                        person.UpdatedAt = DateTime.Parse(GetEventDataValue(evt.EventData, "updatedAt"));
                    }
                    break;

                case "PersonDeleted":
                    person = null;
                    break;
            }
        }

        return person;
    }

    public async Task<Person?> UpdateAsync(string id, UpdatePersonDto dto)
    {
        var existingPerson = await GetByIdAsync(id);
        if (existingPerson == null)
            return null;

        var now = DateTime.UtcNow;
        var updatedPerson = new Person
        {
            Id = existingPerson.Id,
            Name = dto.Name ?? existingPerson.Name,
            Email = dto.Email ?? existingPerson.Email,
            CreatedAt = existingPerson.CreatedAt,
            UpdatedAt = now
        };

        await _eventsService.PublishEventAsync(new EventPayload
        {
            AggregateId = id,
            AggregateType = "Person",
            EventType = "PersonUpdated",
            EventData = new Dictionary<string, object>
            {
                { "id", updatedPerson.Id.ToString() },
                { "name", updatedPerson.Name },
                { "email", updatedPerson.Email },
                { "updatedAt", updatedPerson.UpdatedAt.ToString("O") }
            },
            Metadata = new Dictionary<string, object?>
            {
                { "source", "person-service-dotnet" },
                { "userId", null }
            }
        });

        return updatedPerson;
    }

    public async Task<Person?> DeleteAsync(string id)
    {
        var person = await GetByIdAsync(id);
        if (person == null)
            return null;

        await _eventsService.PublishEventAsync(new EventPayload
        {
            AggregateId = id,
            AggregateType = "Person",
            EventType = "PersonDeleted",
            EventData = new Dictionary<string, object>
            {
                { "id", person.Id.ToString() },
                { "name", person.Name },
                { "email", person.Email },
                { "deletedAt", DateTime.UtcNow.ToString("O") }
            },
            Metadata = new Dictionary<string, object?>
            {
                { "source", "person-service-dotnet" },
                { "userId", null }
            }
        });

        return person;
    }

    private static string GetEventDataValue(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
                return jsonElement.GetString() ?? string.Empty;
            return value?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    private static string? GetEventDataValueOrNull(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
                return jsonElement.GetString();
            return value?.ToString();
        }
        return null;
    }
}
