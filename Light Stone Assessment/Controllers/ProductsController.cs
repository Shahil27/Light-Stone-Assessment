using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Light_Stone_Assessment.Data;
using Light_Stone_Assessment.Models;

namespace Light_Stone_Assessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductsController(AppDbContext db)
        {
            _db = db;
        }

        public record CreateProductDto(string Sku, string Name, decimal Price, int InitialStock);

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var exists = await _db.Products.AnyAsync(p => p.Sku == dto.Sku);
            if (exists) return Conflict(new { message = "SKU already exists" });

            var p = new Product { Sku = dto.Sku, Name = dto.Name, Price = decimal.Round(dto.Price, 2), Stock = dto.InitialStock };
            _db.Products.Add(p);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { sku = p.Sku }, p);
        }

        [HttpGet("{sku}")]
        public async Task<IActionResult> Get(string sku)
        {
            var p = await _db.Products.FindAsync(sku);
            if (p == null) return NotFound();
            return Ok(p);
        }

        public record AdjustStockDto(int Delta);

        [HttpPatch("{sku}/stock")]
        public async Task<IActionResult> AdjustStock(string sku, AdjustStockDto dto)
        {
            var p = await _db.Products.FindAsync(sku);
            if (p == null) return NotFound();
            p.Stock += dto.Delta;
            if (p.Stock < 0) return BadRequest(new { message = "Stock cannot be negative" });
            await _db.SaveChangesAsync();
            return Ok(p);
        }
    }
}
