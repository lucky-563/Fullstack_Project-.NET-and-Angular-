using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class paymentrequests2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectedByEmail",
                table: "PaymentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedOn",
                table: "PaymentRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedByEmail",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "RejectedOn",
                table: "PaymentRequests");
        }
    }
}
