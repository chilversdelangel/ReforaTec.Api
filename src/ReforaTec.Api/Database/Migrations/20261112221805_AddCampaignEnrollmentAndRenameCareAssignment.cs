using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignEnrollmentAndRenameCareAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_cares_for_trees");

            migrationBuilder.CreateTable(
                name: "campaign_enrollments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    campaign_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: true),
                    evaluation_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "fk_campaign_enrollments_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_campaign_enrollments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_campaign_enrollments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "care_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    campaign_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_assignments", x => x.id);
                    table.CheckConstraint("ck_care_assignments_end_date_after_start_date", "end_date IS NULL OR end_date >= start_date");
                    table.ForeignKey(
                        name: "fk_care_assignments_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_assignments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_assignments_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_campaign_enrollments_campaign_id_user_id",
                table: "campaign_enrollments",
                columns: new[] { "campaign_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_campaign_enrollments_tenant_id",
                table: "campaign_enrollments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_enrollments_user_id",
                table: "campaign_enrollments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_assignments_campaign_id",
                table: "care_assignments",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_assignments_campaign_id_user_id_tree_id",
                table: "care_assignments",
                columns: new[] { "campaign_id", "user_id", "tree_id" },
                unique: true,
                filter: "end_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_care_assignments_tenant_id",
                table: "care_assignments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_assignments_tree_id",
                table: "care_assignments",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_assignments_user_id",
                table: "care_assignments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_enrollments");

            migrationBuilder.DropTable(
                name: "care_assignments");

            migrationBuilder.CreateTable(
                name: "user_cares_for_trees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    campaign_id = table.Column<int>(type: "integer", nullable: true),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_cares_for_trees", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_cares_for_trees_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_cares_for_trees_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_cares_for_trees_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_cares_for_trees_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_cares_for_trees_campaign_id",
                table: "user_cares_for_trees",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_cares_for_trees_tenant_id",
                table: "user_cares_for_trees",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_cares_for_trees_tree_id",
                table: "user_cares_for_trees",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_cares_for_trees_user_id",
                table: "user_cares_for_trees",
                column: "user_id");
        }
    }
}
