using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileToFAQRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnsweredByProfileId",
                table: "FAQs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnsweredByProfileId1",
                table: "FAQs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_AnsweredByProfileId1",
                table: "FAQs",
                column: "AnsweredByProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_UserProfiles_AnsweredByProfileId1",
                table: "FAQs",
                column: "AnsweredByProfileId1",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_UserProfiles_AnsweredByProfileId1",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_AnsweredByProfileId1",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "AnsweredByProfileId",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "AnsweredByProfileId1",
                table: "FAQs");
        }
    }
}
