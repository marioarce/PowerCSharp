using WebSample.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddSwaggerServices();

var app = builder.Build();

// Map all PowerCSharp demo endpoints
app.MapPowerCSharpDemoEndpoints();

// Enable Swagger UI
app.UseSwaggerUI();

app.Run();
