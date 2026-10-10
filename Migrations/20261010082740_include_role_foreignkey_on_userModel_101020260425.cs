using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MunPath.Migrations
{
    /// <inheritdoc />
    public partial class include_role_foreignkey_on_userModel_101020260425 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "User",
                type: "character varying(30)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleName",
                table: "User",
                column: "RoleName");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Roles_RoleName",
                table: "User",
                column: "RoleName",
                principalTable: "Roles",
                principalColumn: "role_name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Roles_RoleName",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_RoleName",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "User");
        }
    }
}
