using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torello.Domain.Users;

namespace Torello.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(
                // ToString() is important for upper/lowercase storing
                id => id.Value.ToString(),
                value => UserId.Create(value)!);
    }
}