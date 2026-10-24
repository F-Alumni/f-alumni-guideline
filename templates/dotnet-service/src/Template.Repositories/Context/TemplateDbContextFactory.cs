using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Template.Repositories.Context;

/// <summary>
/// Design-time factory enabling EF Core CLI commands (migrations, script generation).
/// </summary>
public class TemplateDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
    public TemplateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TemplateDbContext>();
        
        // Default local SQL Server Express connection string for migrations design-time
        var connectionString = "Server=localhost\\SQLEXPRESS;Database=FAlumni_Template_Db;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString);

        return new TemplateDbContext(optionsBuilder.Options);
    }
}
