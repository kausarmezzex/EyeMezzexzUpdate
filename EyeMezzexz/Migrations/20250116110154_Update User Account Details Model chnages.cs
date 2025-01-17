using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAccountDetailsModelchnages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccountDetails_Countries_CountryId",
                table: "UserAccountDetails");

            migrationBuilder.DropIndex(
                name: "IX_UserAccountDetails_CountryId",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "EmailId",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "UserAccountDetails");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserAccountDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountDetails_UserId",
                table: "UserAccountDetails",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccountDetails_AspNetUsers_UserId",
                table: "UserAccountDetails",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccountDetails_AspNetUsers_UserId",
                table: "UserAccountDetails");

            migrationBuilder.DropIndex(
                name: "IX_UserAccountDetails_UserId",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserAccountDetails");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "UserAccountDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailId",
                table: "UserAccountDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserAccountDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserAccountDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "UserAccountDetails",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountDetails_CountryId",
                table: "UserAccountDetails",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccountDetails_Countries_CountryId",
                table: "UserAccountDetails",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }
    }
}
