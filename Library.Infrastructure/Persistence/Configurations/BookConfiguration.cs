using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey("Id");
        
        builder.Property(a => a.Id)
            .IsRequired()
            .HasColumnType("uniqueidentifier");
        builder.Property(a => a.Title)
            .IsRequired()
            .HasColumnType("nvarchar(80)");
        builder.Property(a => a.Description)
            .IsRequired(false)
            .HasColumnType("nvarchar(MAX)");
        builder.Property(a => a.AuthorId)
            .IsRequired()
            .HasColumnType("uniqueidentifier");
        
    }
}
