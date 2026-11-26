using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReforaTec.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "notification_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    target_health_state = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    message_body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "species",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scientific_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_scientific_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    common_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_common_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_species", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    institution_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    normalized_institution_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    acronym = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    institutional_email_domain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    normalized_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaigns", x => x.id);
                    table.ForeignKey(
                        name: "fk_campaigns_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    control_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    second_family_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    folio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    normalized_folio = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    planting_date = table.Column<DateOnly>(type: "date", nullable: true),
                    virtue_id = table.Column<int>(type: "integer", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    location_latitude = table.Column<double>(type: "double precision", nullable: true),
                    location_longitude = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trees", x => x.id);
                    table.ForeignKey(
                        name: "fk_trees_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trees_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trees_virtues_virtue_id",
                        column: x => x.virtue_id,
                        principalTable: "virtues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "one_time_passwords",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_one_time_passwords", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_one_time_passwords_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hashed_token = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    audience = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    device_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    os_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_active_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_devices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_devices_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "inspections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
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
                        name: "fk_inspections_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "measurements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    tree_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    height_centimeters = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    device_captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_measurements", x => x.id);
                    table.ForeignKey(
                        name: "fk_measurements_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_measurements_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_measurements_users_user_id",
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
                name: "ix_campaigns_tenant_id",
                table: "campaigns",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_tenant_id_normalized_code",
                table: "campaigns",
                columns: new[] { "tenant_id", "normalized_code" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_inspections_tenant_id",
                table: "inspections",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspections_tree_id",
                table: "inspections",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "ix_inspections_user_id",
                table: "inspections",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_measurements_tenant_id",
                table: "measurements",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_measurements_tree_id",
                table: "measurements",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "ix_measurements_user_id",
                table: "measurements",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_type",
                table: "notification_templates",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_hashed_token",
                table: "refresh_tokens",
                column: "hashed_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_created_at",
                table: "refresh_tokens",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_species_normalized_common_name",
                table: "species",
                column: "normalized_common_name");

            migrationBuilder.CreateIndex(
                name: "ix_species_normalized_scientific_name",
                table: "species",
                column: "normalized_scientific_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_institutional_email_domain",
                table: "tenants",
                column: "institutional_email_domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_normalized_institution_name",
                table: "tenants",
                column: "normalized_institution_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trees_species_id",
                table: "trees",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "ix_trees_tenant_id",
                table: "trees",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_trees_tenant_id_normalized_folio",
                table: "trees",
                columns: new[] { "tenant_id", "normalized_folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trees_virtue_id",
                table: "trees",
                column: "virtue_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_device_token",
                table: "user_devices",
                column: "device_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_tenant_id",
                table: "user_devices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_devices_user_id",
                table: "user_devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant_id_control_number",
                table: "users",
                columns: new[] { "tenant_id", "control_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_virtues_normalized_name",
                table: "virtues",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_enrollments");

            migrationBuilder.DropTable(
                name: "care_assignments");

            migrationBuilder.DropTable(
                name: "care_logs");

            migrationBuilder.DropTable(
                name: "inspections");

            migrationBuilder.DropTable(
                name: "measurements");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "one_time_passwords");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_devices");

            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "care_types");

            migrationBuilder.DropTable(
                name: "trees");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "species");

            migrationBuilder.DropTable(
                name: "virtues");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
