using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class ScopeCampaignCodeToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_campaigns_normalized_code",
                table: "campaigns");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_tenant_id_normalized_code",
                table: "campaigns",
                columns: new[] { "tenant_id", "normalized_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_campaigns_tenant_id_normalized_code",
                table: "campaigns");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_normalized_code",
                table: "campaigns",
                column: "normalized_code",
                unique: true);
        }
    }
}
