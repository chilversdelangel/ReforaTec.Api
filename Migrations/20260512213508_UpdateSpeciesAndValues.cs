using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReforaTec.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpeciesAndValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_values_value_name",
                table: "values");

            migrationBuilder.DropIndex(
                name: "ix_species_scientific_name",
                table: "species");

            migrationBuilder.AddColumn<string>(
                name: "normalized_value_name",
                table: "values",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "normalized_scientific_name",
                table: "species",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_values_normalized_value_name",
                table: "values",
                column: "normalized_value_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_species_normalized_scientific_name",
                table: "species",
                column: "normalized_scientific_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_values_normalized_value_name",
                table: "values");

            migrationBuilder.DropIndex(
                name: "ix_species_normalized_scientific_name",
                table: "species");

            migrationBuilder.DropColumn(
                name: "normalized_value_name",
                table: "values");

            migrationBuilder.DropColumn(
                name: "normalized_scientific_name",
                table: "species");

            migrationBuilder.CreateIndex(
                name: "ix_values_value_name",
                table: "values",
                column: "value_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_species_scientific_name",
                table: "species",
                column: "scientific_name",
                unique: true);
        }
    }
}
