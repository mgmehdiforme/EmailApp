using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailApp.Migrations
{
    /// <inheritdoc />
    public partial class changetopop3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UseSSLForImap",
                table: "EmailConfig",
                newName: "UseSSLForPop3");

            migrationBuilder.RenameColumn(
                name: "ImapServer",
                table: "EmailConfig",
                newName: "Pop3Server");

            migrationBuilder.RenameColumn(
                name: "ImapPort",
                table: "EmailConfig",
                newName: "Pop3Port");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UseSSLForPop3",
                table: "EmailConfig",
                newName: "UseSSLForImap");

            migrationBuilder.RenameColumn(
                name: "Pop3Server",
                table: "EmailConfig",
                newName: "ImapServer");

            migrationBuilder.RenameColumn(
                name: "Pop3Port",
                table: "EmailConfig",
                newName: "ImapPort");
        }
    }
}
