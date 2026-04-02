using System.Text.RegularExpressions;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Entities.Shares;
using TimesynqServer.Domain.Entities.Users;

namespace TimesynqServer.Domain.Entities.Wips
{
    public sealed class Wip
    {
        /// <summary>
        /// The wip's unique identifier.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The name of the wip.
        /// </summary>
        public string Name { get; private set; } = "Untitled WIP";

        /// <summary>
        /// The unique identifier of the user who created the wip.
        /// </summary>
        public Guid OwnerId { get; private set; }

        /// <summary>
        /// The wip's initial beats per minute.
        /// </summary>
        public int Bpm { get; private set; } = TrackerConstants.DefaultBpm;

        /// <summary>
        /// The number of channels each of the wip's frames has including the master channel.
        /// </summary>
        public int Channels { get; private set; } = TrackerConstants.DefaultChannels;

        /// <summary>
        /// The date when the wip was created.
        /// </summary>
        public DateTime CreatedOnUTC { get; private set; }

        /// <summary>
        /// The date when the wip was last opened.
        /// </summary>
        public DateTime LastOpenedOnUTC { get; private set; }

        /// <summary>
        /// The date when the owner deleted the wip.
        /// </summary>
        public DateTime? DeletedOnUTC { get; private set; }

        /// <summary>
        /// Shortcut for determining if a wip has been deleted.
        /// </summary>
        public bool IsDeleted => DeletedOnUTC.HasValue;

        /// <summary>
        /// Navigation property for the user who created the wip.
        /// </summary>
        public TimesynqUser Owner { get; private set; } = null!;

        /// <summary>
        /// Navigation property for all users this wip has been shared with.
        /// </summary>
        public ICollection<Share> Shares { get; private set; } = [];

        private Wip() { }

        public Wip(Guid ownerId)
        {
            Id = Guid.NewGuid();
            OwnerId = ownerId;
            CreatedOnUTC = DateTime.UtcNow;
            LastOpenedOnUTC = DateTime.UtcNow;
            DeletedOnUTC = null;
        }

        public void Open()
        {
            LastOpenedOnUTC = DateTime.UtcNow;
        }

        public Result ChangeName(string newName)
        {
            if (
                string.IsNullOrWhiteSpace(newName) || 
                (newName.Length < WipConstants.MinNameLength || newName.Length > WipConstants.MaxNameLength)
                )
            {
                return Result.Failure(DomainErrors.Wip.InvalidName);
            }

            if (newName == Name)
            {
                return Result.Failure(DomainErrors.Wip.NameConflict);
            }

            MatchCollection matchCollection = Regex.Matches(newName, @"[^\p{L}\p{N} $#_+\-()'&.,!~@%^*={};:<>/?]");
            string invalidCharacters = string.Concat(matchCollection.Select(ic => ic.Value).Distinct());
            if(invalidCharacters.Length > 0)
            {
                return Result.Failure(DomainErrors.Wip.NameContainsInvalidCharacters(invalidCharacters));
            }

            Name = newName;
            return Result.Success();
        }

        public Result SoftDelete()
        {
            if (IsDeleted)
            {
                return Result.Failure(DomainErrors.Wip.WipDeleted);
            }

            DeletedOnUTC = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
