using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Saas_Dormitory.DAL.Migrations.DormitoryDb
{
    public partial class ConvertTenantIdGuidToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new INT column
            migrationBuilder.AddColumn<int>(
                name: "TenantId_New",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            // 2. COPY / MAP DATA (Guid → int)
            // IMPORTANT: Replace mapping logic if you have real tenant table mapping
            migrationBuilder.Sql(@"
                UPDATE ""AspNetUsers""
                SET ""TenantId_New"" = 
                    CASE
                        WHEN ""TenantId"" IS NULL THEN 0
                        ELSE 0
                    END;
            ");

            // 3. Drop GUID column
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers"
            );

            // 4. Rename new column
            migrationBuilder.RenameColumn(
                name: "TenantId_New",
                table: "AspNetUsers",
                newName: "TenantId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Add GUID back
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId_Old",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty
            );

            // 2. Remove INT col
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers"
            );

            // 3. Rename col
            migrationBuilder.RenameColumn(
                name: "TenantId_Old",
                table: "AspNetUsers",
                newName: "TenantId"
            );
        }
    }
}
