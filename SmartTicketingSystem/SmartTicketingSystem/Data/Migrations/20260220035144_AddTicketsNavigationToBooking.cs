using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketsNavigationToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "PAYMENT");

            migrationBuilder.AddColumn<int>(
                name: "BookingID1",
                table: "TICKET",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "PROMO_CODE",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "PAYMENT",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CancelledAt",
                table: "BOOKING",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CancellationReason",
                table: "BOOKING",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateIndex(
                name: "IX_WAITING_LIST_EventID",
                table: "WAITING_LIST",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_WAITING_LIST_member_id",
                table: "WAITING_LIST",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_TICKET_BookingID",
                table: "TICKET",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_TICKET_BookingID1",
                table: "TICKET",
                column: "BookingID1");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BookingID",
                table: "PAYMENT",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_PROMO_BookingCodeID",
                table: "BOOKING_PROMO",
                column: "BookingCodeID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_PROMO_BookingID",
                table: "BOOKING_PROMO",
                column: "BookingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_ITEM_BookingID",
                table: "BOOKING_ITEM",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_ITEM_TicketTypeID",
                table: "BOOKING_ITEM",
                column: "TicketTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_EventID",
                table: "BOOKING",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_member_id",
                table: "BOOKING",
                column: "member_id");

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_EVENT_EventID",
                table: "BOOKING",
                column: "EventID",
                principalTable: "EVENT",
                principalColumn: "eventID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_USER_member_id",
                table: "BOOKING",
                column: "member_id",
                principalTable: "USER",
                principalColumn: "member_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_ITEM_BOOKING_BookingID",
                table: "BOOKING_ITEM",
                column: "BookingID",
                principalTable: "BOOKING",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_ITEM_TICKET_TYPE_TicketTypeID",
                table: "BOOKING_ITEM",
                column: "TicketTypeID",
                principalTable: "TICKET_TYPE",
                principalColumn: "TicketID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_PROMO_BOOKING_BookingID",
                table: "BOOKING_PROMO",
                column: "BookingID",
                principalTable: "BOOKING",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_PROMO_PROMO_CODE_BookingCodeID",
                table: "BOOKING_PROMO",
                column: "BookingCodeID",
                principalTable: "PROMO_CODE",
                principalColumn: "PromoCodeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PAYMENT_BOOKING_BookingID",
                table: "PAYMENT",
                column: "BookingID",
                principalTable: "BOOKING",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TICKET_BOOKING_BookingID",
                table: "TICKET",
                column: "BookingID",
                principalTable: "BOOKING",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TICKET_BOOKING_BookingID1",
                table: "TICKET",
                column: "BookingID1",
                principalTable: "BOOKING",
                principalColumn: "BookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_WAITING_LIST_EVENT_EventID",
                table: "WAITING_LIST",
                column: "EventID",
                principalTable: "EVENT",
                principalColumn: "eventID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WAITING_LIST_USER_member_id",
                table: "WAITING_LIST",
                column: "member_id",
                principalTable: "USER",
                principalColumn: "member_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BOOKING_EVENT_EventID",
                table: "BOOKING");

            migrationBuilder.DropForeignKey(
                name: "FK_BOOKING_USER_member_id",
                table: "BOOKING");

            migrationBuilder.DropForeignKey(
                name: "FK_BOOKING_ITEM_BOOKING_BookingID",
                table: "BOOKING_ITEM");

            migrationBuilder.DropForeignKey(
                name: "FK_BOOKING_ITEM_TICKET_TYPE_TicketTypeID",
                table: "BOOKING_ITEM");

            migrationBuilder.DropForeignKey(
                name: "FK_BOOKING_PROMO_BOOKING_BookingID",
                table: "BOOKING_PROMO");

            migrationBuilder.DropForeignKey(
                name: "FK_BOOKING_PROMO_PROMO_CODE_BookingCodeID",
                table: "BOOKING_PROMO");

            migrationBuilder.DropForeignKey(
                name: "FK_PAYMENT_BOOKING_BookingID",
                table: "PAYMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_TICKET_BOOKING_BookingID",
                table: "TICKET");

            migrationBuilder.DropForeignKey(
                name: "FK_TICKET_BOOKING_BookingID1",
                table: "TICKET");

            migrationBuilder.DropForeignKey(
                name: "FK_WAITING_LIST_EVENT_EventID",
                table: "WAITING_LIST");

            migrationBuilder.DropForeignKey(
                name: "FK_WAITING_LIST_USER_member_id",
                table: "WAITING_LIST");

            migrationBuilder.DropIndex(
                name: "IX_WAITING_LIST_EventID",
                table: "WAITING_LIST");

            migrationBuilder.DropIndex(
                name: "IX_WAITING_LIST_member_id",
                table: "WAITING_LIST");

            migrationBuilder.DropIndex(
                name: "IX_TICKET_BookingID",
                table: "TICKET");

            migrationBuilder.DropIndex(
                name: "IX_TICKET_BookingID1",
                table: "TICKET");

            migrationBuilder.DropIndex(
                name: "IX_PAYMENT_BookingID",
                table: "PAYMENT");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_PROMO_BookingCodeID",
                table: "BOOKING_PROMO");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_PROMO_BookingID",
                table: "BOOKING_PROMO");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_ITEM_BookingID",
                table: "BOOKING_ITEM");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_ITEM_TicketTypeID",
                table: "BOOKING_ITEM");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_EventID",
                table: "BOOKING");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_member_id",
                table: "BOOKING");

            migrationBuilder.DropColumn(
                name: "BookingID1",
                table: "TICKET");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "PROMO_CODE",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "PAYMENT",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "PAYMENT",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CancelledAt",
                table: "BOOKING",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CancellationReason",
                table: "BOOKING",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
