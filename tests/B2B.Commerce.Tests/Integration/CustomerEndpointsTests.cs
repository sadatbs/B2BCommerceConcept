using System.Net;
using System.Net.Http.Json;
using B2B.Commerce.Contracts.Common;
using B2B.Commerce.Contracts.Customers;
using B2B.Commerce.Contracts.Pricing;
using FluentAssertions;

namespace B2B.Commerce.Tests.Integration;

public class CustomerEndpointsTests : IntegrationTestBase
{
    // ── Customer CRUD ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCustomer_ReturnsCreated()
    {
        var request = new CreateCustomerRequest
        {
            Code = "ACME-001",
            Name = "Acme Corp",
            Email = "billing@acme.com"
        };

        var response = await Client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer!.Code.Should().Be("ACME-001");
        customer.Name.Should().Be("Acme Corp");
        customer.Email.Should().Be("billing@acme.com");
        customer.PaymentTerms.Should().Be("Net30");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_DuplicateCode_ReturnsConflict()
    {
        await CreateCustomerAsync("DUP-001", "First Corp", "first@corp.com");

        var response = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
        {
            Code = "DUP-001",
            Name = "Second Corp",
            Email = "second@corp.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCustomer_InvalidEmail_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
        {
            Code = "VAL-001",
            Name = "Test Corp",
            Email = "not-an-email"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomerById_ReturnsCustomerWithUsers()
    {
        var customer = await CreateCustomerAsync("GET-001", "Get Corp", "get@corp.com");

        var response = await Client.GetAsync($"/api/customers/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<CustomerDetailDto>();
        detail!.Id.Should().Be(customer.Id);
        detail.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerById_NotFound_Returns404()
    {
        var response = await Client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomers_ReturnsPaged()
    {
        await CreateCustomerAsync("PAGE-A", "Page Alpha", "alpha@page.com");
        await CreateCustomerAsync("PAGE-B", "Page Beta", "beta@page.com");

        var response = await Client.GetAsync("/api/customers?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerDto>>();
        paged!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsUpdated()
    {
        var customer = await CreateCustomerAsync("UPD-001", "Old Name", "old@corp.com");

        var response = await Client.PutAsJsonAsync($"/api/customers/{customer.Id}", new UpdateCustomerRequest
        {
            Name = "New Name",
            Email = "new@corp.com",
            PaymentTerms = "Net60"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        updated!.Name.Should().Be("New Name");
        updated.Email.Should().Be("new@corp.com");
        updated.PaymentTerms.Should().Be("Net60");
    }

    [Fact]
    public async Task DeactivateCustomer_SetsInactive()
    {
        var customer = await CreateCustomerAsync("DEACT-001", "Active Corp", "active@corp.com");

        var response = await Client.PostAsync($"/api/customers/{customer.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateCustomer_Reactivates()
    {
        var customer = await CreateCustomerAsync("ACT-001", "Deact Corp", "deact@corp.com");
        await Client.PostAsync($"/api/customers/{customer.Id}/deactivate", null);

        var response = await Client.PostAsync($"/api/customers/{customer.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        updated!.IsActive.Should().BeTrue();
    }

    // ── User management ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUser_ReturnsCreated()
    {
        var customer = await CreateCustomerAsync("USR-001", "User Corp", "users@corp.com");

        var response = await Client.PostAsJsonAsync($"/api/customers/{customer.Id}/users", new CreateUserRequest
        {
            Email = "john.doe@corp.com",
            FirstName = "John",
            LastName = "Doe"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be("john.doe@corp.com");
        user.FullName.Should().Be("John Doe");
        user.Role.Should().Be("Buyer");
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsConflict()
    {
        var customer = await CreateCustomerAsync("USR-002", "User Corp 2", "users2@corp.com");
        await Client.PostAsJsonAsync($"/api/customers/{customer.Id}/users", new CreateUserRequest
        {
            Email = "duplicate@corp.com",
            FirstName = "First",
            LastName = "User"
        });

        var response = await Client.PostAsJsonAsync($"/api/customers/{customer.Id}/users", new CreateUserRequest
        {
            Email = "duplicate@corp.com",
            FirstName = "Second",
            LastName = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetCustomerUsers_ReturnsPaged()
    {
        var customer = await CreateCustomerAsync("USR-003", "User Corp 3", "users3@corp.com");
        await Client.PostAsJsonAsync($"/api/customers/{customer.Id}/users", new CreateUserRequest
        {
            Email = "user1@corp.com",
            FirstName = "User",
            LastName = "One"
        });
        await Client.PostAsJsonAsync($"/api/customers/{customer.Id}/users", new CreateUserRequest
        {
            Email = "user2@corp.com",
            FirstName = "User",
            LastName = "Two"
        });

        var response = await Client.GetAsync($"/api/customers/{customer.Id}/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();
        paged!.TotalCount.Should().Be(2);
    }

    // ── Price tier assignment ────────────────────────────────────────────────

    [Fact]
    public async Task AssignPriceTier_UpdatesCustomer()
    {
        var customer = await CreateCustomerAsync("TIER-001", "Tier Corp", "tier@corp.com");
        var tier = await CreatePriceTierAsync("Gold");

        var response = await Client.PutAsJsonAsync($"/api/customers/{customer.Id}/price-tier",
            new AssignPriceTierRequest { PriceTierId = tier.Id });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        updated!.PriceTierId.Should().Be(tier.Id);
        updated.PriceTierName.Should().Be("Gold");
    }

    [Fact]
    public async Task AssignPriceTier_WithNull_ClearsTier()
    {
        var tier = await CreatePriceTierAsync("Silver");
        var customer = await CreateCustomerAsync("TIER-002", "Tier Corp 2", "tier2@corp.com");
        await Client.PutAsJsonAsync($"/api/customers/{customer.Id}/price-tier",
            new AssignPriceTierRequest { PriceTierId = tier.Id });

        var response = await Client.PutAsJsonAsync($"/api/customers/{customer.Id}/price-tier",
            new AssignPriceTierRequest { PriceTierId = null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CustomerDto>();
        updated!.PriceTierId.Should().BeNull();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<CustomerDto> CreateCustomerAsync(string code, string name, string email)
    {
        var response = await Client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
        {
            Code = code,
            Name = name,
            Email = email
        });
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    private async Task<PriceTierDto> CreatePriceTierAsync(string name)
    {
        var response = await Client.PostAsJsonAsync("/api/price-tiers", new CreatePriceTierRequest { Name = name });
        return (await response.Content.ReadFromJsonAsync<PriceTierDto>())!;
    }
}
