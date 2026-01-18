using FluentValidation;

namespace ZeferiniPersonApi.Models;

public class EventValidator : AbstractValidator<Event>
{
    public EventValidator()
    {
        RuleFor(x => x.AggregateId)
            .NotEmpty();
        RuleFor(x => x.AggregateType)
            .NotEmpty();
        RuleFor(x => x.EventType)
            .NotEmpty();
        RuleFor(x => x.EventData)
            .NotNull();
        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0);
    }
}
