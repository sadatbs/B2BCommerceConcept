using B2B.Commerce.Domain.Enums;

namespace B2B.Commerce.Domain.Entities;

public class Customer
{
    private readonly List<User> _users = new();

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public PaymentTerms PaymentTerms { get; private set; }
    public Guid? PriceTierId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public PriceTier? PriceTier { get; private set; }
    public IReadOnlyList<User> Users => _users.AsReadOnly();

    private Customer() { } // EF Core

    public static Customer Create(
        string code,
        string name,
        string email,
        PaymentTerms paymentTerms = PaymentTerms.Net30,
        Guid? priceTierId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        return new Customer
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PaymentTerms = paymentTerms,
            PriceTierId = priceTierId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string email, PaymentTerms paymentTerms)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PaymentTerms = paymentTerms;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignPriceTier(Guid? priceTierId)
    {
        PriceTierId = priceTierId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
