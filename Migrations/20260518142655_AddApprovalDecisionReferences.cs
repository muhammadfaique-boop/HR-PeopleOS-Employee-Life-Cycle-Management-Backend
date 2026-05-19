using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeopleOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalDecisionReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAt",
                table: "ApprovalTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceId",
                table: "ApprovalTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "ApprovalTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecidedAt",
                table: "ApprovalTasks");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "ApprovalTasks");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "ApprovalTasks");
        }
    }
}
