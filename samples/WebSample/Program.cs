using WebSample.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddSwaggerServices();

// Add the PowerCSharp Features Framework (Cache + Sanitization feature modules)
builder.Services.AddPowerCSharpFeatures(builder.Configuration);

var app = builder.Build();

// Run the Features Framework pipeline hook (bridges the Sanitization engine's configuration)
app.UsePowerCSharpFeatures();

// Map all PowerCSharp demo endpoints
app.MapPowerCSharpDemoEndpoints();

// Enable Swagger UI
app.UseSwaggerUI();

app.Run();
