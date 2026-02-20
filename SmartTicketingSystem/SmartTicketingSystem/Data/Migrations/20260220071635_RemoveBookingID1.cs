using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBookingID1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys that might be using BookingID1
            migrationBuilder.DropForeignKey(
                name: "FK_TICKET_BOOKING_BookingID1",
                table: "TICKET");

            migrationBuilder.DropForeignKey(
                name: "FK_PAYMENT_BOOKING_BookingID1",
                table: "PAYMENT");

            // Drop the BookingID1 columns if they exist
            migrationBuilder.DropColumn(
                name: "BookingID1",
                table: "TICKET");

            migrationBuilder.DropColumn(
                name: "BookingID1",
                table: "PAYMENT");

            // Ensure the correct foreign keys exist using BookingID
            migrationBuilder.AddForeignKey(
                name: "FK_TICKET_BOOKING_BookingID",
                table: "TICKET",
                column: "BookingID",
                principalTable: "BOOKING",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PAYMENT_BOOKING_BookingID",
                table: "PAYMENT",
                column: "BookingID",
                principalTable: "BOOKING",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
