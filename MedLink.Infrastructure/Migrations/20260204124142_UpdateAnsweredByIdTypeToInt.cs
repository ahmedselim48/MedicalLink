using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnsweredByIdTypeToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_UserProfiles_AnsweredByProfileId1",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_AnsweredByProfileId1",
                table: "FAQs");

            migrationBuilder.DropColumn(
                name: "AnsweredByProfileId1",
                table: "FAQs");

            migrationBuilder.AlterColumn<int>(
                name: "AnsweredByProfileId",
                table: "FAQs",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_AnsweredByProfileId",
                table: "FAQs",
                column: "AnsweredByProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FAQs_UserProfiles_AnsweredByProfileId",
                table: "FAQs",
                column: "AnsweredByProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FAQs_UserProfiles_AnsweredByProfileId",
                table: "FAQs");

            migrationBuilder.DropIndex(
                name: "IX_FAQs_AnsweredByProfileId",
                table: "FAQs");

            migrationBuilder.AlterColumn<string>(
                name: "AnsweredByProfileId",
                table: "FAQs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
    }
}
