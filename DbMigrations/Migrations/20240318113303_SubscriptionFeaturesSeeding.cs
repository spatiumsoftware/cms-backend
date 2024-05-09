using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionFeaturesSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"USE [SpatiumCMS]
                                    GO
                                    SET IDENTITY_INSERT [dbo].[Subscription] ON 
                                    GO
                                    INSERT [dbo].[Subscription] ([Id], [Title], [SubTitle], [Price], [Duration], [IsDefault], [NumberOfUsers], [SEO_Usage], [CreationDate], [IsDeleted], [NumberOfPosts], [StorageCapacity]) VALUES (1, N'Free', N'Best For Personal Usage', CAST(0.00 AS Decimal(18, 2)), 1, 1, 1, 0, CAST(N'2024-03-18T00:00:00.0000000' AS DateTime2), 0, 20, 256)
                                    GO
                                    INSERT [dbo].[Subscription] ([Id], [Title], [SubTitle], [Price], [Duration], [IsDefault], [NumberOfUsers], [SEO_Usage], [CreationDate], [IsDeleted], [NumberOfPosts], [StorageCapacity]) VALUES (2, N'Standard', N'Best For Personal Usage', CAST(5.00 AS Decimal(18, 2)), 1, 0, 3, 0, CAST(N'2024-03-18T00:00:00.0000000' AS DateTime2), 0, 50, 1024)
                                    GO
                                    INSERT [dbo].[Subscription] ([Id], [Title], [SubTitle], [Price], [Duration], [IsDefault], [NumberOfUsers], [SEO_Usage], [CreationDate], [IsDeleted], [NumberOfPosts], [StorageCapacity]) VALUES (3, N'Advanced', N'Best For Personal Usage', CAST(10.00 AS Decimal(18, 2)), 1, 0, 5, 1, CAST(N'2024-03-18T00:00:00.0000000' AS DateTime2), 0, 100, 3072)
                                    GO
                                    INSERT [dbo].[Subscription] ([Id], [Title], [SubTitle], [Price], [Duration], [IsDefault], [NumberOfUsers], [SEO_Usage], [CreationDate], [IsDeleted], [NumberOfPosts], [StorageCapacity]) VALUES (4, N'Premium', N'Best For Personal Usage', CAST(15.00 AS Decimal(18, 2)), 1, 0, 10, 1, CAST(N'2024-03-18T00:00:00.0000000' AS DateTime2), 0, NULL, 5120)
                                    GO
                                    SET IDENTITY_INSERT [dbo].[Subscription] OFF
                                    GO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
