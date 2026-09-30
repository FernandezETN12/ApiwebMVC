using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiwebMVC.Migrations
{
    /// <inheritdoc />
    public partial class MakePizzaIngredientsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Ingredientes",
                table: "Pizzas",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE `Pizzas` SET `Ingredientes` = '[]' WHERE `Ingredientes` IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Ingredientes",
                table: "Pizzas",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
