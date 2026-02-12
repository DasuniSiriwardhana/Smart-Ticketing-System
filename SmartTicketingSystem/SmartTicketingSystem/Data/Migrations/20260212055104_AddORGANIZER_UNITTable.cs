using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddORGANIZER_UNITTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ORGANIZER_UNIT",
                columns: table => new
                {
                    OrganizerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    unitTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORGANIZER_UNIT", x => x.OrganizerID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORGANIZER_UNIT");
        }
    }
}
