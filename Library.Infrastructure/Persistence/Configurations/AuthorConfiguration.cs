using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey("Id");
        
        builder.Property(a => a.Id)
            .IsRequired()
            .HasColumnType("uniqueidentifier");
        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasColumnType("nvarchar(50)");
        builder.Property(a => a.LastName)
            .IsRequired()
            .HasColumnType("nvarchar(70)");
        builder.Property(a => a.DateOfBirth)
            .IsRequired()
            .HasColumnType("datetimeoffset(4)");
            builder.Property(a => a.DateOfDeath)
            .IsRequired(false)
            .HasColumnType("datetimeoffset(4)");
        builder.Property(a => a.Genre)
            .IsRequired()
            .HasColumnType("nvarchar(60)");
        
        builder.HasMany(a => a.Books)
            .WithOne(b => b.Author)
            .HasForeignKey(b => b.AuthorId);
        
    }
}
