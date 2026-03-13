using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Domain.Entities.Wips;

namespace TimesynqServer.Domain.Entities.Shares
{
    public class Share
    {
        /// <summary>
        /// The unique identifier of the wip that is being shared.
        /// </summary>
        public Guid WipId { get; private set; }
        
        /// <summary>
        /// The unique identifier of the user the wip is being shared with.
        /// </summary>
        public Guid SharedWithId { get; private set; }
        
        /// <summary>
        /// Determines if the recipient of the share has accepted the invite.
        /// IsAccepted == false means that the invite is pending. A declined share invite will be deleted.
        /// </summary>
        public bool IsAccepted { get; private set; }

        /// <summary>
        /// The date when the share action occurred.
        /// </summary>
        public DateTime CreatedOnUTC { get; private set; }

        /// <summary>
        /// Navigation property for the wip that is being shared.
        /// </summary>
        public Wip Wip { get; private set; } = null!;

        /// <summary>
        /// Navigation property for the user that the wip is being shared with.
        /// </summary>
        public TimesynqUser SharedWith { get; private set; } = null!;

        private Share() { }

        private Share(Guid wipId, Guid sharedWithId)
        {
            WipId = wipId;
            SharedWithId = sharedWithId;
            CreatedOnUTC = DateTime.UtcNow;
            IsAccepted = false;
        }

        public static Result<Share> Create(Guid wipId, Guid sharedWithId)
        {
            if (wipId == sharedWithId)
            {
                return Result<Share>.Failure(DomainErrors.Share.MatchingId);
            }

            return Result<Share>.Success(new Share(wipId, sharedWithId));
        }

        public Result Accept()
        {
            if (IsAccepted)
                return Result.Failure(DomainErrors.Share.AlreadyAccepted);
            IsAccepted = true;
            return Result.Success();
        }
    }
}
