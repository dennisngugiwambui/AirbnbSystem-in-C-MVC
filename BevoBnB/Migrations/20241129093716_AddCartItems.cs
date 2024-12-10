using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BevoBnB.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyID = table.Column<int>(type: "int", nullable: false),
                    CustomerID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItems_Properties_PropertyID",
                        column: x => x.PropertyID,
                        principalTable: "Properties",
                        principalColumn: "PropertyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4d3550d1-8fe5-4ede-b140-a22f98ae1c76", new DateTime(2024, 11, 29, 12, 37, 14, 575, DateTimeKind.Local).AddTicks(3824), "AQAAAAIAAYagAAAAEG6CAl42PmO2/GyqqcZ5wCG1IlRT/+4a8PYtSeBEezwnoi2Aq8Wl0TXcxfqnm2ULUg==", "1833a764-0be3-4510-8633-3c423b69fa57" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7266d710-2554-4e10-91d3-b1afe7a39899", new DateTime(2024, 11, 29, 12, 37, 14, 692, DateTimeKind.Local).AddTicks(1176), "AQAAAAIAAYagAAAAEJrEejRF5XxZ9NfscT6nR6WsGphNaQOTJR8ezn346CcZLwN3InekQkGyRHzw/E/sgw==", "ed2540f8-0867-4354-84ed-df2832b44192" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6958f888-1b2a-4b3b-8de6-12bc0bd3b930", new DateTime(2024, 11, 29, 12, 37, 14, 773, DateTimeKind.Local).AddTicks(2124), "AQAAAAIAAYagAAAAECbtZXwySoA1umuHsyeh4w/wGid/2pJM20y+S9DNxt8RsyP1g3pHuTi1DjKVVVDy7g==", "87f6e0fc-0917-4bc5-9fa9-ac10e340e820" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "ConcurrencyStamp", "CreatedDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a353da30-734b-4fb6-81e9-95747e75d7bb", new DateTime(2024, 11, 29, 12, 37, 14, 876, DateTimeKind.Local).AddTicks(4154), "AQAAAAIAAYagAAAAEH4iCOr8hx4QlvKAYcckkl3LdhPai43jrSme8cqvpTA+OWlatU68gLxDcWezB9E7hw==", "b481f39e-f82b-40be-9c15-d4df13c0c67d" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_PropertyID",
                table: "CartItems",
                column: "PropertyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations");

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
    }
}
