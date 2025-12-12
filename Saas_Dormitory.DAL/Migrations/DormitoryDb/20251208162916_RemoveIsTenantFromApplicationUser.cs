using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saas_Dormitory.DAL.Migrations.DormitoryDb
{
    public partial class RemoveIsTenantFromApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTenant",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTenant",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
