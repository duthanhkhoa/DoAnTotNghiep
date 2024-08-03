using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Server.Migrations
{
    public partial class update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportInvoice_PaymentMethod_PaymentMethodId",
                table: "ImportInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_PaymentMethod_PaymentMethodId",
                table: "Invoice");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_PaymentMethodId",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_ImportInvoice_PaymentMethodId",
                table: "ImportInvoice");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "Invoice",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "ImportInvoice",
                newName: "PaymentMethod");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Invoice",
                newName: "PaymentMethodId");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "ImportInvoice",
                newName: "PaymentMethodId");

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_PaymentMethodId",
                table: "Invoice",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportInvoice_PaymentMethodId",
                table: "ImportInvoice",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportInvoice_PaymentMethod_PaymentMethodId",
                table: "ImportInvoice",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_PaymentMethod_PaymentMethodId",
                table: "Invoice",
                column: "PaymentMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
