using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Text.Json;
using System.Collections.Generic;

namespace Light_Stone_Assessment.Controllers
{
    [ApiController]
    [Route("openapi")]
    public class OpenApiController : ControllerBase
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiProvider;

        public OpenApiController(IApiDescriptionGroupCollectionProvider apiProvider)
        {
            _apiProvider = apiProvider;
        }

        [HttpGet]
        public IActionResult Get()
        {
            // Minimal OpenAPI v3 document with components for main DTOs so Swagger UI shows schemas
            var doc = new Dictionary<string, object>
            {
                ["openapi"] = "3.0.1",
                ["info"] = new Dictionary<string, object> { ["title"] = "Light Stone Assessment API", ["version"] = "v1" },
                ["paths"] = BuildPaths(),
                ["components"] = new Dictionary<string, object>
                {
                    ["schemas"] = new Dictionary<string, object>
                    {
                        ["Product"] = new { type = "object", properties = new Dictionary<string, object> { ["sku"] = new { type = "string" }, ["name"] = new { type = "string" }, ["price"] = new { type = "number", format = "decimal" }, ["stock"] = new { type = "integer" } }, required = new[] { "sku", "name", "price", "stock" } },
                        ["CreateProductDto"] = new { type = "object", properties = new Dictionary<string, object> { ["sku"] = new { type = "string" }, ["name"] = new { type = "string" }, ["price"] = new { type = "number", format = "decimal" }, ["initialStock"] = new { type = "integer" } }, required = new[] { "sku", "name", "price", "initialStock" } },
                        ["AdjustStockDto"] = new { type = "object", properties = new Dictionary<string, object> { ["delta"] = new { type = "integer" } }, required = new[] { "delta" } },
                        ["OrderItemDto"] = new { type = "object", properties = new Dictionary<string, object> { ["sku"] = new { type = "string" }, ["qty"] = new { type = "integer" }, ["unitPrice"] = new { type = "number", format = "decimal" } }, required = new[] { "sku", "qty", "unitPrice" } },
                        ["CreateOrderDto"] = new Dictionary<string, object>
                        {
                            ["type"] = "object",
                            ["properties"] = new Dictionary<string, object>
                            {
                                ["externalOrderId"] = new Dictionary<string, object> { ["type"] = "string" },
                                ["placedAt"] = new Dictionary<string, object> { ["type"] = "string", ["format"] = "date-time" },
                                ["items"] = new Dictionary<string, object>
                                {
                                    ["type"] = "array",
                                    ["items"] = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/OrderItemDto" }
                                }
                            },
                            ["required"] = new[] { "externalOrderId", "placedAt", "items" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            return Content(json, "application/json");
        }

        private object BuildPaths()
        {
            var paths = new Dictionary<string, object>();

            // Products
            paths["/api/products"] = new Dictionary<string, object>
            {
                ["post"] = new {
                    summary = "Create product",
                    requestBody = new { content = new Dictionary<string, object> { ["application/json"] = new { schema = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/CreateProductDto" } } } },
                    responses = new Dictionary<string, object> { ["201"] = new { description = "Created" } }
                }
            };

            paths["/api/products/{sku}"] = new Dictionary<string, object>
            {
                ["get"] = new {
                    summary = "Get product",
                    parameters = new[] { new { name = "sku", @in = "path", required = true, schema = new { type = "string" } } },
                    responses = new Dictionary<string, object> { ["200"] = new { description = "OK", content = new Dictionary<string, object> { ["application/json"] = new { schema = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/Product" } } } } }
                }
            };

            paths["/api/products/{sku}/stock"] = new Dictionary<string, object>
            {
                ["patch"] = new {
                    summary = "Adjust stock",
                    parameters = new[] { new { name = "sku", @in = "path", required = true, schema = new { type = "string" } } },
                    requestBody = new { content = new Dictionary<string, object> { ["application/json"] = new { schema = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/AdjustStockDto" } } } },
                    responses = new Dictionary<string, object> { ["200"] = new { description = "OK" } }
                }
            };

            // Orders
            paths["/api/orders"] = new Dictionary<string, object>
            {
                ["post"] = new {
                    summary = "Submit order",
                    requestBody = new { content = new Dictionary<string, object> { ["application/json"] = new { schema = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/CreateOrderDto" } } } },
                    responses = new Dictionary<string, object> { ["201"] = new { description = "Created" }, ["400"] = new { description = "Bad Request" } }
                }
            };

            paths["/api/sales"] = new Dictionary<string, object>
            {
                ["get"] = new {
                    summary = "Daily sales summary",
                    parameters = new[] { new { name = "start", @in = "query", required = true, schema = new { type = "string", format = "date" } }, new { name = "end", @in = "query", required = true, schema = new { type = "string", format = "date" } } },
                    responses = new Dictionary<string, object> { ["200"] = new { description = "OK" } }
                }
            };

            return paths;
        }
    }
}
