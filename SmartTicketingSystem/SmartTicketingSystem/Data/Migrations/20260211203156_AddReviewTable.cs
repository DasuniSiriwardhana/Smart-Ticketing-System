using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REVIEW",
                columns: table => new
                {
                    ReviewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    eventID = table.Column<int>(type: "int", nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false),
                    Ratings = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isVerifiedAttendee = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REVIEW", x => x.ReviewID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REVIEW");
        }
    }
}
