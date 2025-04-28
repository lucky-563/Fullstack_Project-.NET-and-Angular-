using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class request1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedByEmail",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "PaymentRequests");

            migrationBuilder.RenameColumn(
                name: "RejectedOn",
                table: "PaymentRequests",
                newName: "ActionOn");

            migrationBuilder.RenameColumn(
                name: "RejectedByEmail",
                table: "PaymentRequests",
                newName: "ActionByEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActionOn",
                table: "PaymentRequests",
                newName: "RejectedOn");

            migrationBuilder.RenameColumn(
                name: "ActionByEmail",
                table: "PaymentRequests",
                newName: "RejectedByEmail");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByEmail",
                table: "PaymentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "PaymentRequests",
                type: "datetime2",
                nullable: true);
        }
    }
}
