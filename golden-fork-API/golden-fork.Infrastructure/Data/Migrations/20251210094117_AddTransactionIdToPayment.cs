using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace golden_fork.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIdToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3140));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3144));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3145));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3146));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3464));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3469));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3470));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3346));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3351));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 10, 9, 41, 16, 656, DateTimeKind.Utc).AddTicks(3572));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(204));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(207));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(229));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(230));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(377));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(385));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(386));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(300));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(305));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 8, 9, 35, 35, 550, DateTimeKind.Utc).AddTicks(445));
        }
    }
}
