using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MunPath.Migrations
{
    /// <inheritdoc />
    public partial class update_role_and_user_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Roles_RoleName",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "User",
                newName: "role_name");

            migrationBuilder.RenameIndex(
                name: "IX_User_RoleName",
                table: "User",
                newName: "IX_User_role_name");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Roles_role_name",
                table: "User",
                column: "role_name",
                principalTable: "Roles",
                principalColumn: "role_name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Roles_role_name",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "role_name",
                table: "User",
                newName: "RoleName");

            migrationBuilder.RenameIndex(
                name: "IX_User_role_name",
                table: "User",
                newName: "IX_User_RoleName");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Roles_RoleName",
                table: "User",
                column: "RoleName",
                principalTable: "Roles",
                principalColumn: "role_name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
