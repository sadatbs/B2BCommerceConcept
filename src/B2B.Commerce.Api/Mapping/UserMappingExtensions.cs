using B2B.Commerce.Contracts.Customers;
using B2B.Commerce.Domain.Entities;

namespace B2B.Commerce.Api.Mapping;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            CustomerId = user.CustomerId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public static IReadOnlyList<UserDto> ToDtos(this IEnumerable<User> users)
    {
        return users.Select(u => u.ToDto()).ToList();
    }
}
