using Microsoft.EntityFrameworkCore;
using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Domain.Entities.Shares;

namespace TimesynqServer.Persistence.Repository
{
    public class ShareRepository : IShareRepository
    {
        private readonly TimesynqDbContext _dbContext;

        public ShareRepository(TimesynqDbContext dbContext) 
        {
            _dbContext = dbContext;
        }    

        public async Task<Share?> GetTrackedShareAsync(Guid wipId, Guid sharedWithId)
        {
            return await _dbContext.Shares
                .Where(s => s.WipId == wipId && s.SharedWithId == sharedWithId && !s.IsAccepted)
                .FirstOrDefaultAsync();
        }

        public async Task<SharedWipProjection?> GetSharedWipAsync(Guid wipId, Guid sharedWithId)
        {
            return await _dbContext.Shares
                .AsNoTracking()
                .Where(s => s.WipId == wipId && s.SharedWithId == sharedWithId && s.IsAccepted)
                .Select(s => new SharedWipProjection
                (
                    s.Wip.Id,
                    s.Wip.Name,
                    s.Wip.OwnerId,
                    s.Wip.CreatedOnUTC,
                    s.Wip.LastOpenedOnUTC,
                    s.IsAccepted
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(Guid wipId, Guid sharedWithId)
        {
            return await _dbContext.Shares
                .AsNoTracking()
                .Where(s => s.WipId == wipId && s.SharedWithId == sharedWithId)
                .AnyAsync();
        }

        public async Task<int> GetUnacceptedShareCountAsync(Guid callerId)
        {
            return await _dbContext.Shares
                .AsNoTracking()
                .Where(s => s.Wip.OwnerId == callerId && !s.IsAccepted)
                .CountAsync();
        }

        public async Task<int> GetSharedWipCountAsync(Guid sharedWithId, string? searchString)
        {
            var query = _dbContext.Shares
                .AsNoTracking()
                .Where(s => s.SharedWithId == sharedWithId);

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Wip.Name.StartsWith(searchString));

            return await query.CountAsync();
        }

        public async Task<IEnumerable<SharedUserProjection>> GetSharedUsersByWipAsync(Guid wipId)
        {
            return await _dbContext.Shares
                .AsNoTracking()
                .Where(s => s.WipId == wipId)
                .OrderBy(s => s.SharedWith.UserName)
                .Select(s => new SharedUserProjection
                (
                    s.SharedWith.Id,
                    s.SharedWith.UserName!,
                    s.SharedWith.ProfilePicture!,
                    s.SharedWith.CreatedOnUTC,
                    s.SharedWith.Followers.Count,
                    s.SharedWith.Followees.Count,
                    s.IsAccepted
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<SharedWipProjection>> GetSharedWipsByUserAsync(Guid sharedWithId, string? searchString, int pageNumber, int pageSize, SortOrder sortOrder, ShareSortBy sortBy)
        {
            var query = _dbContext.Shares
                .AsNoTracking()
                .Where(s => s.SharedWithId == sharedWithId);

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Wip.Name.StartsWith(searchString));

            query = (sortOrder, sortBy) switch
            {
                (SortOrder.Default, ShareSortBy.Name) => query.OrderBy(s => s.Wip.Name),
                (SortOrder.Reverse, ShareSortBy.Name) => query.OrderByDescending(s => s.Wip.Name),

                (SortOrder.Default, ShareSortBy.ShareAge) => query.OrderByDescending(s => s.CreatedOnUTC),
                (SortOrder.Reverse, ShareSortBy.ShareAge) => query.OrderBy(s => s.CreatedOnUTC),

                _ => query.OrderBy(s => s.Wip.Name)
            };

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SharedWipProjection(
                    s.Wip.Id,
                    s.Wip.Name,
                    s.Wip.OwnerId,
                    s.Wip.CreatedOnUTC,
                    s.Wip.LastOpenedOnUTC,
                    s.IsAccepted
                ))
                .ToListAsync();
        }

        public async Task AddShareAsync(Share share)
        {
            await _dbContext.Shares.AddAsync(share);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteShareAsync(Guid wipId, Guid sharedWithId)
        {
            return await _dbContext.Shares
                .Where(s => s.WipId == wipId && s.SharedWithId == sharedWithId)
                .ExecuteDeleteAsync();
        }

        public async Task<int> DeleteAllSharesByWipAsync(Guid wipId)
        {
            return await _dbContext.Shares
                .Where(s => s.WipId == wipId)
                .ExecuteDeleteAsync();
        }

    }
}
