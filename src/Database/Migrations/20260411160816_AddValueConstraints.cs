using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReforaTec.Api.src.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddValueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValueName",
                table: "Values",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Values_ValueName",
                table: "Values",
                column: "ValueName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_ValueName",
                table: "Values");

            migrationBuilder.AlterColumn<string>(
                name: "ValueName",
                table: "Values",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);
        }
    }
}
