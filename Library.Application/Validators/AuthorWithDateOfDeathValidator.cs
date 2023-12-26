using FluentValidation;
using Library.Application.Models;

namespace Library.Application.Validators;

public class AuthorWithDateOfDeathValidator : AbstractValidator<AuthorCreateWithDateOfDeathDto>
{
    public AuthorWithDateOfDeathValidator()
    {
        RuleFor(a => a.FirstName).MaximumLength(50).NotEmpty();
        RuleFor(a => a.LastName).MaximumLength(70).NotEmpty();
        RuleFor(a => a.DateOfBirth).NotEmpty().LessThan(DateTimeOffset.Now);
        RuleFor(a => a.DateOfDeath).LessThanOrEqualTo(DateTimeOffset.Now);
        RuleFor(a => a.Genre).MaximumLength(60).NotEmpty();
        
    }
}
