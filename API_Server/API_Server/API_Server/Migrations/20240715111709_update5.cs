using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Server.Migrations
{
    public partial class update5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailCombo");

            migrationBuilder.DropTable(
                name: "Combo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Combo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetailCombo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboId = table.Column<int>(type: "int", nullable: false),
                    ProductId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailCombo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailCombo_Combo_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailCombo_Product_ProductId1",
                        column: x => x.ProductId1,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailCombo_ComboId",
                table: "DetailCombo",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailCombo_ProductId1",
                table: "DetailCombo",
                column: "ProductId1");
        }
    }
}
