using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMeet.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoredIsActiveUseComputed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Meetings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Meetings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
