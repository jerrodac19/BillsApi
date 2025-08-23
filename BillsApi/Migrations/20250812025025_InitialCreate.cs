using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillsApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "BalanceMonitor",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Amount = table.Column<decimal>(type: "money", nullable: true),
            //        Updated = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BalanceMonitor", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Groups",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Groups", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AccountBalance",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Amount = table.Column<decimal>(type: "money", nullable: true),
            //        Updated = table.Column<DateTime>(type: "datetime", nullable: true),
            //        GroupID = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK__AccountB__3214EC07092CD002", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AccountBalance_Groups",
            //            column: x => x.GroupID,
            //            principalTable: "Groups",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BillConfigurations",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DefaultAmount = table.Column<decimal>(type: "smallmoney", nullable: true),
            //        DefaultPayEarly = table.Column<short>(type: "smallint", nullable: true),
            //        Website = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        GroupID = table.Column<int>(type: "int", nullable: true),
            //        MonthlyDueDate = table.Column<int>(type: "int", nullable: true),
            //        DefaultTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        TransactionRegex = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BillConfigurations", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_BillConfigurations_Groups",
            //            column: x => x.GroupID,
            //            principalTable: "Groups",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DailyAllowance",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Allowance = table.Column<decimal>(type: "money", nullable: true),
            //        Date = table.Column<DateTime>(type: "datetime", nullable: true),
            //        GroupID = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK__DailyAll__3214EC078F068AC8", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DailyAllowance_Groups",
            //            column: x => x.GroupID,
            //            principalTable: "Groups",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Transactions",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Withdrawal = table.Column<decimal>(type: "money", nullable: true),
            //        Deposit = table.Column<decimal>(type: "money", nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        GroupID = table.Column<int>(type: "int", nullable: false),
            //        Date = table.Column<DateTime>(type: "datetime", nullable: true),
            //        AccountName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
            //        Status = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Transactions", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Transactions_Groups",
            //            column: x => x.GroupID,
            //            principalTable: "Groups",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        GroupID = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK__tmp_ms_x__3214EC0782CF78EA", x => x.Id);
            //        table.ForeignKey(
            //            name: "GroupId",
            //            column: x => x.GroupID,
            //            principalTable: "Groups",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Bills",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Amount = table.Column<decimal>(type: "smallmoney", nullable: true),
            //        DueDate = table.Column<DateOnly>(type: "date", nullable: true),
            //        Payed = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
            //        PayEarly = table.Column<short>(type: "smallint", nullable: true),
            //        Active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
            //        Updated = table.Column<DateTime>(type: "datetime", nullable: false),
            //        ConfigurationID = table.Column<int>(type: "int", nullable: false),
            //        Valid = table.Column<bool>(type: "bit", nullable: false),
            //        CalendarEventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        ReminderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        TaskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK__Bills__3214EC07F83B15FB", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Bills_BillConfigurations",
            //            column: x => x.ConfigurationID,
            //            principalTable: "BillConfigurations",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Income",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        StartDate = table.Column<DateOnly>(type: "date", nullable: false),
            //        Frequency = table.Column<int>(type: "int", nullable: true),
            //        Amount = table.Column<decimal>(type: "money", nullable: false),
            //        UserID = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK__Income__3214EC07ED3D60D2", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Income_Users",
            //            column: x => x.UserID,
            //            principalTable: "Users",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AccountBalance_GroupID",
            //    table: "AccountBalance",
            //    column: "GroupID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_BillConfigurations_GroupID",
            //    table: "BillConfigurations",
            //    column: "GroupID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Bills_ConfigurationID",
            //    table: "Bills",
            //    column: "ConfigurationID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DailyAllowance_GroupID",
            //    table: "DailyAllowance",
            //    column: "GroupID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Income_UserID",
            //    table: "Income",
            //    column: "UserID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Transactions_GroupID",
            //    table: "Transactions",
            //    column: "GroupID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_GroupID",
            //    table: "Users",
            //    column: "GroupID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "AccountBalance");

            //migrationBuilder.DropTable(
            //    name: "BalanceMonitor");

            //migrationBuilder.DropTable(
            //    name: "Bills");

            //migrationBuilder.DropTable(
            //    name: "DailyAllowance");

            //migrationBuilder.DropTable(
            //    name: "Income");

            //migrationBuilder.DropTable(
            //    name: "Transactions");

            //migrationBuilder.DropTable(
            //    name: "BillConfigurations");

            //migrationBuilder.DropTable(
            //    name: "Users");

            //migrationBuilder.DropTable(
            //    name: "Groups");
        }
    }
}
