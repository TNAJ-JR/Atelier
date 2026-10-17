using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Migrations
{
    /// <inheritdoc />
    public partial class ModificationdespropriétésdelaclasseApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nom",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "users");
        }
    }
}
