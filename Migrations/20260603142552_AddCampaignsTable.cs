using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    campaign_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    normalized_campaign_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    school_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    location_latitude = table.Column<double>(type: "double precision", nullable: true),
                    location_longitude = table.Column<double>(type: "double precision", nullable: true),
                    location_street = table.Column<string>(type: "text", nullable: false),
                    location_neighborhood = table.Column<string>(type: "text", nullable: false),
                    location_street_number = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaigns", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_normalized_campaign_name",
                table: "campaigns",
                column: "normalized_campaign_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaigns");
        }
    }
}
