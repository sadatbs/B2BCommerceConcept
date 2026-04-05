using B2B.Commerce.Domain.Entities;
using B2B.Commerce.Domain.Enums;
using FluentAssertions;

namespace B2B.Commerce.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_CreatesUser()
    {
        var customerId = Guid.NewGuid();

        var user = User.Create(customerId, "john.doe@acme.com", "John", "Doe");

        user.Id.Should().NotBeEmpty();
        user.CustomerId.Should().Be(customerId);
        user.Email.Should().Be("john.doe@acme.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Role.Should().Be(UserRole.Buyer);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_NormalizesEmailToLowerInvariant()
    {
        var user = User.Create(Guid.NewGuid(), "JOHN.DOE@ACME.COM", "John", "Doe");

        user.Email.Should().Be("john.doe@acme.com");
    }

    [Fact]
    public void Create_WithRole_SetsRole()
    {
        var user = User.Create(Guid.NewGuid(), "admin@acme.com", "Admin", "User", UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void Create_WithEmptyEmail_Throws()
    {
        var act = () => User.Create(Guid.NewGuid(), "", "John", "Doe");

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void FullName_ReturnsConcatenatedName()
    {
        var user = User.Create(Guid.NewGuid(), "john.doe@acme.com", "John", "Doe");

        user.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void UpdateDetails_UpdatesFields()
    {
        var user = User.Create(Guid.NewGuid(), "john.doe@acme.com", "John", "Doe");

        user.UpdateDetails("Jonathan", "Smith", UserRole.Admin);

        user.FirstName.Should().Be("Jonathan");
        user.LastName.Should().Be("Smith");
        user.Role.Should().Be(UserRole.Admin);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var user = User.Create(Guid.NewGuid(), "john.doe@acme.com", "John", "Doe");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var user = User.Create(Guid.NewGuid(), "john.doe@acme.com", "John", "Doe");
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }
}
