using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HyperVCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHyperVManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Host",
                table: "virtual_machines");

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "virtual_machines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HyperVHostId",
                table: "virtual_machines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "LastSyncError",
                table: "hyperv_hosts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "hyperv_hosts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsVersion",
                table: "hyperv_hosts",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessorCount",
                table: "hyperv_hosts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TotalMemoryBytes",
                table: "hyperv_hosts",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_virtual_machines_HyperVHostId",
                table: "virtual_machines",
                column: "HyperVHostId");

            migrationBuilder.AddForeignKey(
                name: "FK_virtual_machines_hyperv_hosts_HyperVHostId",
                table: "virtual_machines",
                column: "HyperVHostId",
                principalTable: "hyperv_hosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_virtual_machines_hyperv_hosts_HyperVHostId",
                table: "virtual_machines");

            migrationBuilder.DropIndex(
                name: "IX_virtual_machines_HyperVHostId",
                table: "virtual_machines");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "virtual_machines");

            migrationBuilder.DropColumn(
                name: "HyperVHostId",
                table: "virtual_machines");

            migrationBuilder.DropColumn(
                name: "LastSyncError",
                table: "hyperv_hosts");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "hyperv_hosts");

            migrationBuilder.DropColumn(
                name: "OsVersion",
                table: "hyperv_hosts");

            migrationBuilder.DropColumn(
                name: "ProcessorCount",
                table: "hyperv_hosts");

            migrationBuilder.DropColumn(
                name: "TotalMemoryBytes",
                table: "hyperv_hosts");

            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "virtual_machines",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
