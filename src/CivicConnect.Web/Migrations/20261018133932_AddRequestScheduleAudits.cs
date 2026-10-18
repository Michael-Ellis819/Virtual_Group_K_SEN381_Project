using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestScheduleAudits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestScheduleAudits",
                columns: table => new
                {
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviousPriority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NewPriority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PreviousDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NewDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestScheduleAudits", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_RequestScheduleAudits_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestScheduleAudits_RequestId",
                table: "RequestScheduleAudits",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestScheduleAudits");
        }
    }
}
