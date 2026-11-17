using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameServiceToCareLogAndStandardizeUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inspections_users_inspector_id",
                table: "inspections");

            migrationBuilder.DropForeignKey(
                name: "fk_measurements_users_inspector_id",
                table: "measurements");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "service_types");

            migrationBuilder.RenameColumn(
                name: "inspector_id",
                table: "measurements",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_measurements_inspector_id",
                table: "measurements",
                newName: "ix_measurements_user_id");

            migrationBuilder.RenameColumn(
                name: "inspector_id",
                table: "inspections",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_inspections_inspector_id",
                table: "inspections",
                newName: "ix_inspections_user_id");

            migrationBuilder.CreateTable(
                name: "care_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    icon_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "care_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    care_type_id = table.Column<int>(type: "integer", nullable: false),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_care_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_care_logs_care_types_care_type_id",
                        column: x => x.care_type_id,
                        principalTable: "care_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_logs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_logs_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_care_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_care_logs_care_type_id",
                table: "care_logs",
                column: "care_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_logs_tenant_id",
                table: "care_logs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_logs_tree_id",
                table: "care_logs",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_logs_user_id",
                table: "care_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_care_types_normalized_name",
                table: "care_types",
                column: "normalized_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_inspections_users_user_id",
                table: "inspections",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_measurements_users_user_id",
                table: "measurements",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inspections_users_user_id",
                table: "inspections");

            migrationBuilder.DropForeignKey(
                name: "fk_measurements_users_user_id",
                table: "measurements");

            migrationBuilder.DropTable(
                name: "care_logs");

            migrationBuilder.DropTable(
                name: "care_types");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "measurements",
                newName: "inspector_id");

            migrationBuilder.RenameIndex(
                name: "ix_measurements_user_id",
                table: "measurements",
                newName: "ix_measurements_inspector_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "inspections",
                newName: "inspector_id");

            migrationBuilder.RenameIndex(
                name: "ix_inspections_user_id",
                table: "inspections",
                newName: "ix_inspections_inspector_id");

            migrationBuilder.CreateTable(
                name: "service_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    icon_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    normalized_service_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    service_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    campaign_id = table.Column<int>(type: "integer", nullable: true),
                    service_type_id = table.Column<int>(type: "integer", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    server_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_services_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_services_service_types_service_type_id",
                        column: x => x.service_type_id,
                        principalTable: "service_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_services_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_services_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_services_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_types_normalized_service_name",
                table: "service_types",
                column: "normalized_service_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_campaign_id",
                table: "services",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_service_type_id",
                table: "services",
                column: "service_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_student_id",
                table: "services",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_tenant_id",
                table: "services",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_tree_id",
                table: "services",
                column: "tree_id");

            migrationBuilder.AddForeignKey(
                name: "fk_inspections_users_inspector_id",
                table: "inspections",
                column: "inspector_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_measurements_users_inspector_id",
                table: "measurements",
                column: "inspector_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
