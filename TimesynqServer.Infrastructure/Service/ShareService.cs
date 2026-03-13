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
using TimesynqServer.Persistence.UnitOfWork;

namespace TimesynqServer.Infrastructure.Service
{
    public class ShareService : IShareService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShareRepository _shareRepository;
        private readonly IWipRepository _wipRepository;
        private readonly IUserRepository _userRepository;

        public ShareService(
            IUnitOfWork unitOfWork,
            IShareRepository shareRepository,
            IWipRepository wipRepository,
            IUserRepository userRepository
            ) 
        {
            _unitOfWork = unitOfWork;
            _shareRepository = shareRepository;
            _wipRepository = wipRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<SharedUserDTO>> GetSharedUsersAsync(Guid callerId, Guid wipId)
        {
            WipProjection? wip = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            if (wip == null)
            {
                return [];
            }

            IEnumerable<SharedUserProjection> sharedUserProjections = await _shareRepository.GetSharedUsersByWipAsync(wipId);

            return sharedUserProjections.Select(SharedUserDTO.FromProjection);
        }

        public async Task<PagedResult<SharedWipDTO>> GetSharedWipsAsync(Guid callerId, string? searchString, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest)
        {
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            int totalWips = await _shareRepository.GetSharedWipCountAsync(callerId, searchString);

            int totalPages = (int)Math.Ceiling((double)totalWips / pageSize);

            if (totalPages <= 0)
            {
                return PagedResult<SharedWipDTO>.CreateEmpty();
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

            IEnumerable<SharedWipProjection> sharedWipProjections = await _shareRepository.GetSharedWipsByUserAsync(callerId, searchString, pageNumber, pageSize, parsedSortOrder, parsedSortBy);

            IEnumerable<SharedWipDTO> sharedWipDTOs = sharedWipProjections.Select(SharedWipDTO.FromProjection);

            return new PagedResult<SharedWipDTO>(sharedWipDTOs, pageNumber, pageSize, totalWips, totalPages, httpRequest);
        }

        public async Task<Result<UserDTO>> ShareWipAsync(Guid callerId, Guid wipId, Guid shareWithId)
        {
            if (callerId == shareWithId)
            {
                return Result<UserDTO>.Failure(DomainErrors.Share.CannotShareWithSelf);
            }

            WipProjection? wip = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            if (wip == null)
            {
                return Result<UserDTO>.Failure(DomainErrors.Wip.NotFound);
            }

            UserProjection? user = await _userRepository.GetByIdAsync(shareWithId);
            if (user == null)
            {
                return Result<UserDTO>.Failure(DomainErrors.User.NotFound);
            }

            bool existsAlready = await _shareRepository.ExistsAsync(wipId, shareWithId);
            if (existsAlready)
            {
                return Result<UserDTO>.Failure(DomainErrors.Share.AlreadyShared);
            }

            int numUnacceptedShares = await _shareRepository.GetUnacceptedShareCountAsync(callerId);
            if (numUnacceptedShares >= WipConstants.MaxUnacceptedShares)
            {
                return Result<UserDTO>.Failure(DomainErrors.Share.ShareLimitReached);
            }

            // WipConstants.MaxUsersAWipCanBeSharedWith should be set to a low enough value for this to be inconsequential
            IEnumerable<SharedUserProjection> sharesForThisWip = await _shareRepository.GetSharedUsersByWipAsync(wipId);
            if (sharesForThisWip.Count() >= WipConstants.MaxUsersAWipCanBeSharedWith)
            {
                return Result<UserDTO>.Failure(DomainErrors.Share.ShareLimitReached);
            }

            Result<Share> shareCreationResult = Share.Create(wipId, shareWithId);
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

        public async Task<Result<SharedWipDTO>> AcceptShareAsync(Guid callerId, Guid wipId)
        {
            Share? share = await _shareRepository.GetTrackedShareAsync(wipId, callerId);
            if (share == null)
            {
                return Result<SharedWipDTO>.Failure(DomainErrors.Share.NotFound);
            }

            Result shareResult = share.Accept();
            return await shareResult.Match
            (
                onSuccess: async () =>
                {
                    await _unitOfWork.SaveChangesAsync();
                    SharedWipProjection? sharedWipProjection = await _shareRepository.GetSharedWipAsync(wipId, callerId);
                    if (sharedWipProjection == null)
                        return Result<SharedWipDTO>.Failure(DomainErrors.Share.NotFound);
                    return Result<SharedWipDTO>.Success(SharedWipDTO.FromProjection(sharedWipProjection));
                },
                onFailure: error => Task.FromResult(Result<SharedWipDTO>.Failure(error))
            );
        }

        public async Task<Result> UnshareFromWipAsync(Guid callerId, Guid wipId, Guid sharedWithId)
        {
            WipProjection? wipOwnedByCaller = await _wipRepository.GetOwnedWipByIdAsync(callerId, wipId);
            bool wipSharedWithCaller = await _shareRepository.ExistsAsync(wipId, callerId) && callerId == sharedWithId;
            if (wipOwnedByCaller == null && !wipSharedWithCaller)
            {
                return Result.Failure(DomainErrors.Wip.NotFound);
            }

            int deleted = await _shareRepository.DeleteShareAsync(wipId, sharedWithId);
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
