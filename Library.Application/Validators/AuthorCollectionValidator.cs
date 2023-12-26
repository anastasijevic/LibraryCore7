using FluentValidation;
using Library.Application.Models;

namespace Library.Application.Validators;

public class AuthorCollectionValidator : AbstractValidator<List<AuthorCreateDto>>
{
    public AuthorCollectionValidator()
    {
        RuleForEach(a => a).SetValidator(new AuthorValidator()).NotEmpty();
    }
}
