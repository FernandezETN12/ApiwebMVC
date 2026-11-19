using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiwebMVC.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCalleAlturaACliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Altura",
                table: "Clientes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Calle",
                table: "Clientes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Altura",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Calle",
                table: "Clientes");
        }
    }
}
