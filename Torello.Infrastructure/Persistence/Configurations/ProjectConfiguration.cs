using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torello.Domain.Projects;

namespace Torello.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder
            .HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value.ToString(), // ToString() is important for upper/lowercase storing
                value => ProjectId.Create(value));
    }
}