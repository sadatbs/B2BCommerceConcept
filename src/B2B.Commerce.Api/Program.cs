using B2B.Commerce.Api.Endpoints;
using B2B.Commerce.Api.Middleware;
using B2B.Commerce.Api.Validators;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Infrastructure.Data;
using B2B.Commerce.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "B2B Commerce API",
        Version = "v1",
        Description = "Headless B2B Commerce Engine"
    });
});

// Add DbContext
builder.Services.AddDbContext<CommerceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();

// Add Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

var app = builder.Build();

// Global exception handling — must be first
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapProductEndpoints();
app.MapCatalogEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
