using Microsoft.EntityFrameworkCore;

namespace TkvgSubstitutionBot.Data;

public static class DbMigrator
{
    public static async Task MigrateAsync(AppDbContext db, string migrationsPath)
    {
        if (!Directory.Exists(migrationsPath))
            return;

        var sqlFiles = Directory.GetFiles(migrationsPath, "*.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        foreach (var sqlFile in sqlFiles)
        {
            var sql = await File.ReadAllTextAsync(sqlFile);
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
