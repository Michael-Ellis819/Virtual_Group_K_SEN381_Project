using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortalAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SecurityStamp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortalAccountCategories",
                columns: table => new
                {
                    AccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalAccountCategories", x => new { x.AccountId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PortalAccountCategories_PortalAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalAccounts_Username",
                table: "PortalAccounts",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalAccountCategories");

            migrationBuilder.DropTable(
                name: "PortalAccounts");
        }
    }
}
