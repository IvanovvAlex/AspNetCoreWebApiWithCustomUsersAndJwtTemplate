using Microsoft.Extensions.DependencyInjection;
using Template.Data.Seed;

namespace Template.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        IServiceScopeFactory scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using IServiceScope scope = scopeFactory.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await UserSeeder.SeedAsync(db);
    }
}