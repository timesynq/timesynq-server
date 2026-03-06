using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Shares
{
    public interface IShareRepository
    {
        public Task<bool> ExistsAsync(Guid wipId, Guid sharedWithId);
        public Task<int> GetSharedUserCountAsync(Guid wipId);
        public Task<int> GetSharedWipCountAsync(Guid sharedWithId);
        public Task<IEnumerable<UserProjection>> GetSharedUsersByWipAsync(Guid wipId, int pageNumber, int pageSize, SortOrder sortOrder, ShareSortBy sortBy);
        public Task<IEnumerable<WipProjection>> GetSharedWipsByUserAsync(Guid sharedWithId, int pageNumber, int pageSize, SortOrder sortOrder, ShareSortBy sortBy);
        public Task AddShareAsync(Share share);
        public Task<int> DeleteShareAsync(Guid wipId, Guid sharedWithId);
        public Task<int> DeleteAllSharesByWipAsync(Guid wipId);
    }
}
