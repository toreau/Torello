using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Torello.Domain.Boards;
using Torello.Domain.Issues;
using Torello.Domain.Lanes;
using Torello.Domain.Projects;
using Torello.Domain.UserProjects;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Persistence;

public class TorelloDbContext(DbContextOptions<TorelloDbContext> options) : DbContext(options)
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<Project> Projects { get; set; }
    public required DbSet<UserProject> UserProjects { get; set; }
    public required DbSet<Board> Boards { get; set; }
    public required DbSet<Lane> Lanes { get; set; }
    public required DbSet<Issue> Issues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(TorelloDbContext).Assembly);

        // I want lowercased table names
        // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        //     entityType.SetTableName(entityType.DisplayName().ToLowerInvariant());

        // I want singular table names
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            entityType.SetTableName(entityType.DisplayName());

        // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
        // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
        // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
        // use the DateTimeOffsetToBinaryConverter
        // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
        // This only supports millisecond precision, but should be sufficient for most use cases.
        if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            IEnumerable<PropertyInfo> properties = entityType.ClrType
                .GetProperties()
                .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));

            foreach (var property in properties)
            {
                modelBuilder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new DateTimeOffsetToBinaryConverter());
            }
        }
    }
}