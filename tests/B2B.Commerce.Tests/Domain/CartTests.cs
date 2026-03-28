using B2B.Commerce.Domain.Entities;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class CartTests
{
    [Fact]
    public void Create_ShouldInitializeEmptyCart()
    {
        var cart = Cart.Create();

        cart.Id.Should().NotBeEmpty();
        cart.Items.Should().BeEmpty();
        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void AddItem_ShouldAddNewItem()
    {
        var cart = Cart.Create();
        var productId = Guid.NewGuid();

        cart.AddItem(productId, 2);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].ProductId.Should().Be(productId);
        cart.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_SameProduct_ShouldIncreaseQuantity()
    {
        var cart = Cart.Create();
        var productId = Guid.NewGuid();

        cart.AddItem(productId, 2);
        cart.AddItem(productId, 3);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_ZeroQuantity_ShouldThrow()
    {
        var cart = Cart.Create();

        var act = () => cart.AddItem(Guid.NewGuid(), 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateQuantity()
    {
        var cart = Cart.Create();
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        cart.UpdateItemQuantity(productId, 5);

        cart.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_ToZero_ShouldRemoveItem()
    {
        var cart = Cart.Create();
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        cart.UpdateItemQuantity(productId, 0);

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItem()
    {
        var cart = Cart.Create();
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 2);

        cart.RemoveItem(productId);

        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_NotInCart_ShouldThrow()
    {
        var cart = Cart.Create();

        var act = () => cart.RemoveItem(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found in cart*");
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var cart = Cart.Create();
        cart.AddItem(Guid.NewGuid(), 1);
        cart.AddItem(Guid.NewGuid(), 2);

        cart.Clear();

        cart.Items.Should().BeEmpty();
        cart.IsEmpty.Should().BeTrue();
    }
}
