using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using B2B.Commerce.Domain.Events;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void CreateFromCart_ShouldCreateOrderWithItems()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        var product = Product.Create("TEST-001", "Test Product", 50.00m);
        typeof(Product).GetProperty("Id")!.SetValue(product, productId);

        var order = Order.CreateFromCart(cart, id => id == productId ? product : null);

        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(2);
        order.Items[0].UnitPrice.Should().Be(50.00m);
        order.Items[0].ProductName.Should().Be("Test Product");
        order.TotalAmount.Should().Be(100.00m);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void CreateFromCart_EmptyCart_ShouldThrow()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => Order.CreateFromCart(cart, _ => null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*empty cart*");
    }

    [Fact]
    public void CreateFromCart_ShouldRaiseOrderPlacedEvent()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 1);

        var product = Product.Create("TEST-001", "Test Product", 100.00m);
        typeof(Product).GetProperty("Id")!.SetValue(product, productId);

        var order = Order.CreateFromCart(cart, id => id == productId ? product : null);

        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents[0].Should().BeOfType<OrderPlacedEvent>();

        var @event = (OrderPlacedEvent)order.DomainEvents[0];
        @event.OrderId.Should().Be(order.Id);
        @event.TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public void Confirm_FromPending_ShouldSucceed()
    {
        var order = CreateTestOrder();

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_FromConfirmed_ShouldThrow()
    {
        var order = CreateTestOrder();
        order.Confirm();

        var act = () => order.Confirm();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot confirm*");
    }

    [Fact]
    public void Complete_FromConfirmed_ShouldSucceed()
    {
        var order = CreateTestOrder();
        order.Confirm();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_FromPending_ShouldThrow()
    {
        var order = CreateTestOrder();

        var act = () => order.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot complete*");
    }

    private static Order CreateTestOrder()
    {
        var cart = Cart.Create(Guid.NewGuid(), Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 1);

        var product = Product.Create("TEST-001", "Test Product", 100.00m);
        typeof(Product).GetProperty("Id")!.SetValue(product, productId);

        return Order.CreateFromCart(cart, id => id == productId ? product : null);
    }
}
