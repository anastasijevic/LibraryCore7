using FluentValidation;
using Library.Application.Models;

namespace Library.Application.Validators;

public class BookCreateValidator : AbstractValidator<BookCreateDto>
{
    public BookCreateValidator()
    {
        Include(new BookCreateUpdateValidatorBase());
        
    }
}