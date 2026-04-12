using B2B.Commerce.Contracts.Requisitions;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class RequisitionMappingExtensions
{
    public static RequisitionDto ToDto(this Requisition requisition)
    {
        return new RequisitionDto
        {
            Id = requisition.Id,
            UserId = requisition.UserId,
            CustomerId = requisition.CustomerId,
            Status = requisition.Status.ToString(),
            TotalAmount = requisition.TotalAmount,
            RejectionReason = requisition.RejectionReason,
            SubmittedAt = requisition.SubmittedAt,
            ResolvedAt = requisition.ResolvedAt,
            LineItemCount = requisition.LineItems.Count
        };
    }

    public static RequisitionDetailDto ToDetailDto(this Requisition requisition)
    {
        return new RequisitionDetailDto
        {
            Id = requisition.Id,
            UserId = requisition.UserId,
            CustomerId = requisition.CustomerId,
            Status = requisition.Status.ToString(),
            TotalAmount = requisition.TotalAmount,
            RejectionReason = requisition.RejectionReason,
            SubmittedAt = requisition.SubmittedAt,
            ResolvedAt = requisition.ResolvedAt,
            LineItems = requisition.LineItems.Select(li => li.ToDto()).ToList()
        };
    }

    public static RequisitionLineItemDto ToDto(this RequisitionLineItem lineItem)
    {
        return new RequisitionLineItemDto
        {
            Id = lineItem.Id,
            ProductId = lineItem.ProductId,
            Sku = lineItem.Sku,
            ProductName = lineItem.ProductName,
            UnitPrice = lineItem.UnitPrice,
            Quantity = lineItem.Quantity,
            LineTotal = lineItem.LineTotal
        };
    }
}
