using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class seedPaymentTypeTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"USE [SpatiumCMS]
GO
SET IDENTITY_INSERT [Lookup].[PaymentTypes] ON 
GO
INSERT [Lookup].[PaymentTypes] ([Id], [Name]) VALUES (1, N'Stripe')
GO
INSERT [Lookup].[PaymentTypes] ([Id], [Name]) VALUES (2, N'PayPal')
GO
INSERT [Lookup].[PaymentTypes] ([Id], [Name]) VALUES (3, N'Visa')
GO
INSERT [Lookup].[PaymentTypes] ([Id], [Name]) VALUES (4, N'MasterCard')
GO
SET IDENTITY_INSERT [Lookup].[PaymentTypes] OFF
GO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
