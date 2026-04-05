using B2B.Commerce.Api.Filters;
using B2B.Commerce.Api.Mapping;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Invoices;
using B2B.Commerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace B2B.Commerce.Api.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .WithOpenApi();

        group.MapGet("/", GetInvoices)
            .WithName("GetInvoices")
            .WithSummary("Get all invoices with pagination");

        group.MapGet("/{id:guid}", GetInvoiceById)
            .WithName("GetInvoiceById")
            .WithSummary("Get an invoice by ID");

        group.MapGet("/number/{invoiceNumber}", GetInvoiceByNumber)
            .WithName("GetInvoiceByNumber")
            .WithSummary("Get an invoice by invoice number");

        group.MapPost("/", CreateInvoice)
            .WithName("CreateInvoice")
            .WithSummary("Create a new invoice")
            .AddEndpointFilter<ValidationFilter<CreateInvoiceRequest>>();

        group.MapPut("/{id:guid}", UpdateInvoice)
            .WithName("UpdateInvoice")
            .WithSummary("Update invoice details")
            .AddEndpointFilter<ValidationFilter<UpdateInvoiceRequest>>();

        group.MapDelete("/{id:guid}", DeleteInvoice)
            .WithName("DeleteInvoice")
            .WithSummary("Delete an invoice");
    }

    private static async Task<Ok<PagedResponse<InvoiceDto>>> GetInvoices(
        [AsParameters] PagedRequest request,
        IInvoiceRepository repository,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            request.Skip,
            request.PageSize,
            cancellationToken);

        var response = new PagedResponse<InvoiceDto>
        {
            Items = items.ToDtos(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<InvoiceDto>, NotFound<ErrorResponse>>> GetInvoiceById(
        Guid id,
        IInvoiceRepository repository,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Invoice", id.ToString()));

        return TypedResults.Ok(invoice.ToDto());
    }

    private static async Task<Results<Ok<InvoiceDto>, NotFound<ErrorResponse>>> GetInvoiceByNumber(
        string invoiceNumber,
        IInvoiceRepository repository,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByInvoiceNumberAsync(invoiceNumber, cancellationToken);

        if (invoice is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Invoice", invoiceNumber));

        return TypedResults.Ok(invoice.ToDto());
    }

    private static async Task<Results<Created<InvoiceDto>, Conflict<ErrorResponse>>> CreateInvoice(
        CreateInvoiceRequest request,
        IInvoiceRepository repository,
        CancellationToken cancellationToken)
    {
        if (await repository.InvoiceNumberExistsAsync(request.InvoiceNumber, cancellationToken))
        {
            return TypedResults.Conflict(
                ErrorResponse.Conflict($"An invoice with number '{request.InvoiceNumber}' already exists."));
        }

        var invoice = request.ToEntity();
        await repository.AddAsync(invoice, cancellationToken);

        return TypedResults.Created($"/api/invoices/{invoice.Id}", invoice.ToDto());
    }

    private static async Task<Results<Ok<InvoiceDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> UpdateInvoice(
        Guid id,
        UpdateInvoiceRequest request,
        IInvoiceRepository repository,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Invoice", id.ToString()));

        try
        {
            var status = request.Status.ToInvoiceStatus();
            invoice.UpdateDetails(request.DueDate);
            invoice.UpdateStatus(status);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(new ErrorResponse { Code = "INVALID_REQUEST", Message = ex.Message });
        }

        await repository.UpdateAsync(invoice, cancellationToken);

        return TypedResults.Ok(invoice.ToDto());
    }

    private static async Task<Results<NoContent, NotFound<ErrorResponse>>> DeleteInvoice(
        Guid id,
        IInvoiceRepository repository,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
            return TypedResults.NotFound(ErrorResponse.NotFound("Invoice", id.ToString()));

        await repository.DeleteAsync(id, cancellationToken);

        return TypedResults.NoContent();
    }
}
