using B2B.Commerce.Domain.Enums;

namespace B2B.Commerce.Domain.Entities;

public class Requisition : AggregateRoot
{
    private readonly List<RequisitionLineItem> _lineItems = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }       // buyer who submitted
    public Guid CustomerId { get; private set; }   // company context
    public RequisitionStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }  // set on Approve or Reject

    public IReadOnlyList<RequisitionLineItem> LineItems => _lineItems.AsReadOnly();

    private Requisition() { } // EF Core

    /// <summary>
    /// Creates a Requisition by snapshotting a Cart.
    /// Prices are frozen at this moment — they will not change even if tier prices update.
    /// </summary>
    public static Requisition CreateFromCart(
        Cart cart,
        IReadOnlyDictionary<Guid, (string Sku, string Name, decimal Price)> productSnapshot)
    {
        if (cart.IsEmpty)
            throw new InvalidOperationException("Cannot submit an empty cart as a requisition");

        var requisition = new Requisition
        {
            Id = Guid.NewGuid(),
            UserId = cart.UserId,
            CustomerId = cart.CustomerId,
            Status = RequisitionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var cartItem in cart.Items)
        {
            if (!productSnapshot.TryGetValue(cartItem.ProductId, out var product))
                throw new InvalidOperationException($"Product {cartItem.ProductId} not found in snapshot");

            var lineItem = RequisitionLineItem.CreateSnapshot(
                requisition.Id,
                cartItem.ProductId,
                product.Sku,
                product.Name,
                product.Price,  // price is frozen here
                cartItem.Quantity);

            requisition._lineItems.Add(lineItem);
        }

        requisition.TotalAmount = requisition._lineItems.Sum(i => i.LineTotal);

        return requisition;
    }

    /// <summary>
    /// Approve the requisition. Called by Approver role or by system on auto-approve.
    /// </summary>
    public void Approve()
    {
        if (Status != RequisitionStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot approve a requisition in {Status} status");

        Status = RequisitionStatus.Approved;
        ResolvedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reject the requisition. Called by Approver role only.
    /// </summary>
    public void Reject(string reason)
    {
        if (Status != RequisitionStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot reject a requisition in {Status} status");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = RequisitionStatus.Rejected;
        RejectionReason = reason.Trim();
        ResolvedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Called by system after Order is created from this Requisition.
    /// </summary>
    public void MarkOrdered()
    {
        if (Status != RequisitionStatus.Approved)
            throw new InvalidOperationException(
                "Cannot mark as Ordered — Requisition must be Approved first");

        Status = RequisitionStatus.Ordered;
    }
}
