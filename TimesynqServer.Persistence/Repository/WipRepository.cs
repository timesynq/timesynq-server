using Microsoft.EntityFrameworkCore;
using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Domain.Entities.Wips;

namespace TimesynqServer.Persistence.Repository
{
    public class WipRepository : IWipRepository
    {
        private readonly TimesynqDbContext _dbContext;

        public WipRepository(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WipProjection?> GetWipByIdAsync(Guid wipId)
        {
            return await _dbContext.Wips
                .AsNoTracking()
                .Where(w => w.Id == wipId)
                .Select(w => new WipProjection
                (
                    w.Id,
                    w.Name,
                    w.OwnerId,
                    w.CreatedOnUTC,
                    w.LastOpenedOnUTC
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetWipCountAsync(Guid ownerId)
        {
            return await _dbContext.Wips
                .AsNoTracking()
                .Where(w => w.OwnerId == ownerId)
                .CountAsync();
        }

        public async Task<IEnumerable<WipProjection>> GetWipsByUserAsync(Guid ownerId, int pageNumber, int pageSize, SortOrder sortOrder, WipSortBy sortBy)
        {
            var query = _dbContext.Wips
                .AsNoTracking()
                .Where(w => w.OwnerId == ownerId);

            query = (sortOrder, sortBy) switch
            {
                (SortOrder.Default, WipSortBy.Name) => query.OrderBy(w => w.Name),
                (SortOrder.Reverse, WipSortBy.Name) => query.OrderByDescending(w => w.Name),

                // default is newest/most recently opened first
                (SortOrder.Default, WipSortBy.LastOpened) => query.OrderByDescending(w => w.LastOpenedOnUTC),
                (SortOrder.Reverse, WipSortBy.LastOpened) => query.OrderBy(w => w.LastOpenedOnUTC),

                // default sorts oldest/least recently created first
                (SortOrder.Default, WipSortBy.WipAge) => query.OrderBy(w => w.CreatedOnUTC),
                (SortOrder.Reverse, WipSortBy.WipAge) => query.OrderByDescending(w => w.CreatedOnUTC),

                _ => query.OrderBy(w => w.LastOpenedOnUTC)
            };

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WipProjection
                (
                    w.Id,
                    w.Name,
                    w.OwnerId,
                    w.CreatedOnUTC,
                    w.LastOpenedOnUTC
                ))
                .ToListAsync();
        }

        public async Task<Wip?> GetTrackedWipByIdAsync(Guid wipId)
        {
            return await _dbContext.Wips
                .Where(w => w.Id == wipId)
                .FirstOrDefaultAsync();
        }

        public async Task AddWipAsync(Wip wip)
        {
            await _dbContext.Wips.AddAsync(wip);
            await _dbContext.SaveChangesAsync();
        }
    }
}
