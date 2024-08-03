using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_Server.Migrations
{
    public partial class update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetail_Invoice_InvoiceId1",
                table: "InvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceDetail_InvoiceId1",
                table: "InvoiceDetail");

            migrationBuilder.DropColumn(
                name: "InvoiceId1",
                table: "InvoiceDetail");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceId",
                table: "InvoiceDetail",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_InvoiceId",
                table: "InvoiceDetail",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetail_Invoice_InvoiceId",
                table: "InvoiceDetail",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetail_Invoice_InvoiceId",
                table: "InvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceDetail_InvoiceId",
                table: "InvoiceDetail");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "InvoiceDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceId1",
                table: "InvoiceDetail",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_InvoiceId1",
                table: "InvoiceDetail",
                column: "InvoiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetail_Invoice_InvoiceId1",
                table: "InvoiceDetail",
                column: "InvoiceId1",
                principalTable: "Invoice",
                principalColumn: "Id");
        }
    }
}
