using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEVENT_CATEGORYTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EVENT_CATEGORY",
                columns: table => new
                {
                    categoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CATEGORY", x => x.categoryID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EVENT_CATEGORY");
        }
    }
}
