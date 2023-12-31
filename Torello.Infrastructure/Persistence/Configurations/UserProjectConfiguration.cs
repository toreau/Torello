using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torello.Domain.UserProjects;

namespace Torello.Infrastructure.Persistence.Configurations;

public sealed class UserProjectConfiguration : IEntityTypeConfiguration<UserProject>
{
    public void Configure(EntityTypeBuilder<UserProject> builder)
    {
        builder
            .HasKey(up => new { up.UserId, up.ProjectId });
    }
}
