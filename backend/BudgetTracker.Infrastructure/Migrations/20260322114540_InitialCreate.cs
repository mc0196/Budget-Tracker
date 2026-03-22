using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BudgetTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RowsImported = table.Column<int>(type: "INTEGER", nullable: false),
                    RowsSkipped = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategorizationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Keyword = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorizationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorizationRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalText = table.Column<string>(type: "TEXT", nullable: true),
                    SourceFile = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsManuallyCreated = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "BankName", "CreatedAt", "Currency", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "EUR", "My Bank Account" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "Icon", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "#22c55e", "💰", "Salary", "Income" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "#84cc16", "💻", "Freelance", "Income" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "#f97316", "🛒", "Groceries", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "#3b82f6", "🚌", "Transport", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "#a855f7", "⚡", "Utilities", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "#ec4899", "🎬", "Entertainment", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "#14b8a6", "💊", "Health", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "#ef4444", "🍽️", "Dining", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "#f59e0b", "🛍️", "Shopping", "Expense" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "#6b7280", null, "Other", "Expense" }
                });

            migrationBuilder.InsertData(
                table: "CategorizationRules",
                columns: new[] { "Id", "CategoryId", "IsActive", "Keyword", "Priority" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001"), true, "salary", 10 },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001"), true, "payroll", 10 },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000003"), true, "supermarket", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000003"), true, "grocery", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000004"), true, "uber", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000006"), new Guid("10000000-0000-0000-0000-000000000004"), true, "metro", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000007"), new Guid("10000000-0000-0000-0000-000000000006"), true, "netflix", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000008"), new Guid("10000000-0000-0000-0000-000000000006"), true, "spotify", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000007"), true, "pharmacy", 5 },
                    { new Guid("20000000-0000-0000-0000-000000000010"), new Guid("10000000-0000-0000-0000-000000000008"), true, "restaurant", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategorizationRules_CategoryId",
                table: "CategorizationRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportLogs_FileHash",
                table: "ImportLogs",
                column: "FileHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date",
                table: "Transactions",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorizationRules");

            migrationBuilder.DropTable(
                name: "ImportLogs");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
