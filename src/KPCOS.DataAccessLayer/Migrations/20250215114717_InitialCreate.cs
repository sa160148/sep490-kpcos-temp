using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPCOS.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "construction_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("construction_template_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("equipment_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("maintenance_item_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_package",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price_per_unit = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("maintenance_package_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "package",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("package_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "package_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("package_item_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    discount = table.Column<int>(type: "integer", nullable: false),
                    starttime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    exptime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("promotion_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("service_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTaotZTcu1CLMGOJMDl-f_LYBECs7tqwhgpXA&s'::character varying"),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "construction_template_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    idparent = table.Column<Guid>(type: "uuid", nullable: true),
                    idtemplate = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("construction_template_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "construction_template_item_idparent_fkey",
                        column: x => x.idparent,
                        principalTable: "construction_template_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "construction_template_item_idtemplate_fkey",
                        column: x => x.idtemplate,
                        principalTable: "construction_template",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "maintenance_package_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    maintenance_package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("maintenance_package_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "maintenance_package_item_maintenance_item_id_fkey",
                        column: x => x.maintenance_item_id,
                        principalTable: "maintenance_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "maintenance_package_item_maintenance_package_id_fkey",
                        column: x => x.maintenance_package_id,
                        principalTable: "maintenance_package",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "package_detail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("package_detail_pkey", x => x.id);
                    table.ForeignKey(
                        name: "package_detail_package_id_fkey",
                        column: x => x.package_id,
                        principalTable: "package",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "package_detail_package_item_id_fkey",
                        column: x => x.package_item_id,
                        principalTable: "package_item",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    point = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    dob = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "'2000-01-01'::date"),
                    gender = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_pkey", x => x.id);
                    table.ForeignKey(
                        name: "customer_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    position = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("staff_pkey", x => x.id);
                    table.ForeignKey(
                        name: "staff_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "construction_template_task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    idtemplateitem = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("construction_template_task_pkey", x => x.id);
                    table.ForeignKey(
                        name: "construction_template_task_idtemplateitem_fkey",
                        column: x => x.idtemplateitem,
                        principalTable: "construction_template_item",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "maintenance_request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("maintenance_request_pkey", x => x.id);
                    table.ForeignKey(
                        name: "maintenance_request_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "maintenance_request_maintenance_package_id_fkey",
                        column: x => x.maintenance_package_id,
                        principalTable: "maintenance_package",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    area = table.Column<double>(type: "double precision", nullable: false),
                    depth = table.Column<double>(type: "double precision", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    templatedesignid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("project_pkey", x => x.id);
                    table.ForeignKey(
                        name: "project_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "project_package_id_fkey",
                        column: x => x.package_id,
                        principalTable: "package",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "maintenance_request_task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    maintenance_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("maintenance_request_task_pkey", x => x.id);
                    table.ForeignKey(
                        name: "maintenance_request_task_maintenance_request_id_fkey",
                        column: x => x.maintenance_request_id,
                        principalTable: "maintenance_request",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "maintenance_request_task_staff_id_fkey",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "construction_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    estdate = table.Column<DateOnly>(type: "date", nullable: false),
                    actdate = table.Column<DateOnly>(type: "date", nullable: true),
                    idparent = table.Column<Guid>(type: "uuid", nullable: true),
                    idproject = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("construction_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "construction_item_idproject_fkey",
                        column: x => x.idproject,
                        principalTable: "project",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    contract_value = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("contract_pkey", x => x.id);
                    table.ForeignKey(
                        name: "contract_project_id_fkey",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "design",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("design_pkey", x => x.id);
                    table.ForeignKey(
                        name: "design_project_id_fkey",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "design_staff_id_fkey",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "docs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("docs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "docs_project_id_fkey",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "project_staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("project_staff_pkey", x => x.id);
                    table.ForeignKey(
                        name: "project_staff_project_id_fkey",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "project_staff_staff_id_fkey",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quotation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    total_price = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idtemplate = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quotation_pkey", x => x.id);
                    table.ForeignKey(
                        name: "quotation_idtemplate_fkey",
                        column: x => x.idtemplate,
                        principalTable: "construction_template",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotation_project_id_fkey",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotation_promotion_id_fkey",
                        column: x => x.promotion_id,
                        principalTable: "promotion",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "construction_task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    idconstructionitem = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    idstaff = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("construction_task_pkey", x => x.id);
                    table.ForeignKey(
                        name: "construction_task_idconstructionitem_fkey",
                        column: x => x.idconstructionitem,
                        principalTable: "construction_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "construction_task_idstaff_fkey",
                        column: x => x.idstaff,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payment_batch",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    total_value = table.Column<int>(type: "integer", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("payment_batch_pkey", x => x.id);
                    table.ForeignKey(
                        name: "payment_batch_contract_id_fkey",
                        column: x => x.contract_id,
                        principalTable: "contract",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "design_image",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    design_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("design_image_pkey", x => x.id);
                    table.ForeignKey(
                        name: "design_image_design_id_fkey",
                        column: x => x.design_id,
                        principalTable: "design",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    no = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    id_docs = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("transaction_pkey", x => x.id);
                    table.ForeignKey(
                        name: "transaction_customer_id_fkey",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "transaction_id_docs_fkey",
                        column: x => x.id_docs,
                        principalTable: "docs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quotation_detail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quotation_detail_pkey", x => x.id);
                    table.ForeignKey(
                        name: "quotation_detail_quotation_id_fkey",
                        column: x => x.quotation_id,
                        principalTable: "quotation",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotation_detail_service_id_fkey",
                        column: x => x.service_id,
                        principalTable: "service",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quotation_equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quotation_equipment_pkey", x => x.id);
                    table.ForeignKey(
                        name: "quotation_equipment_equipment_id_fkey",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotation_equipment_quotation_id_fkey",
                        column: x => x.quotation_id,
                        principalTable: "quotation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "construction_item_idparent_index",
                table: "construction_item",
                column: "idparent");

            migrationBuilder.CreateIndex(
                name: "construction_item_idproject_index",
                table: "construction_item",
                column: "idproject");

            migrationBuilder.CreateIndex(
                name: "construction_item_name_index",
                table: "construction_item",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "construction_item_status_index",
                table: "construction_item",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "construction_task_idconstructionitem_index",
                table: "construction_task",
                column: "idconstructionitem");

            migrationBuilder.CreateIndex(
                name: "construction_task_idstaff_index",
                table: "construction_task",
                column: "idstaff");

            migrationBuilder.CreateIndex(
                name: "construction_task_name_index",
                table: "construction_task",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "construction_task_status_index",
                table: "construction_task",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "construction_template_name_index",
                table: "construction_template",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "construction_template_status_index",
                table: "construction_template",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "construction_template_item_idparent_index",
                table: "construction_template_item",
                column: "idparent");

            migrationBuilder.CreateIndex(
                name: "construction_template_item_name_index",
                table: "construction_template_item",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "construction_template_item_status_index",
                table: "construction_template_item",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_construction_template_item_idtemplate",
                table: "construction_template_item",
                column: "idtemplate");

            migrationBuilder.CreateIndex(
                name: "construction_template_task_idtemplateitem_index",
                table: "construction_template_task",
                column: "idtemplateitem");

            migrationBuilder.CreateIndex(
                name: "construction_template_task_name_index",
                table: "construction_template_task",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "construction_template_task_status_index",
                table: "construction_template_task",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "contract_project_id_index",
                table: "contract",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "contract_quotation_id_index",
                table: "contract",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "contract_status_index",
                table: "contract",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "customer_user_id_index",
                table: "customer",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "design_project_id_index",
                table: "design",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "design_staff_id_index",
                table: "design",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "design_status_index",
                table: "design",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "design_version_index",
                table: "design",
                column: "version");

            migrationBuilder.CreateIndex(
                name: "design_image_design_id_index",
                table: "design_image",
                column: "design_id");

            migrationBuilder.CreateIndex(
                name: "docs_name_index",
                table: "docs",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "docs_project_id_index",
                table: "docs",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "docs_type_index",
                table: "docs",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "equipment_name_index",
                table: "equipment",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "maintenance_item_name_index",
                table: "maintenance_item",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "maintenance_package_name_index",
                table: "maintenance_package",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "maintenance_package_status_index",
                table: "maintenance_package",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_package_item_maintenance_item_id",
                table: "maintenance_package_item",
                column: "maintenance_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_package_item_maintenance_package_id",
                table: "maintenance_package_item",
                column: "maintenance_package_id");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_customer_id_index",
                table: "maintenance_request",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_maintenance_package_id_index",
                table: "maintenance_request",
                column: "maintenance_package_id");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_status_index",
                table: "maintenance_request",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_task_maintenance_request_id_index",
                table: "maintenance_request_task",
                column: "maintenance_request_id");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_task_name_index",
                table: "maintenance_request_task",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_task_staff_id_index",
                table: "maintenance_request_task",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "maintenance_request_task_status_index",
                table: "maintenance_request_task",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "package_name_index",
                table: "package",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "package_status_index",
                table: "package",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "package_detail_package_id_index",
                table: "package_detail",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "package_detail_package_item_id_index",
                table: "package_detail",
                column: "package_item_id");

            migrationBuilder.CreateIndex(
                name: "package_item_name_index",
                table: "package_item",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "payment_batch_contract_id_index",
                table: "payment_batch",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "payment_batch_status_index",
                table: "payment_batch",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_project_package_id",
                table: "project",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "project_address_index",
                table: "project",
                column: "address");

            migrationBuilder.CreateIndex(
                name: "project_customer_id_index",
                table: "project",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "project_customer_name_index",
                table: "project",
                column: "customer_name");

            migrationBuilder.CreateIndex(
                name: "project_email_index",
                table: "project",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "project_name_index",
                table: "project",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "project_phone_index",
                table: "project",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "project_status_index",
                table: "project",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "project_staff_project_id_index",
                table: "project_staff",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "project_staff_staff_id_index",
                table: "project_staff",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "promotion_code_index",
                table: "promotion",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "promotion_name_index",
                table: "promotion",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "promotion_status_index",
                table: "promotion",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_quotation_idtemplate",
                table: "quotation",
                column: "idtemplate");

            migrationBuilder.CreateIndex(
                name: "IX_quotation_promotion_id",
                table: "quotation",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "quotation_project_id_index",
                table: "quotation",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "quotation_status_index",
                table: "quotation",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "quotation_version_index",
                table: "quotation",
                column: "version");

            migrationBuilder.CreateIndex(
                name: "quotation_detail_quotation_id_index",
                table: "quotation_detail",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "quotation_detail_service_id_index",
                table: "quotation_detail",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "quotation_equipment_equipment_id_index",
                table: "quotation_equipment",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "quotation_equipment_quotation_id_index",
                table: "quotation_equipment",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "service_name_index",
                table: "service",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "service_status_index",
                table: "service",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "staff_user_id_index",
                table: "staff",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_id_docs",
                table: "transaction",
                column: "id_docs");

            migrationBuilder.CreateIndex(
                name: "transaction_customer_id_index",
                table: "transaction",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "transaction_no_index",
                table: "transaction",
                column: "no");

            migrationBuilder.CreateIndex(
                name: "transaction_status_index",
                table: "transaction",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "transaction_type_index",
                table: "transaction",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "users_email_index",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "users_phone_index",
                table: "users",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "users_status_index",
                table: "users",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "construction_task");

            migrationBuilder.DropTable(
                name: "construction_template_task");

            migrationBuilder.DropTable(
                name: "design_image");

            migrationBuilder.DropTable(
                name: "maintenance_package_item");

            migrationBuilder.DropTable(
                name: "maintenance_request_task");

            migrationBuilder.DropTable(
                name: "package_detail");

            migrationBuilder.DropTable(
                name: "payment_batch");

            migrationBuilder.DropTable(
                name: "project_staff");

            migrationBuilder.DropTable(
                name: "quotation_detail");

            migrationBuilder.DropTable(
                name: "quotation_equipment");

            migrationBuilder.DropTable(
                name: "transaction");

            migrationBuilder.DropTable(
                name: "construction_item");

            migrationBuilder.DropTable(
                name: "construction_template_item");

            migrationBuilder.DropTable(
                name: "design");

            migrationBuilder.DropTable(
                name: "maintenance_item");

            migrationBuilder.DropTable(
                name: "maintenance_request");

            migrationBuilder.DropTable(
                name: "package_item");

            migrationBuilder.DropTable(
                name: "contract");

            migrationBuilder.DropTable(
                name: "service");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "quotation");

            migrationBuilder.DropTable(
                name: "docs");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "maintenance_package");

            migrationBuilder.DropTable(
                name: "construction_template");

            migrationBuilder.DropTable(
                name: "promotion");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "package");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
