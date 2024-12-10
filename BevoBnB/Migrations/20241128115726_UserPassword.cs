using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BevoBnB.Migrations
{
    /// <inheritdoc />
    public partial class UserPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2cbacda0-f321-4e1c-acc2-1f711f9fb289", new DateTime(2024, 11, 28, 14, 57, 25, 73, DateTimeKind.Local).AddTicks(9441), "AQAAAAIAAYagAAAAEFMP8XdRjcKyS/b5k1HNfKCSbF72SZF8SAjabYlEiGp1ut4rDCDVpPWpW6F/6TojPw==", "fd49fcc9-65b5-430c-a9bc-42c778a22670" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c2204ee2-d669-4896-9518-6ce6e6213693", new DateTime(2024, 11, 28, 14, 57, 25, 163, DateTimeKind.Local).AddTicks(2198), "AQAAAAIAAYagAAAAEMl3e3tUEd6rG3rWEPobT+nrRSCyjuB2GkP0wzYa5NbxZpUDjVVdKSB4kWDBdERLQw==", "668fb8fd-5a00-4ecd-931e-489c6b46f268" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "91200c31-c286-4710-b718-3c44759523c7", new DateTime(2024, 11, 28, 14, 57, 25, 246, DateTimeKind.Local).AddTicks(1662), "AQAAAAIAAYagAAAAEOvZe4qvVhjf18QG59SXAY4KRNAlURJpmxL5Gj7sAVHVjhngnz1oYnB3GkABiRsyFw==", "458f3aa5-e409-4d15-8c51-f22b17f3514b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "07aecae8-466d-425e-87bb-8f936dbffcad", new DateTime(2024, 11, 28, 14, 57, 25, 329, DateTimeKind.Local).AddTicks(7352), "AQAAAAIAAYagAAAAEA0srt0H1vOM/akbyWflloum3YlME1Guzh01Zx7oqqqSskgg7Mw5Gw3Io87TBkndVg==", "a4b56b64-b0c5-4e2e-b9e6-04eacbe4f830" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3dbc4902-67a3-4bc1-89e0-cede4c06738f", new DateTime(2024, 11, 28, 14, 50, 14, 643, DateTimeKind.Local).AddTicks(8080), null, "f101b2d5-b6c7-4ede-8674-56345b4786dc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ecec82e3-17f5-423b-9cba-d84fe654de09", new DateTime(2024, 11, 28, 14, 50, 14, 644, DateTimeKind.Local).AddTicks(6598), null, "54ffb3b9-461b-4e4e-805a-b70928f4dcb5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1b164634-49dd-4b68-9f12-6bd1476d01c7", new DateTime(2024, 11, 28, 14, 50, 14, 644, DateTimeKind.Local).AddTicks(6643), null, "5ae47a68-6c4d-44e5-a3d6-4bb96e0cc780" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3f52d2ec-96f1-4486-a229-d6855afd7f5c", new DateTime(2024, 11, 28, 14, 50, 14, 644, DateTimeKind.Local).AddTicks(6651), null, "ff4f632c-87fa-4e3f-9e78-7548cfe6b2df" });
        }
    }
}
