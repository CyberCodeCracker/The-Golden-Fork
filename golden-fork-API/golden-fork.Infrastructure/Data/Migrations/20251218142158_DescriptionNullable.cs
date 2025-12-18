using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace golden_fork.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(814));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(816));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(816));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(817));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(1001));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(1005));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(1006));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(928));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 14, 21, 58, 549, DateTimeKind.Utc).AddTicks(1060));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9472));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9473));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9474));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9658));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9662));

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9663));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9591));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9594));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 18, 9, 52, 7, 426, DateTimeKind.Utc).AddTicks(9720));
        }
    }
}
