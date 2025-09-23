using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCreationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Transactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Transactions");
        }
    }
}
