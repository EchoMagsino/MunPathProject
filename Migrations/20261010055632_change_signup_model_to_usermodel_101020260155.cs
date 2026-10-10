using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MunPath.Migrations
{
    /// <inheritdoc />
    public partial class change_signup_model_to_usermodel_101020260155 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_sign_up",
                table: "sign_up");

            migrationBuilder.RenameTable(
                name: "sign_up",
                newName: "User");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "sign_up");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sign_up",
                table: "sign_up",
                column: "Id");
        }
    }
}
