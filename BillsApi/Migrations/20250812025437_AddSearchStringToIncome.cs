using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchStringToIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchString",
                table: "Income",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchString",
                table: "Income");
        }
    }
}
