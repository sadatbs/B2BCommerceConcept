using B2B.Commerce.Domain.Interfaces;

namespace B2B.Commerce.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly CommerceDbContext _context;

    public UnitOfWork(CommerceDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
