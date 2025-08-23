using BillsApi.Configuration;
using BillsApi.Models;
using BillsApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure the GoogleTasksApiOptions from appsettings.json
builder.Services.Configure<GoogleTasksApiOptions>(builder.Configuration.GetSection("GoogleTasksApi"));

// Register the HttpClient for the service
builder.Services.AddHttpClient<GoogleTaskService>();

builder.Services.AddScoped<BalanceAnalyticsService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BillsApiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
