using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.Membership
{
    /// <inheritdoc />
    public partial class AddMemberKycProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KycStatus",
                table: "Members",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsNextOfKin",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MemberKycProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IprsVerified = table.Column<bool>(type: "bit", nullable: false),
                    KraVerified = table.Column<bool>(type: "bit", nullable: false),
                    CrbChecked = table.Column<bool>(type: "bit", nullable: false),
                    AmlCleared = table.Column<bool>(type: "bit", nullable: false),
                    LastVerifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VerificationNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberKycProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberKycProfiles_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_TenantId_AppUserId",
                table: "Members",
                columns: new[] { "TenantId", "AppUserId" },
                unique: true,
                filter: "[AppUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MemberKycProfiles_MemberId",
                table: "MemberKycProfiles",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberKycProfiles_TenantId_MemberId",
                table: "MemberKycProfiles",
                columns: new[] { "TenantId", "MemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberKycProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Members_TenantId_AppUserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "KycStatus",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "IsNextOfKin",
                table: "Beneficiaries");
        }
    }
}
