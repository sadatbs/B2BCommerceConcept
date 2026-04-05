using B2B.Commerce.Contracts.Customers;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class CustomerMappingExtensions
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Email = customer.Email,
            PaymentTerms = customer.PaymentTerms.ToString(),
            PriceTierId = customer.PriceTierId,
            PriceTierName = customer.PriceTier?.Name,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };
    }

    public static CustomerDetailDto ToDetailDto(this Customer customer)
    {
        return new CustomerDetailDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Email = customer.Email,
            PaymentTerms = customer.PaymentTerms.ToString(),
            PriceTierId = customer.PriceTierId,
            PriceTierName = customer.PriceTier?.Name,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            Users = customer.Users.Select(u => u.ToDto()).ToList()
        };
    }

    public static IReadOnlyList<CustomerDto> ToDtos(this IEnumerable<Customer> customers)
    {
        return customers.Select(c => c.ToDto()).ToList();
    }
}
