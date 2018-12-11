using Microsoft.EntityFrameworkCore.Migrations;

namespace PostItCore.Migrations
{
    public partial class AddIsPostToFavors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPost",
                table: "Favors",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPost",
                table: "Favors");
        }
    }
}
