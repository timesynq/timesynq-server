using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Wips
{
    public interface IWipRepository
    {
        public Task<WipProjection?> GetOwnedWipByIdAsync(Guid ownerId, Guid wipId);
        public Task<int> GetWipCountAsync(Guid ownerId, string? searchString);
        public Task<IEnumerable<WipProjection>> GetWipsByUserAsync(Guid ownerId, string? searchString, int pageNumber, int pageSize, SortOrder sortOrder, WipSortBy sortBy);
        public Task<Wip?> GetTrackedWipByIdAsync(Guid wipId);
        public Task AddWipAsync(Wip wip);
    }
}
