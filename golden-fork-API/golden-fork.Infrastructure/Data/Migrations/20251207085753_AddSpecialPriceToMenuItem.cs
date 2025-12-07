using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace golden_fork.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialPriceToMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SpecialPrice",
                table: "MenuItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 7, 8, 57, 53, 378, DateTimeKind.Utc).AddTicks(4896));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 7, 8, 57, 53, 378, DateTimeKind.Utc).AddTicks(4900));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 7, 8, 57, 53, 378, DateTimeKind.Utc).AddTicks(5072));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialPrice",
                table: "MenuItems");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 30, 10, 45, 19, 677, DateTimeKind.Utc).AddTicks(9490));

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 30, 10, 45, 19, 677, DateTimeKind.Utc).AddTicks(9495));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 30, 10, 45, 19, 677, DateTimeKind.Utc).AddTicks(9642));
        }
    }
}
