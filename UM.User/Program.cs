using BackendApi.Extensions;
using BackendApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();

builder.Services.AddCustomServices();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddHostedService<BackendApi.Services.DatabaseSeedService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins =
            builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// ===============================
// Static Frontend
// ===============================
app.UseDefaultFiles();
app.UseStaticFiles();

// ===============================
// Error Middleware
// ===============================
app.UseMiddleware<GlobalExceptionMiddleware>();

// ===============================
// Swagger
// ===============================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Backend API v1");
});

// ===============================
// HTTP Pipeline
// ===============================

// Tạm tắt HTTPS vì đang chạy HTTP 5001
// app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// ===============================
// API Controllers
// ===============================
app.MapControllers();

app.Run();