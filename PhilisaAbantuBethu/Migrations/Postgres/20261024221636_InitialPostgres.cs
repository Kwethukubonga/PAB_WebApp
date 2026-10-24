using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhilisaAbantuBethu.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactInquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Subject = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Message = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Surname = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactMethod = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SupportType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Situation = table.Column<string>(type: "text", nullable: false),
                    Urgent = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Extra = table.Column<string>(type: "text", nullable: false),
                    Declaration = table.Column<bool>(type: "boolean", nullable: false),
                    Reference = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false, defaultValue: "new"),
                    StatusUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandledBy = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    StaffNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactInquiries_SubmittedAt",
                table: "ContactInquiries",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_Reference",
                table: "SupportRequests",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_Status",
                table: "SupportRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequests_SubmittedAt",
                table: "SupportRequests",
                column: "SubmittedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactInquiries");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "SupportRequests");
        }
    }
}
