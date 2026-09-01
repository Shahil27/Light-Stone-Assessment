using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Light_Stone_Assessment.Data;
using Light_Stone_Assessment.Models;

namespace Light_Stone_Assessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext db, ILogger<OrdersController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public record OrderItemDto(string Sku, int Qty, decimal UnitPrice);
        public record CreateOrderDto(string ExternalOrderId, DateTime PlacedAt, List<OrderItemDto> Items);

        [HttpPost]
        public async Task<IActionResult> Submit(CreateOrderDto dto)
        {
            _logger.LogInformation("Order submission attempt {ExternalOrderId} with {Items} items", dto.ExternalOrderId, dto.Items.Count);

            using var tx = await _db.Database.BeginTransactionAsync();

            // idempotency check
            var exists = await _db.Orders.AnyAsync(o => o.ExternalOrderId == dto.ExternalOrderId);
            if (exists)
            {
                _logger.LogInformation("Duplicate order {ExternalOrderId}", dto.ExternalOrderId);
                await tx.RollbackAsync();
                return Ok(new { outcome = "duplicate" });
            }

            var order = new Order { ExternalOrderId = dto.ExternalOrderId, PlacedAt = dto.PlacedAt };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // decrement stock using conditional updates to avoid oversell
            foreach (var item in dto.Items)
            {
                var rows = await _db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Products SET Stock = Stock - {item.Qty} WHERE Sku = {item.Sku} AND Stock >= {item.Qty}");
                if (rows == 0)
                {
                    _logger.LogWarning("Order {ExternalOrderId} rejected due to insufficient stock for SKU {Sku}", dto.ExternalOrderId, item.Sku);
                    await tx.RollbackAsync();
                    return BadRequest(new { outcome = "rejected", reason = $"Insufficient stock for {item.Sku}" });
                }

                var oi = new OrderItem { OrderId = order.Id, Sku = item.Sku, Qty = item.Qty, UnitPrice = decimal.Round(item.UnitPrice, 2) };
                _db.OrderItems.Add(oi);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Order {ExternalOrderId} accepted", dto.ExternalOrderId);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, new { outcome = "accepted", orderId = order.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var o = await _db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            return Ok(o);
        }
    }
}
