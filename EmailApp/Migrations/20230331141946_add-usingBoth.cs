using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailApp.Migrations
{
    /// <inheritdoc />
    public partial class addusingBoth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImapPort",
                table: "EmailConfig",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImapServer",
                table: "EmailConfig",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseImap",
                table: "EmailConfig",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseSSLForImap",
                table: "EmailConfig",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImapPort",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "ImapServer",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "UseImap",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "UseSSLForImap",
                table: "EmailConfig");
        }
    }
}
