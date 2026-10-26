using ModelLayer;
using RepositoryLayer.Entities;

namespace BusinessLayer.Labels
{
    internal static class LabelMapper
    {
        public static LabelResponseModel ToResponse(LabelEntity l) => new()
        {
            LabelId = l.LabelId,
            Name = l.Name,
            CreatedAt = l.CreatedAt
        };
    }
}