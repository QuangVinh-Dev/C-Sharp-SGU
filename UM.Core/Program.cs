using Microsoft.EntityFrameworkCore;
using UM.Core.config;
using UM.Core.repository;
using UM.Core.sercurity;
using UM.Core.service;
using UM.Core.websocket;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller & HttpContext
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// 2. Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Security & WebSocket integration abstractions
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IServerPermissionService, ServerPermissionService>();
builder.Services.AddScoped<IServerEventPublisher, ServerEventPublisher>();

// 4. Repositories
builder.Services.AddScoped<IServerRepository, ServerRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
builder.Services.AddScoped<IServerMemberRepository, ServerMemberRepository>();
builder.Services.AddScoped<IServerRoleRepository, ServerRoleRepository>();

// 5. Services
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IChannelService, ChannelService>();

var app = builder.Build();

// Middleware xử lý lỗi tập trung
app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.MapControllers();

app.MapGet("/", () => "UM.Core Server Management Backend API is running on .NET 8.");

app.Run();
