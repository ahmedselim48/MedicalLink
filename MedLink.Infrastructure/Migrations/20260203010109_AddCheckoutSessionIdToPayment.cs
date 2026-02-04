using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutSessionIdToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutSessionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutSessionId",
                table: "Payments");
        }
    }
}
