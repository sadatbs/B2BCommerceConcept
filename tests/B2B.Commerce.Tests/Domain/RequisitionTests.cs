using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class RequisitionTests
{
    [Fact]
    public void CreateFromCart_ShouldFreezeProductPrices()
    {
        var (cart, productId) = CreateCartWithItem(100m, quantity: 2);
        var snapshot = BuildSnapshot(productId, "WIDGET-001", "Widget", 100m);

        var requisition = Requisition.CreateFromCart(cart, snapshot);

        requisition.LineItems.Should().HaveCount(1);
        requisition.LineItems[0].UnitPrice.Should().Be(100m);
        requisition.LineItems[0].Quantity.Should().Be(2);
        requisition.TotalAmount.Should().Be(200m);
        requisition.Status.Should().Be(RequisitionStatus.Submitted);
    }

    [Fact]
    public void CreateFromCart_EmptyCart_ShouldThrow()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        var snapshot = new Dictionary<Guid, (string Sku, string Name, decimal Price)>();

        var act = () => Requisition.CreateFromCart(cart, snapshot);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*empty cart*");
    }

    [Fact]
    public void Approve_FromSubmitted_ShouldSucceed()
    {
        var requisition = CreateSubmittedRequisition();

        requisition.Approve();

        requisition.Status.Should().Be(RequisitionStatus.Approved);
        requisition.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_AlreadyApproved_ShouldThrow()
    {
        var requisition = CreateSubmittedRequisition();
        requisition.Approve();

        var act = () => requisition.Approve();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot approve*");
    }

    [Fact]
    public void Reject_FromSubmitted_ShouldSucceed()
    {
        var requisition = CreateSubmittedRequisition();

        requisition.Reject("Budget exceeded for Q2");

        requisition.Status.Should().Be(RequisitionStatus.Rejected);
        requisition.RejectionReason.Should().Be("Budget exceeded for Q2");
        requisition.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_EmptyReason_ShouldThrow()
    {
        var requisition = CreateSubmittedRequisition();

        var act = () => requisition.Reject("   ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*reason*");
    }

    [Fact]
    public void MarkOrdered_AfterApprove_ShouldSucceed()
    {
        var requisition = CreateSubmittedRequisition();
        requisition.Approve();

        requisition.MarkOrdered();

        requisition.Status.Should().Be(RequisitionStatus.Ordered);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (Cart cart, Guid productId) CreateCartWithItem(decimal price, int quantity)
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, quantity);
        return (cart, productId);
    }

    private static IReadOnlyDictionary<Guid, (string Sku, string Name, decimal Price)> BuildSnapshot(
        Guid productId, string sku, string name, decimal price)
    {
        return new Dictionary<Guid, (string Sku, string Name, decimal Price)>
        {
            [productId] = (sku, name, price)
        };
    }

    private static Requisition CreateSubmittedRequisition()
    {
        var (cart, productId) = CreateCartWithItem(50m, quantity: 1);
        var snapshot = BuildSnapshot(productId, "TEST-001", "Test Product", 50m);
        return Requisition.CreateFromCart(cart, snapshot);
    }
}
