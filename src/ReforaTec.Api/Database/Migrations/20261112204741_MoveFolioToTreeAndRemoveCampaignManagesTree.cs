using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class MoveFolioToTreeAndRemoveCampaignManagesTree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_manages_trees");

            migrationBuilder.AddColumn<string>(
                name: "folio",
                table: "trees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "normalized_folio",
                table: "trees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_trees_tenant_id_normalized_folio",
                table: "trees",
                columns: new[] { "tenant_id", "normalized_folio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trees_tenant_id_normalized_folio",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "folio",
                table: "trees");

            migrationBuilder.DropColumn(
                name: "normalized_folio",
                table: "trees");

            migrationBuilder.CreateTable(
                name: "campaign_manages_trees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    campaign_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    campaign_folio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    normalized_campaign_folio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_manages_trees", x => x.id);
                    table.ForeignKey(
                        name: "fk_campaign_manages_trees_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_campaign_manages_trees_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_campaign_manages_trees_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_manages_trees_campaign_id",
                table: "campaign_manages_trees",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_manages_trees_campaign_id_normalized_campaign_folio",
                table: "campaign_manages_trees",
                columns: new[] { "campaign_id", "normalized_campaign_folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_campaign_manages_trees_tenant_id",
                table: "campaign_manages_trees",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_manages_trees_tree_id",
                table: "campaign_manages_trees",
                column: "tree_id");
        }
    }
}
