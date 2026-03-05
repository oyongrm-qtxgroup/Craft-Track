using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RejectRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RejectReason = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    InventoryItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RejectRecords_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RejectRecords_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RejectRecords_Date",
                table: "RejectRecords",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_RejectRecords_InventoryItemId",
                table: "RejectRecords",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RejectRecords_OrderItemId",
                table: "RejectRecords",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RejectRecords_RejectReason",
                table: "RejectRecords",
                column: "RejectReason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RejectRecords");
        }
    }
}
