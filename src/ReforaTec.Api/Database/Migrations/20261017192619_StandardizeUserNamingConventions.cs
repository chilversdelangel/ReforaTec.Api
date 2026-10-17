using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReforaTec.Api.src.Database.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeUserNamingConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "second_last_name",
                table: "users",
                newName: "second_family_name");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "users",
                newName: "family_name");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "users",
                newName: "given_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "second_family_name",
                table: "users",
                newName: "second_last_name");

            migrationBuilder.RenameColumn(
                name: "given_name",
                table: "users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "family_name",
                table: "users",
                newName: "last_name");
        }
    }
}
