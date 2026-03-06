using TimesynqServer.Contracts.Projections;
using TimesynqServer.Domain.Entities.Wips;

namespace TimesynqServer.Application.DTO
{
    /// <summary>
    /// Represents only the publicly relevant information of a Wip.
    /// </summary>
    public sealed class WipDTO
    {
        /// <summary>
        /// The wip's unique identifier.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The name of the wip.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The unique identifier of the user who created the wip.
        /// </summary>
        public Guid OwnerId { get; }

        /// <summary>
        /// The date when the wip was created.
        /// </summary>
        public DateTime CreatedOnUTC { get; }

        /// <summary>
        /// The date when the wip was last opened.
        /// </summary>
        public DateTime LastOpenedOnUTC { get; }

        private WipDTO(Guid id, string name, Guid ownerId, DateTime createdOnUTC, DateTime lastOpenedOnUTC)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            CreatedOnUTC = createdOnUTC;
            LastOpenedOnUTC = lastOpenedOnUTC;
        }

        public static WipDTO FromProjection(WipProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new WipDTO(
                projection.Id,
                projection.Name,
                projection.OwnerId,
                projection.CreatedOnUTC,
                projection.LastOpenedOnUTC
            );
        }

        public static WipDTO FromWip(Wip wip)
        {
            ArgumentNullException.ThrowIfNull(wip);

            return new WipDTO(
                wip.Id,
                wip.Name,
                wip.OwnerId,
                wip.CreatedOnUTC,
                wip.LastOpenedOnUTC
            );
        }
    }
}
