using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class projects2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerMail",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerMail",
                table: "Projects");
        }
    }
}
