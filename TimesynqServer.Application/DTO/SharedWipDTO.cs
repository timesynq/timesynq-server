using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    public class SharedWipDTO : WipDTO
    {
        public string OwnerName { get; }
        public DateTime SharedOnUTC { get; }
        public bool IsAccepted { get; }

        private SharedWipDTO(
            Guid id,
            string name,
            Guid ownerId,
            string ownerName,
            DateTime sharedOnUTC,
            DateTime createdOnUTC,
            DateTime lastOpenedOnUTC,
            bool isAccepted
        ) : base(id, name, ownerId, createdOnUTC, lastOpenedOnUTC)
        {
            OwnerName = ownerName;
            SharedOnUTC = sharedOnUTC;
            IsAccepted = isAccepted;
        }

        public static SharedWipDTO FromProjection(SharedWipProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new SharedWipDTO(
                projection.Id,
                projection.Name,
                projection.OwnerId,
                projection.OwnerName,
                projection.SharedOnUTC,
                projection.CreatedOnUTC,
                projection.LastOpenedOnUTC,
                projection.IsAccepted
            );
        }
    }
}
