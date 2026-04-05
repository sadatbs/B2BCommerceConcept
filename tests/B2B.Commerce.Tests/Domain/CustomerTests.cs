using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidData_CreatesCustomer()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com");

        customer.Id.Should().NotBeEmpty();
        customer.Code.Should().Be("ACME-001");
        customer.Name.Should().Be("Acme Corp");
        customer.Email.Should().Be("billing@acme.com");
        customer.PaymentTerms.Should().Be(PaymentTerms.Net30);
        customer.IsActive.Should().BeTrue();
        customer.PriceTierId.Should().BeNull();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_NormalizesCodeToUpperInvariant()
    {
        var customer = Customer.Create("acme-001", "Acme Corp", "billing@acme.com");

        customer.Code.Should().Be("ACME-001");
    }

    [Fact]
    public void Create_NormalizesEmailToLowerInvariant()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "BILLING@ACME.COM");

        customer.Email.Should().Be("billing@acme.com");
    }

    [Fact]
    public void Create_WithPaymentTermsAndTier_SetsBothFields()
    {
        var tierId = Guid.NewGuid();

        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com", PaymentTerms.Net60, tierId);

        customer.PaymentTerms.Should().Be(PaymentTerms.Net60);
        customer.PriceTierId.Should().Be(tierId);
    }

    [Fact]
    public void Create_WithEmptyCode_Throws()
    {
        var act = () => Customer.Create("", "Acme Corp", "billing@acme.com");

        act.Should().Throw<ArgumentException>().WithParameterName("code");
    }

    [Fact]
    public void UpdateDetails_UpdatesFields()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com");

        customer.UpdateDetails("Acme Ltd", "NEW@ACME.COM", PaymentTerms.Net45);

        customer.Name.Should().Be("Acme Ltd");
        customer.Email.Should().Be("new@acme.com");
        customer.PaymentTerms.Should().Be(PaymentTerms.Net45);
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AssignPriceTier_SetsTierId()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com");
        var tierId = Guid.NewGuid();

        customer.AssignPriceTier(tierId);

        customer.PriceTierId.Should().Be(tierId);
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AssignPriceTier_WithNull_ClearsTierId()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com", priceTierId: Guid.NewGuid());

        customer.AssignPriceTier(null);

        customer.PriceTierId.Should().BeNull();
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com");

        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var customer = Customer.Create("ACME-001", "Acme Corp", "billing@acme.com");
        customer.Deactivate();

        customer.Activate();

        customer.IsActive.Should().BeTrue();
    }
}
