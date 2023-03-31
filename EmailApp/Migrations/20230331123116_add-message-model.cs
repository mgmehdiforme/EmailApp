using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailApp.Migrations
{
    /// <inheritdoc />
    public partial class addmessagemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Port",
                table: "EmailConfig",
                newName: "SmtpPort");

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
                name: "UseSSLForImap",
                table: "EmailConfig",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseSSLForSmtp",
                table: "EmailConfig",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flagged = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailConfigId = table.Column<int>(type: "int", nullable: false),
                    JsonMessageMetadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessage_EmailConfig_EmailConfigId",
                        column: x => x.EmailConfigId,
                        principalTable: "EmailConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_EmailConfigId",
                table: "EmailMessage",
                column: "EmailConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "ImapPort",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "ImapServer",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "UseSSLForImap",
                table: "EmailConfig");

            migrationBuilder.DropColumn(
                name: "UseSSLForSmtp",
                table: "EmailConfig");

            migrationBuilder.RenameColumn(
                name: "SmtpPort",
                table: "EmailConfig",
                newName: "Port");
        }
    }
}
