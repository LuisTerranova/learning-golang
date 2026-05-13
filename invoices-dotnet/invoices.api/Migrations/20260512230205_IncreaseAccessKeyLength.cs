using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invoices.api.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseAccessKeyLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessKey",
                table: "Invoices",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(44)",
                oldMaxLength: 44,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessKey",
                table: "Invoices",
                type: "character varying(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
