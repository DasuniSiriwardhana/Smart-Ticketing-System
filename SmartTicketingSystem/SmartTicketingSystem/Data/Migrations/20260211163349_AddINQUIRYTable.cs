using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddINQUIRYTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "issuedAt",
                table: "TICKET",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateTable(
                name: "INQUIRY",
                columns: table => new
                {
                    InquiryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HandleByUserID = table.Column<int>(type: "int", nullable: false),
                    HandleAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseNote = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INQUIRY", x => x.InquiryID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INQUIRY");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "issuedAt",
                table: "TICKET",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
