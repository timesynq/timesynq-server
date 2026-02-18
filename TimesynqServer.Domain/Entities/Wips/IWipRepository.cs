using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Wips
{
    public interface IWipRepository
    {
        public Task<WipProjection?> GetWipByIdAsync(Guid wipId);
        public Task<int> GetWipCountAsync(Guid ownerId);
        public Task<IEnumerable<WipProjection>> GetWipsByUserAsync(Guid ownerId, int pageNumber, int pageSize, SortOrder sortOrder, WipSortBy sortBy);
        public Task<Wip?> GetTrackedWipByIdAsync(Guid wipId);
        public Task AddWipAsync(Wip wip);
    }
}
