using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventorySize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "InventoryItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "InventoryItems");
        }
    }
}
