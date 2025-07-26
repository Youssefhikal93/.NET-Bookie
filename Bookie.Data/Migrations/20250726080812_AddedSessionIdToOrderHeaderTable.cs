using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookie.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedSessionIdToOrderHeaderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "orderTotal",
                table: "OrderHeaders",
                newName: "OrderTotal");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "OrderTotal",
                table: "OrderHeaders",
                newName: "orderTotal");
        }
    }
}
