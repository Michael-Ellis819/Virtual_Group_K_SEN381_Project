using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ServiceRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.Sql("""
                                 UPDATE "ServiceRequests"
                                 SET "CreatedAt" = "UpdatedAt";
                                 """);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "RequestNotes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.Sql("""
                                 UPDATE "RequestNotes" AS n
                                 SET "CreatedAt" = r."UpdatedAt"
                                 FROM "ServiceRequests" AS r
                                 WHERE n."RequestId" = r."RequestId";
                                 """);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ServiceRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "ServiceRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionKey",
                table: "ServiceRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "RequestNotes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "legacy-unknown");

            
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequesterId_SubmissionKey",
                table: "ServiceRequests",
                columns: new[] { "RequesterId", "SubmissionKey" },
                unique: true,
                filter: "\"SubmissionKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RequesterId_SubmissionKey",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DueAt",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "SubmissionKey",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "RequestNotes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RequestNotes");
        }
    }
}
