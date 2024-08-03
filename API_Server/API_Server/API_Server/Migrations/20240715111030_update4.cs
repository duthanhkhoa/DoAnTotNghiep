using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Server.Migrations
{
    public partial class update4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseDetail");

            migrationBuilder.DropTable(
                name: "Warehouse");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductTypeDetailId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouse_ProductTypeDetail_ProductTypeDetailId",
                        column: x => x.ProductTypeDetailId,
                        principalTable: "ProductTypeDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseDetail_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseDetail_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouse_ProductTypeDetailId",
                table: "Warehouse",
                column: "ProductTypeDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDetail_ProductId",
                table: "WarehouseDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseDetail_WarehouseId",
                table: "WarehouseDetail",
                column: "WarehouseId");
        }
    }
}
