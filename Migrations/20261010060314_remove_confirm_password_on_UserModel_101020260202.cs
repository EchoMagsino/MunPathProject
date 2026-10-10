using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MunPath.Migrations
{
    /// <inheritdoc />
    public partial class remove_confirm_password_on_UserModel_101020260202 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirm_password",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "confirm_password",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
