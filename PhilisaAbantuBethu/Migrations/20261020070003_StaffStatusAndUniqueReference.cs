using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhilisaAbantuBethu.Migrations
{
    /// <inheritdoc />
    public partial class StaffStatusAndUniqueReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandledBy",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SupportRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "new");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusUpdatedAt",
                table: "SupportRequests",
                type: "TEXT",
                nullable: true);

            // The old PAB + three digit references could repeat. Keep the earliest request's
            // reference and add the row id to later copies, or the unique index below would
            // refuse to build on a live database.
            migrationBuilder.Sql(
                "UPDATE SupportRequests SET Reference = Reference || '-' || Id " +
                "WHERE Id NOT IN (SELECT MIN(Id) FROM SupportRequests GROUP BY Reference);");

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

            migrationBuilder.CreateIndex(
                name: "IX_ContactInquiries_SubmittedAt",
                table: "ContactInquiries",
                column: "SubmittedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_Reference",
                table: "SupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_Status",
                table: "SupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupportRequests_SubmittedAt",
                table: "SupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactInquiries_SubmittedAt",
                table: "ContactInquiries");

            migrationBuilder.DropColumn(
                name: "HandledBy",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAt",
                table: "SupportRequests");
        }
    }
}
