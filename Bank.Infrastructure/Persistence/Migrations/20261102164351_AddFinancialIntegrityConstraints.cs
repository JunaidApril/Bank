using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_WithdrawalTransactions_Amount_Positive",
                table: "WithdrawalTransactions",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WithdrawalTransactions_Balance_Consistent",
                table: "WithdrawalTransactions",
                sql: "[BalanceAfter] = [BalanceBefore] - [Amount]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WithdrawalTransactions_Balance_NonNegative",
                table: "WithdrawalTransactions",
                sql: "[BalanceBefore] >= 0 AND [BalanceAfter] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accounts_Balance_NonNegative",
                table: "Accounts",
                sql: "[Balance] >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_IdempotencyRecords_Accounts_AccountId",
                table: "IdempotencyRecords",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdempotencyRecords_Accounts_AccountId",
                table: "IdempotencyRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WithdrawalTransactions_Amount_Positive",
                table: "WithdrawalTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WithdrawalTransactions_Balance_Consistent",
                table: "WithdrawalTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WithdrawalTransactions_Balance_NonNegative",
                table: "WithdrawalTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accounts_Balance_NonNegative",
                table: "Accounts");
        }
    }
}
