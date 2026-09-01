using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Light_Stone_Assessment.Data;

namespace Light_Stone_Assessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SalesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (end < start) return BadRequest(new { message = "end must be >= start" });

            // normalize to date-only boundaries (inclusive)
            var startDate = start.Date;
            var endDate = end.Date.AddDays(1).AddTicks(-1);

            var query = from oi in _db.OrderItems
                        join o in _db.Orders on oi.OrderId equals o.Id
                        where o.PlacedAt >= startDate && o.PlacedAt <= endDate
                        select new { o.PlacedAt, oi.Sku, oi.Qty, oi.UnitPrice };

            var items = await query.ToListAsync();

            var days = items
                .GroupBy(x => x.PlacedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    products = g.GroupBy(x => x.Sku).Select(pg => new
                    {
                        sku = pg.Key,
                        qty_sold = pg.Sum(x => x.Qty),
                        gross_sales = decimal.Round(pg.Sum(x => x.Qty * x.UnitPrice), 2)
                    }).ToList(),
                    totals = new
                    {
                        qty_sold = g.Sum(x => x.Qty),
                        gross_sales = decimal.Round(g.Sum(x => x.Qty * x.UnitPrice), 2)
                    }
                }).ToList();

            var result = new
            {
                start_date = startDate.ToString("yyyy-MM-dd"),
                end_date = endDate.Date.ToString("yyyy-MM-dd"),
                days
            };

            return Ok(result);
        }
    }
}
