using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionAndNormalizeTreeMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_measurements_campaigns_campaign_id",
                table: "measurements");

            migrationBuilder.DropIndex(
                name: "ix_measurements_campaign_id",
                table: "measurements");

            migrationBuilder.DropColumn(
                name: "diameter_centimeters",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "health_state",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "height_centimeters",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "campaign_id",
                table: "measurements");

            migrationBuilder.DropColumn(
                name: "detected_health_state",
                table: "measurements");

            migrationBuilder.DropColumn(
                name: "inspection_type",
                table: "measurements");

            migrationBuilder.DropColumn(
                name: "observation_notes",
                table: "measurements");

            migrationBuilder.DropColumn(
                name: "server_synced_at",
                table: "measurements");

            migrationBuilder.RenameColumn(
                name: "evidence_photo_url",
                table: "measurements",
                newName: "photo_url");

            migrationBuilder.AlterColumn<decimal>(
                name: "height_centimeters",
                table: "measurements",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "diameter_centimeters",
                table: "measurements",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "inspections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    inspector_id = table.Column<int>(type: "integer", nullable: false),
                    health_state = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    device_captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inspections", x => x.id);
                    table.ForeignKey(
                        name: "fk_inspections_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inspections_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inspections_users_inspector_id",
                        column: x => x.inspector_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inspections_inspector_id",
                table: "inspections",
                column: "inspector_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspections_tenant_id",
                table: "inspections",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspections_tree_id",
                table: "inspections",
                column: "tree_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inspections");

            migrationBuilder.RenameColumn(
                name: "photo_url",
                table: "measurements",
                newName: "evidence_photo_url");

            migrationBuilder.AddColumn<decimal>(
                name: "diameter_centimeters",
                table: "trees",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "health_state",
                table: "trees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "height_centimeters",
                table: "trees",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "trees",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "height_centimeters",
                table: "measurements",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "diameter_centimeters",
                table: "measurements",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "campaign_id",
                table: "measurements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "detected_health_state",
                table: "measurements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "inspection_type",
                table: "measurements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "observation_notes",
                table: "measurements",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "server_synced_at",
                table: "measurements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_measurements_campaign_id",
                table: "measurements",
                column: "campaign_id");

            migrationBuilder.AddForeignKey(
                name: "fk_measurements_campaigns_campaign_id",
                table: "measurements",
                column: "campaign_id",
                principalTable: "campaigns",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
