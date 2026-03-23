using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Shares
{
    public interface IShareRepository
    {
        public Task<Share?> GetTrackedShareAsync(Guid wipId, Guid sharedWithId);
        public Task<SharedWipProjection?> GetSharedWipAsync(Guid wipId, Guid sharedWithId);
        public Task<bool> ExistsAsync(Guid wipId, Guid sharedWithId);
        public Task<int> GetSharedWipCountAsync(Guid sharedWithId, bool isAccepted, string? searchString);
        public Task<IEnumerable<SharedUserProjection>> GetSharedUsersByWipAsync(Guid wipId);
        public Task<IEnumerable<SharedWipProjection>> GetSharedWipsByUserAsync(Guid sharedWithId, bool isAccepted, string? searchString, int pageNumber, int pageSize, SortOrder sortOrder, ShareSortBy sortBy);
        public Task AddShareAsync(Share share);
        public Task<int> DeleteShareAsync(Guid wipId, Guid sharedWithId);
        public Task<int> DeleteAllSharesByWipAsync(Guid wipId);
    }
}
