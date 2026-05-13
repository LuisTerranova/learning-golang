using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace invoices.api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawInvoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RawId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: true),
                    AccessKey = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    Establishment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ParserVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ParseErrors = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_RawInvoices_RawId",
                        column: x => x.RawId,
                        principalTable: "RawInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AccessKey",
                table: "Invoices",
                column: "AccessKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Cnpj",
                table: "Invoices",
                column: "Cnpj");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Date",
                table: "Invoices",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RawId",
                table: "Invoices",
                column: "RawId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "RawInvoices");
        }
    }
}
