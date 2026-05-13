using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invoices.api.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_Cnpj",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Establishment",
                table: "Invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "EstablishmentId",
                table: "Invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Establishments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Establishments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EstablishmentId",
                table: "Invoices",
                column: "EstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Cnpj",
                table: "Establishments",
                column: "Cnpj");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Name",
                table: "Establishments",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Establishments_EstablishmentId",
                table: "Invoices",
                column: "EstablishmentId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Establishments_EstablishmentId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_EstablishmentId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "EstablishmentId",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "Invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Establishment",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Cnpj",
                table: "Invoices",
                column: "Cnpj");
        }
    }
}
