using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailApp.Migrations
{
    /// <inheritdoc />
    public partial class removepop3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pop3Port",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "Pop3Server",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "UseSSLForPop3",
                table: "EmailConfig");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Pop3Port",
                table: "EmailConfig",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Pop3Server",
                table: "EmailConfig",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseSSLForPop3",
                table: "EmailConfig",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
