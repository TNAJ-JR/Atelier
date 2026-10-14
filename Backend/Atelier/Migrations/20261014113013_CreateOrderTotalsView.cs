using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atelier.Migrations
{
    /// <inheritdoc />
    public partial class CreateOrderTotalsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW [dbo].[order_totals] AS
SELECT o.id AS order_id,
       COALESCE(SUM(i.quantity * i.unit_price_cents), 0) AS total_cents,
       COUNT(i.product_id) AS line_count
FROM dbo.orders AS o
LEFT JOIN dbo.order_items AS i ON i.order_id = o.id
GROUP BY o.id;
" );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS [dbo].[order_totals];");

        }
    }
}
