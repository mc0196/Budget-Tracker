using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BudgetTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentsSavingsCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use INSERT OR IGNORE to be idempotent — these categories may already
            // exist in databases that applied an earlier version of this migration.
            migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO ""Categories"" (""Id"", ""Color"", ""Icon"", ""Name"", ""Type"")
                VALUES ('10000000-0000-0000-0000-000000000011', '#0ea5e9', '📈', 'Investments', 'Expense');

                INSERT OR IGNORE INTO ""Categories"" (""Id"", ""Color"", ""Icon"", ""Name"", ""Type"")
                VALUES ('10000000-0000-0000-0000-000000000012', '#8b5cf6', '🏦', 'Savings', 'Expense');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));
        }
    }
}
