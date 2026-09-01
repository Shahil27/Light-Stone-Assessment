using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.IO;
using System.Linq;
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
            var paths = new Dictionary<string, Dictionary<string, object>>();

            foreach (var api in _apiProvider.ApiDescriptionGroups.Items.SelectMany(g => g.Items))
            {
                var rawPath = api.RelativePath ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawPath)) continue;
                var path = "/" + rawPath.TrimEnd('/');

                if (!paths.TryGetValue(path, out var methods))
                {
                    methods = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    paths[path] = methods;
                }

                var method = (api.HttpMethod ?? "GET").ToLowerInvariant();

                if (!methods.ContainsKey(method))
                {
                    // Build parameters from ApiDescription.ParameterDescriptions so path/query params are usable in the UI
                    var parameters = new List<object>();
                    object? requestBody = null;

                    foreach (var p in api.ParameterDescriptions)
                    {
                        var source = p.Source?.Id ?? string.Empty; // e.g., "Path", "Query", "Body"
                        var name = p.Name ?? "param";

                        if (string.Equals(source, "Path", StringComparison.OrdinalIgnoreCase) || string.Equals(source, "Query", StringComparison.OrdinalIgnoreCase))
                        {
                            // determine simple type
                            var typeName = p.Type?.Name ?? "String";
                            var schemaType = "string";
                            string? format = null;
                            if (string.Equals(typeName, "Int32", StringComparison.OrdinalIgnoreCase) || string.Equals(typeName, "Int64", StringComparison.OrdinalIgnoreCase)) { schemaType = "integer"; format = "int32"; }
                            else if (string.Equals(typeName, "Boolean", StringComparison.OrdinalIgnoreCase)) { schemaType = "boolean"; }
                            else if (string.Equals(typeName, "Decimal", StringComparison.OrdinalIgnoreCase) || string.Equals(typeName, "Double", StringComparison.OrdinalIgnoreCase) || string.Equals(typeName, "Single", StringComparison.OrdinalIgnoreCase)) { schemaType = "number"; }

                            parameters.Add(new Dictionary<string, object>
                            {
                                ["name"] = name,
                                ["in"] = source.Equals("Path", StringComparison.OrdinalIgnoreCase) ? "path" : "query",
                                ["required"] = source.Equals("Path", StringComparison.OrdinalIgnoreCase),
                                ["schema"] = format == null ? new { type = schemaType } : new { type = schemaType, format }
                            });
                        }
                        else if (string.Equals(source, "Body", StringComparison.OrdinalIgnoreCase) || string.Equals(source, "Form", StringComparison.OrdinalIgnoreCase))
                        {
                            // represent request body as a generic object schema
                            requestBody = new
                            {
                                content = new Dictionary<string, object>
                                {
                                    ["application/json"] = new { schema = new { type = "object" } }
                                }
                            };
                        }
                    }

                    var op = new Dictionary<string, object>
                    {
                        ["summary"] = api.ActionDescriptor.DisplayName,
                        ["responses"] = new Dictionary<string, object>
                        {
                            ["200"] = new { description = "OK" }
                        }
                    };

                    if (parameters.Count > 0) op["parameters"] = parameters;
                    if (requestBody != null) op["requestBody"] = requestBody;

                    methods[method] = op;
                }
            }

            var doc = new
            {
                openapi = "3.0.1",
                info = new { title = "Light Stone Assessment API", version = "v1" },
                paths = paths
            };

            var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            return Content(json, "application/json");
        }
    }
}
