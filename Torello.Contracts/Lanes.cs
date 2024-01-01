using System.Data.SqlTypes;
using Torello.Domain.Lanes;

namespace Torello.Contracts;

// Single
public sealed record LaneResult(Lane Lane)
{
    public LaneResponse ToResponse() => new(Lane.Id.Value, Lane.Title);
}

public sealed record LaneResponse(Guid Id, string Title);

// Multiple
public sealed record LanesResult(IEnumerable<Lane> Lanes)
{
    public LanesResponse ToResponse() => new(Lanes.Select(lane => new LaneResult(lane).ToResponse()));
}

public sealed record LanesResponse(IEnumerable<LaneResponse> Lanes);
