using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Enums;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Domain.Entities.Shares;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Domain.Entities.Wips;

namespace TimesynqServer.Infrastructure.Service
{
    public class ShareService : IShareService
    {
        private readonly IShareRepository _shareRepository;
        private readonly IWipRepository _wipRepository;
        private readonly IUserRepository _userRepository;

        public ShareService(
            IShareRepository shareRepository,
            IWipRepository wipRepository,
            IUserRepository userRepository
            ) 
        {
            _shareRepository = shareRepository;
            _wipRepository = wipRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResult<UserDTO>> GetSharedUsersAsync(Guid callerId, Guid wipId, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest)
        {
            WipProjection? wip = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            if (wip == null)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            int totalWips = await _shareRepository.GetSharedUserCountAsync(wipId);

            int totalPages = (int)Math.Ceiling((double)totalWips / pageSize);

            if (totalPages <= 0)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            if (!Enum.TryParse<SortOrder>(sortOrder, true, out var parsedSortOrder))
            {
                parsedSortOrder = SortOrder.Default;
            }

            if (!Enum.TryParse<ShareSortBy>(sortBy, true, out var parsedSortBy))
            {
                parsedSortBy = ShareSortBy.ShareAge;
            }

            IEnumerable<UserProjection> userProjections = await _shareRepository.GetSharedUsersByWipAsync(wipId, pageNumber, pageSize, parsedSortOrder, parsedSortBy);

            IEnumerable<UserDTO> userDTOs = userProjections.Select(UserDTO.FromProjection);

            return new PagedResult<UserDTO>(userDTOs, pageNumber, pageSize, totalWips, totalPages, httpRequest);
        }

        public async Task<PagedResult<WipDTO>> GetSharedWipsAsync(Guid callerId, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest)
        {
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            int totalWips = await _shareRepository.GetSharedWipCountAsync(callerId);

            int totalPages = (int)Math.Ceiling((double)totalWips / pageSize);

            if (totalPages <= 0)
            {
                return PagedResult<WipDTO>.CreateEmpty();
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            if (!Enum.TryParse<SortOrder>(sortOrder, true, out var parsedSortOrder))
            {
                parsedSortOrder = SortOrder.Default;
            }

            if (!Enum.TryParse<ShareSortBy>(sortBy, true, out var parsedSortBy))
            {
                parsedSortBy = ShareSortBy.ShareAge;
            }

            IEnumerable<WipProjection> wipProjections = await _shareRepository.GetSharedWipsByUserAsync(callerId, pageNumber, pageSize, parsedSortOrder, parsedSortBy);

            IEnumerable<WipDTO> wipDTOs = wipProjections.Select(WipDTO.FromProjection);

            return new PagedResult<WipDTO>(wipDTOs, pageNumber, pageSize, totalWips, totalPages, httpRequest);
        }

        public async Task<Result<UserDTO>> ShareWipAsync(Guid callerId, Guid wipId, Guid userId)
        {
            if (callerId == userId)
            {
                return Result<UserDTO>.Failure(DomainErrors.Share.CannotShareWithSelf);
            }

            WipProjection? wip = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            if (wip == null)
            {
                return Result<UserDTO>.Failure(DomainErrors.Wip.NotFound);
            }

            UserProjection? user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDTO>.Failure(DomainErrors.User.NotFound);
            }

            bool existsAlready = await _shareRepository.ExistsAsync(wipId, userId);
            if (existsAlready)
            {
                return Result<UserDTO>.Failure(DomainErrors.Share.AlreadyShared);
            }

            Result<Share> shareCreationResult = Share.Create(wipId, userId);
            return await shareCreationResult.Match
            (
                onSuccess: async share =>
                {
                    await _shareRepository.AddShareAsync(share);
                    return Result<UserDTO>.Success(UserDTO.FromProjection(user));
                },
                onFailure: error => Task.FromResult(Result<UserDTO>.Failure(error))
            );
        }

        public async Task<Result> UnshareFromWipAsync(Guid callerId, Guid wipId, Guid userId)
        {
            WipProjection? wipOwnedByCaller = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            bool wipSharedWithCaller = await _shareRepository.ExistsAsync(wipId, callerId) && callerId == userId;
            if (wipOwnedByCaller == null && !wipSharedWithCaller)
            {
                return Result.Failure(DomainErrors.Wip.NotFound);
            }

            int deleted = await _shareRepository.DeleteShareAsync(wipId, userId);
            if (deleted == 0)
            {
                return Result.Failure(DomainErrors.Share.NoSharesDeleted);
            }

            return Result.Success();
        }

        public async Task<Result> UnshareAllFromWipAsync(Guid callerId, Guid wipId)
        {
            WipProjection? wip = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            if (wip == null)
            {
                return Result.Failure(DomainErrors.Wip.NotFound);
            }

            int deleted = await _shareRepository.DeleteAllSharesByWipAsync(wipId);
            if (deleted == 0)
            {
                return Result.Failure(DomainErrors.Share.NoSharesDeleted);
            }

            return Result.Success();
        }
    }
}
