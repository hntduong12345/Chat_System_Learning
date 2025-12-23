using Microsoft.EntityFrameworkCore;
using TestChatAPI.Models;
using TestChatAPI.Hubs;
using TestChatAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Entity Framework
builder.Services.AddDbContext<TestChatSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add SignalR with CORS support
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Enable for debugging
});

// Add custom services
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();

// IMPORTANT: Configure CORS properly for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // React app URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // REQUIRED for SignalR
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: Apply CORS before other middleware
app.UseCors("AllowSignalR");

app.UseAuthorization();

app.MapControllers();

// IMPORTANT: Map the SignalR hub - this creates the /negotiate endpoint
app.MapHub<ChatHub>("/chathub");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TestChatSystemDbContext>();
    context.Database.EnsureCreated();
}

app.Run();