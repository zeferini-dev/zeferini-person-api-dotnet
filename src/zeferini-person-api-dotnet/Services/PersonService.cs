using System.Text.Json;
using ZeferiniPersonApi.DTOs;
using ZeferiniPersonApi.Models;
using MongoDB.Driver;

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
    private readonly IMongoCollection<Person> _personCollection;
    private readonly IEventsService _eventsService;

    public PersonService(IMongoCollection<Person> personCollection, IEventsService eventsService)
    {
        _personCollection = personCollection;
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
        var persons = await _personCollection.Find(_ => true).ToListAsync();
        return persons.OrderBy(p => p.CreatedAt).ToList();
    }

    public async Task<Person?> GetByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return null;
        var person = await _personCollection.Find(p => p.Id == guid).FirstOrDefaultAsync();
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
}
