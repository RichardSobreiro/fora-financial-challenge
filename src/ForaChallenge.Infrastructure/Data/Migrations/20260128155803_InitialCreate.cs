using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForaChallenge.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cik = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCompanies = table.Column<int>(type: "int", nullable: false),
                    ProcessedCompanies = table.Column<int>(type: "int", nullable: false),
                    SuccessfulCompanies = table.Column<int>(type: "int", nullable: false),
                    FailedCompanies = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LockedByInstance = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsStartupImport = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CurrentJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedByInstance = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Frame = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomeRecords_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ImportLocks",
                columns: new[] { "Id", "CurrentJobId", "LockedAt", "LockedByInstance" },
                values: new object[] { 1, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Cik",
                table: "Companies",
                column: "Cik",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_StartedAt",
                table: "ImportJobs",
                column: "StartedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_IncomeRecords_CompanyId",
                table: "IncomeRecords",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeRecords_CompanyId_Year",
                table: "IncomeRecords",
                columns: new[] { "CompanyId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropTable(
                name: "ImportLocks");

            migrationBuilder.DropTable(
                name: "IncomeRecords");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
