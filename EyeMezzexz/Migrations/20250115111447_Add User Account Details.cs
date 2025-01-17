using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EyeMezzexz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccountDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccountDetails",
                columns: table => new
                {
                    AccountDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayrollNumber = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsManager = table.Column<bool>(type: "bit", nullable: false),
                    EmailId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    AgreementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Zipcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthenticLeave = table.Column<int>(type: "int", nullable: true),
                    NINumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFixedHours = table.Column<bool>(type: "bit", nullable: false),
                    IsFixedSalary = table.Column<bool>(type: "bit", nullable: false),
                    AgreedHours = table.Column<int>(type: "int", nullable: true),
                    PayrollHours = table.Column<int>(type: "int", nullable: true),
                    PayrollHoursRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CashHours = table.Column<int>(type: "int", nullable: true),
                    CashHoursRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPayBack = table.Column<bool>(type: "bit", nullable: false),
                    PayBackTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayBackAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HolidayDaysYearly = table.Column<int>(type: "int", nullable: true),
                    HolidayHoursYearly = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountDetails", x => x.AccountDetailsId);
                    table.ForeignKey(
                        name: "FK_UserAccountDetails_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountDetails_CountryId",
                table: "UserAccountDetails",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountDetails");
        }
    }
}
