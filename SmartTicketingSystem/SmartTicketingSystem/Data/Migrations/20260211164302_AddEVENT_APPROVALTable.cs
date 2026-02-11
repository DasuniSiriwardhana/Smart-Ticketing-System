using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEVENT_APPROVALTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EVENT_APPROVAL",
                columns: table => new
                {
                    ApprovalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserID = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    DecisionNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecisionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    member_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_APPROVAL", x => x.ApprovalID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EVENT_APPROVAL");
        }
    }
}
