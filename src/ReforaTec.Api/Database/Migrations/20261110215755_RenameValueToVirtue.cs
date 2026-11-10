using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameValueToVirtue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_trees_values_value_id",
                table: "trees");

            migrationBuilder.DropTable(
                name: "values");

            migrationBuilder.RenameColumn(
                name: "value_id",
                table: "trees",
                newName: "virtue_id");

            migrationBuilder.RenameIndex(
                name: "ix_trees_value_id",
                table: "trees",
                newName: "ix_trees_virtue_id");

            migrationBuilder.CreateTable(
                name: "virtues",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_virtues", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_virtues_normalized_name",
                table: "virtues",
                column: "normalized_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_trees_virtues_virtue_id",
                table: "trees",
                column: "virtue_id",
                principalTable: "virtues",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_trees_virtues_virtue_id",
                table: "trees");

            migrationBuilder.DropTable(
                name: "virtues");

            migrationBuilder.RenameColumn(
                name: "virtue_id",
                table: "trees",
                newName: "value_id");

            migrationBuilder.RenameIndex(
                name: "ix_trees_virtue_id",
                table: "trees",
                newName: "ix_trees_value_id");

            migrationBuilder.CreateTable(
                name: "values",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    normalized_value_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    value_name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_values", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_values_normalized_value_name",
                table: "values",
                column: "normalized_value_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_trees_values_value_id",
                table: "trees",
                column: "value_id",
                principalTable: "values",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
