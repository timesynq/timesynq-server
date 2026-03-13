using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    public class SharedWipDTO : WipDTO
    {
        public bool IsAccepted { get; }

        private SharedWipDTO(
            Guid id,
            string name,
            Guid ownerId,
            DateTime createdOnUTC,
            DateTime lastOpenedOnUTC,
            bool isAccepted
        ) : base(id, name, ownerId, createdOnUTC, lastOpenedOnUTC)
        {
            IsAccepted = isAccepted;
        }

        public static SharedWipDTO FromProjection(SharedWipProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new SharedWipDTO(
                projection.Id,
                projection.Name,
                projection.OwnerId,
                projection.CreatedOnUTC,
                projection.LastOpenedOnUTC,
                projection.IsAccepted
            );
        }
    }
}
