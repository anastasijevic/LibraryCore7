using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfDeathToAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfDeath",
                table: "Author",
                type: "datetimeoffset(4)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: new Guid("25320c5e-f58a-4b1f-b63a-8ee07a840bdf"),
                column: "DateOfDeath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: new Guid("412c3012-d891-4f5e-9613-ff7aa63e6bb3"),
                column: "DateOfDeath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: new Guid("578359b7-1967-41d6-8b87-64ab7605587e"),
                column: "DateOfDeath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: new Guid("76053df4-6687-4353-8937-b45556748abe"),
                column: "DateOfDeath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: new Guid("a1da1d8e-1988-4634-b538-a01709477b77"),
                column: "DateOfDeath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Author",
                keyColumn: "Id",
                keyValue: new Guid("f74d6899-9ed2-4137-9876-66b070553f8f"),
                column: "DateOfDeath",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfDeath",
                table: "Author");
        }
    }
}
