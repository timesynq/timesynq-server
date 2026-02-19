using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Enums;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Domain.Entities.Wips;
using TimesynqServer.Persistence.UnitOfWork;

namespace TimesynqServer.Infrastructure.Service
{
    public class WipService : IWipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IWipRepository _wipRepository;

        public WipService(IUnitOfWork unitOfWork, IUserRepository userRepository, IWipRepository wipRepository) 
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _wipRepository = wipRepository;
        }

        public async Task<WipDTO?> GetWipAsync(Guid callerId, Guid wipId)
        {
            WipProjection? wipProjection = await _wipRepository.GetWipByIdAsync(wipId);

            if (wipProjection == null || wipProjection.OwnerId != callerId)
            {
                return null;
            }

            return WipDTO.FromProjection(wipProjection);
        }

        public async Task<PagedResult<WipDTO>> GetMyWipsAsync(Guid callerId, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest)
        {
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            int totalWips = await _wipRepository.GetWipCountAsync(callerId);

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

            if (!Enum.TryParse<WipSortBy>(sortBy, true, out var parsedSortBy))
            {
                parsedSortBy = WipSortBy.LastOpened;
            }

            IEnumerable<WipProjection> wipProjections = await _wipRepository.GetWipsByUserAsync(callerId, pageNumber, pageSize, parsedSortOrder, parsedSortBy);

            IEnumerable<WipDTO> wipDTOs = wipProjections.Select(WipDTO.FromProjection);

            return new PagedResult<WipDTO>(wipDTOs, pageNumber, pageSize, totalWips, totalPages, httpRequest);
        }

        public async Task<Result<WipDTO>> CreateWipAsync(Guid callerId)
        {
            if (!await _userRepository.UserExistsAsync(callerId))
            {
                return Result<WipDTO>.Failure(DomainErrors.User.NotFound);
            }

            var wip = new Wip(callerId);
            await _wipRepository.AddWipAsync(wip);
            return Result<WipDTO>.Success(WipDTO.FromWip(wip));
        }

        public async Task<Result<WipDTO>> ChangeWipName(Guid callerId, Guid wipId, string newName)
        {
            Wip? wip = await _wipRepository.GetTrackedWipByIdAsync(wipId);
            if (wip == null || wip.OwnerId != callerId)
            {
                return Result<WipDTO>.Failure(DomainErrors.Wip.NotFound);
            }

            Result changeNameResult = wip.ChangeName(newName);
            return await changeNameResult.Match
            (
                onSuccess: async () =>
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Result<WipDTO>.Success(WipDTO.FromWip(wip));
                },
                onFailure: error => Task.FromResult(Result<WipDTO>.Failure(error))
            );
        }

        public async Task<Result> DeleteWipAsync(Guid callerId, Guid wipId)
        {
            Wip? wip = await _wipRepository.GetTrackedWipByIdAsync(wipId);
            if (wip == null || wip.OwnerId != callerId)
            {
                return Result.Failure(DomainErrors.Wip.NotFound);
            }

            Result softDeleteResult = wip.SoftDelete();
            return await softDeleteResult.Match
            (
                onSuccess: async () =>
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Result.Success();
                },
                onFailure: error => Task.FromResult(Result.Failure(error))
            );
        }
    }
}
