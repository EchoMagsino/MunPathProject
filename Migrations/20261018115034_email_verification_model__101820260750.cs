using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MunPath.Migrations
{
    /// <inheritdoc />
    public partial class email_verification_model__101820260750 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "attemps",
                table: "email_verifications",
                newName: "attempts");

            migrationBuilder.AlterColumn<bool>(
                name: "is_used",
                table: "email_verifications",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "attempts",
                table: "email_verifications",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_email_verifications_user_id",
                table: "email_verifications",
                column: "user_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_email_verifications_attempts",
                table: "email_verifications",
                sql: "attempts >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_email_verifications_User_user_id",
                table: "email_verifications",
                column: "user_id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_email_verifications_User_user_id",
                table: "email_verifications");

            migrationBuilder.DropIndex(
                name: "IX_email_verifications_user_id",
                table: "email_verifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_email_verifications_attempts",
                table: "email_verifications");

            migrationBuilder.RenameColumn(
                name: "attempts",
                table: "email_verifications",
                newName: "attemps");

            migrationBuilder.AlterColumn<bool>(
                name: "is_used",
                table: "email_verifications",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "attemps",
                table: "email_verifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }
    }
}
