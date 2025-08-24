using BillsApi.Configuration;
using BillsApi.Models;
using BillsApi.Repositories;
using BillsApi.Repositories.Common;
using BillsApi.Repositories.UnitOfWork;
using BillsApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure the GoogleTasksApiOptions from appsettings.json
builder.Services.Configure<GoogleTasksApiOptions>(builder.Configuration.GetSection("GoogleTasksApi"));

// Register the HttpClient for the service
builder.Services.AddHttpClient<GoogleTaskService>();

// Register the Unit of Work and all repositories for dependency injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
builder.Services.AddScoped<IBalanceMonitorRepository, BalanceMonitorRepository>();
builder.Services.AddScoped<IBillConfigurationRepository, BillConfigurationRepository>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register all services
builder.Services.AddScoped<BalanceAnalyticsService>();
builder.Services.AddScoped<BillService>();

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