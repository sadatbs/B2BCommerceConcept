using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace B2B.Commerce.Infrastructure.Data;

public class CommerceDbContextFactory : IDesignTimeDbContextFactory<CommerceDbContext>
{
    public CommerceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommerceDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=b2b_commerce;Username=b2b_user;Password=b2b_dev_password");

        return new CommerceDbContext(optionsBuilder.Options);
    }
}
