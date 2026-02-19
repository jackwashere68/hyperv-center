using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HyperVCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClusters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClusterId",
                table: "hyperv_hosts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "clusters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CredentialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clusters_credentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "credentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hyperv_hosts_ClusterId",
                table: "hyperv_hosts",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_clusters_CredentialId",
                table: "clusters",
                column: "CredentialId");

            migrationBuilder.AddForeignKey(
                name: "FK_hyperv_hosts_clusters_ClusterId",
                table: "hyperv_hosts",
                column: "ClusterId",
                principalTable: "clusters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hyperv_hosts_clusters_ClusterId",
                table: "hyperv_hosts");

            migrationBuilder.DropTable(
                name: "clusters");

            migrationBuilder.DropIndex(
                name: "IX_hyperv_hosts_ClusterId",
                table: "hyperv_hosts");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "hyperv_hosts");
        }
    }
}
