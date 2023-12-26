using FluentValidation;
using Library.Application.Models;

namespace Library.Application.Validators;

public class BookCreateUpdateValidatorBase : AbstractValidator<BookCreateUpdateDtoBase>
{
    public BookCreateUpdateValidatorBase()
    {
        RuleFor(a => a.Title).MaximumLength(80).NotEmpty().WithMessage("You should fill out a title.");
        RuleFor(a => a.Description).MaximumLength(500);
        RuleFor(a => a.Description).NotEqual(c => c.Title).WithMessage("The provided 'Description' should be different from the 'Title'.");
    }
    
}
