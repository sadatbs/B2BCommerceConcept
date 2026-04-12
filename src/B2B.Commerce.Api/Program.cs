using B2B.Commerce.Api.Endpoints;
using B2B.Commerce.Api.Middleware;
using B2B.Commerce.Api.Validators;
using B2B.Commerce.Domain.Events;
using B2B.Commerce.Domain.Interfaces;
using B2B.Commerce.Domain.Services;
using B2B.Commerce.Infrastructure.Data;
using B2B.Commerce.Infrastructure.Events;
using B2B.Commerce.Infrastructure.Repositories;
using B2B.Commerce.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// Add Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPriceTierRepository, PriceTierRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IRequisitionRepository, RequisitionRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Domain Services
builder.Services.AddScoped<IPricingService, PricingService>();

// Event Dispatcher
builder.Services.AddScoped<IEventDispatcher, InMemoryEventDispatcher>();

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
app.MapCartEndpoints();
app.MapOrderEndpoints();
app.MapCustomerEndpoints();
app.MapPriceTierEndpoints();
app.MapInvoiceEndpoints();
app.MapRequisitionEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
