using FluentValidation;
using Library.Application.Models;

namespace Library.Application.Validators;

public class BookUpdateValidator : AbstractValidator<BookUpdateDto>
{
    public BookUpdateValidator()
    {
        Include(new BookCreateUpdateValidatorBase());
        RuleFor(a => a.Description).NotEmpty();
        
    }
}