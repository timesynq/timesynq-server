namespace TimesynqServer.Contracts.Projections
{
    public sealed class SharedWipProjection : WipProjection
    {
        /// <summary>
        /// The username of the user who created the wip.
        /// </summary>
        public string OwnerName { get; }

        /// <summary>
        /// The date when the wip was shared.
        /// </summary>
        public DateTime SharedOnUTC { get; }

        /// <summary>
        /// Determines if the recipient of the share has accepted the invite.
        /// IsAccepted == false means that the invite is pending. A declined share invite will be deleted.
        /// </summary>
        public bool IsAccepted { get; }

        /// <summary>
        /// Constructs a <see cref="SharedWipProjection"/> instance from shared wip information. Used for filtering columns in queries.
        /// </summary>
        /// <param name="id">The unique identifier of the wip.</param>
        /// <param name="name">The name of the wip.</param>
        /// <param name="ownerId">The unique identifier of the user who created the wip.</param>
        /// <param name="ownerName">The username of the user who created the wip.</param>
        /// <param name="sharedOnUTC">The date when the wip was shared.</param>
        /// <param name="createdOnUTC">The date when the wip was created.</param>
        /// <param name="lastOpenedOnUTC">The date when the wip was last opened.</param>
        /// <param name="isAccepted">Determines if the recipient of the share has accepted the invite.</param>
        public SharedWipProjection(
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
    }
}
