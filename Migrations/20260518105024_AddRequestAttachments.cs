using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeopleOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentDataUrl",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptDataUrl",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptFileName",
                table: "ExpenseClaims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentDataUrl",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ReceiptDataUrl",
                table: "ExpenseClaims");

            migrationBuilder.DropColumn(
                name: "ReceiptFileName",
                table: "ExpenseClaims");
        }
    }
}
