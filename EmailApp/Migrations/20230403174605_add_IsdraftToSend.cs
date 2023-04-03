using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailApp.Migrations
{
    /// <inheritdoc />
    public partial class add_IsdraftToSend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "configId",
                table: "SendEmail",
                newName: "ConfigId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SendEmail",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SendEmail");

            migrationBuilder.RenameColumn(
                name: "ConfigId",
                table: "SendEmail",
                newName: "configId");
        }
    }
}
