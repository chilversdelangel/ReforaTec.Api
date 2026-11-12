using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserInspectsCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_inspects_campaigns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_inspects_campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    campaign_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_inspects_campaigns", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_inspects_campaigns_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_inspects_campaigns_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_inspects_campaigns_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_inspects_campaigns_campaign_id",
                table: "user_inspects_campaigns",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_inspects_campaigns_tenant_id",
                table: "user_inspects_campaigns",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_inspects_campaigns_user_id",
                table: "user_inspects_campaigns",
                column: "user_id");
        }
    }
}
