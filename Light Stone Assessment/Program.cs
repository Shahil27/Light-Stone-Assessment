using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Register API explorer (used by our custom OpenAPI generator)
builder.Services.AddEndpointsApiExplorer();

// Configure DbContext
builder.Services.AddDbContext<Light_Stone_Assessment.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// seed sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<Light_Stone_Assessment.Data.AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Products.Any())
        {
            db.Products.AddRange(new[] {
                new Light_Stone_Assessment.Models.Product { Sku = "SKU-001", Name = "Wireless Mouse", Price = 24.99m, Stock = 50 },
                new Light_Stone_Assessment.Models.Product { Sku = "SKU-002", Name = "Mechanical Keyboard", Price = 79.99m, Stock = 20 },
                new Light_Stone_Assessment.Models.Product { Sku = "SKU-003", Name = "USB-C Cable", Price = 9.99m, Stock = 200 }
            });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
// Serve a lightweight Swagger UI that loads the /openapi document produced by OpenApiController
app.MapGet("/swagger", () => Results.Content(@"<!doctype html>
<html>
  <head>
    <meta charset='utf-8'/>
    <title>Swagger UI - Light Stone Assessment</title>
    <link rel='stylesheet' href='https://unpkg.com/swagger-ui-dist@4/swagger-ui.css' />
    <style>body { margin:0; }</style>
  </head>
  <body>
    <div id='swagger-ui'></div>
    <script src='https://unpkg.com/swagger-ui-dist@4/swagger-ui-bundle.js'></script>
    <script>
      window.onload = function() {
        SwaggerUIBundle({
          url: '/openapi',
          dom_id: '#swagger-ui'
        });
      };
    </script>
  </body>
</html>", "text/html"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// When running locally without the debugger (dotnet run), open the Swagger UI automatically.
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            if (!Debugger.IsAttached)
            {
                var url = app.Urls.FirstOrDefault() ?? "https://localhost:7041";
                var swaggerUrl = url.TrimEnd('/') + "/swagger";
                Process.Start(new ProcessStartInfo { FileName = swaggerUrl, UseShellExecute = true });
            }
        }
        catch {
            // ignore failures to open browser
        }
    });
}

app.Run();
