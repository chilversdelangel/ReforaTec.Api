using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStreetAddressFromTrees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "location_neighborhood",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "location_street",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "location_street_number",
                table: "trees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "location_neighborhood",
                table: "trees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_street",
                table: "trees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location_street_number",
                table: "trees",
                type: "text",
                nullable: true);
        }
    }
}
