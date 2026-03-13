namespace TimesynqServer.Contracts.Projections
{
    public class WipProjection
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

        /// <summary>
        /// Constructs a <see cref="WipProjection"/> instance from Wip information. Used for filtering columns in queries.
        /// </summary>
        /// <param name="id">The unique identifier of the wip.</param>
        /// <param name="name">The name of the wip.</param>
        /// <param name="ownerId">The unique identifier of the user who created the wip.</param>
        /// <param name="createdOnUTC">The date when the wip was created.</param>
        /// <param name="lastOpenedOnUTC">The date when the wip was last opened.</param>
        public WipProjection(Guid id, string name, Guid ownerId, DateTime createdOnUTC, DateTime lastOpenedOnUTC)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            CreatedOnUTC = createdOnUTC;
            LastOpenedOnUTC = lastOpenedOnUTC;
        }
    }
}
