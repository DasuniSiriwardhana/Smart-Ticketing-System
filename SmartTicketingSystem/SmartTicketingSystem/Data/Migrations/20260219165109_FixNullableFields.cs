using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicketingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "USER",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalID",
                table: "USER",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ReviewedByUserID",
                table: "PUBLIC_EVENT_REQUEST",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "organizerInfo",
                table: "EVENT",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "onlineLink",
                table: "EVENT",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "maplink",
                table: "EVENT",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalID",
                table: "EVENT",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Agenda",
                table: "EVENT",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "AccessibilityInfo",
                table: "EVENT",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.CreateIndex(
                name: "IX_PUBLIC_EVENT_REQUEST_ReviewedByUserID",
                table: "PUBLIC_EVENT_REQUEST",
                column: "ReviewedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_ApprovalID",
                table: "EVENT",
                column: "ApprovalID");

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_createdByUserID",
                table: "EVENT",
                column: "createdByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_EVENT_EVENT_APPROVAL_ApprovalID",
                table: "EVENT",
                column: "ApprovalID",
                principalTable: "EVENT_APPROVAL",
                principalColumn: "ApprovalID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EVENT_USER_createdByUserID",
                table: "EVENT",
                column: "createdByUserID",
                principalTable: "USER",
                principalColumn: "member_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PUBLIC_EVENT_REQUEST_USER_ReviewedByUserID",
                table: "PUBLIC_EVENT_REQUEST",
                column: "ReviewedByUserID",
                principalTable: "USER",
                principalColumn: "member_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EVENT_EVENT_APPROVAL_ApprovalID",
                table: "EVENT");

            migrationBuilder.DropForeignKey(
                name: "FK_EVENT_USER_createdByUserID",
                table: "EVENT");

            migrationBuilder.DropForeignKey(
                name: "FK_PUBLIC_EVENT_REQUEST_USER_ReviewedByUserID",
                table: "PUBLIC_EVENT_REQUEST");

            migrationBuilder.DropIndex(
                name: "IX_PUBLIC_EVENT_REQUEST_ReviewedByUserID",
                table: "PUBLIC_EVENT_REQUEST");

            migrationBuilder.DropIndex(
                name: "IX_EVENT_ApprovalID",
                table: "EVENT");

            migrationBuilder.DropIndex(
                name: "IX_EVENT_createdByUserID",
                table: "EVENT");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "USER",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalID",
                table: "USER",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReviewedByUserID",
                table: "PUBLIC_EVENT_REQUEST",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organizerInfo",
                table: "EVENT",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "onlineLink",
                table: "EVENT",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "maplink",
                table: "EVENT",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalID",
                table: "EVENT",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Agenda",
                table: "EVENT",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessibilityInfo",
                table: "EVENT",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
