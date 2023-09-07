using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torello.Domain.Lanes;

namespace Torello.Infrastructure.Persistence.Configurations;

public sealed class LaneConfiguration : IEntityTypeConfiguration<Lane>
{
    public void Configure(EntityTypeBuilder<Lane> builder)
    {
        builder
            .HasKey(b => b.Id);

        builder
            .Property(b => b.Id)
            .ValueGeneratedNever()
            .HasConversion(
                // ToString() is important for upper/lowercase storing
                id => id.Value.ToString(),
                value => LaneId.Create(value)!);
    }
}