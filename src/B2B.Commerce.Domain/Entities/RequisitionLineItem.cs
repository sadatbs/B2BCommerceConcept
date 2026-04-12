namespace B2B.Commerce.Domain.Entities;

public class RequisitionLineItem
{
    public Guid Id { get; private set; }
    public Guid RequisitionId { get; private set; }
    public Guid ProductId { get; private set; }  // reference only — no FK enforced

    // Snapshot at submit time — these never change after creation
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }  // frozen price at submit
    public int Quantity { get; private set; }

    // Navigation
    public Requisition Requisition { get; private set; } = null!;

    public decimal LineTotal => UnitPrice * Quantity;

    private RequisitionLineItem() { } // EF Core

    internal static RequisitionLineItem CreateSnapshot(
        Guid requisitionId,
        Guid productId,
        string sku,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        return new RequisitionLineItem
        {
            Id = Guid.NewGuid(),
            RequisitionId = requisitionId,
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
