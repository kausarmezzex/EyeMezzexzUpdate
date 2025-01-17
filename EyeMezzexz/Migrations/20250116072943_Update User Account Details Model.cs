using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAccountDetailsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "UserAccountDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "UserAccountDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNames",
                table: "UserAccountDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Documents",
                table: "UserAccountDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "DocumentNames",
                table: "UserAccountDetails");

            migrationBuilder.DropColumn(
                name: "Documents",
                table: "UserAccountDetails");
        }
    }
}
