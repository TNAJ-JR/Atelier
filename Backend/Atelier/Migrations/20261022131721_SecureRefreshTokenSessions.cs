using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Migrations
{
    /// <inheritdoc />
    public partial class SecureRefreshTokenSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Refuse rather than truncate existing data or silently discard duplicates.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [RefreshTokens] WHERE DATALENGTH([TokenHash]) > 128)
    THROW 50001, 'Un hash existant dépasse 64 caractères. Vérifier les données avant migration.', 1;
IF EXISTS (SELECT [TokenHash] FROM [RefreshTokens] GROUP BY [TokenHash] HAVING COUNT(*) > 1)
    THROW 50002, 'Des hash de refresh token sont dupliqués. Vérifier les données avant migration.', 1;
");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "RefreshTokens",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "RefreshTokens",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
