using System.Collections.Specialized;
using System.Web;
using TimesynqServer.Models.Pagination;

namespace TimesynqServer.Extensions
{
    public static class PaginationExtensions
    {
        public static PagedResult<T> ToPagedResult<T>(
            this IEnumerable<T> items,
            int pageNumber,
            int pageSize,
            int totalItems,
            int totalPages,
            HttpRequest request)
        {

            string baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";

            string BuildUrl(int targetPage)
            {
                NameValueCollection query = HttpUtility.ParseQueryString(request.QueryString.ToString());
                query.Set(nameof(pageNumber), targetPage.ToString());
                query.Set(nameof(pageSize), pageSize.ToString());
                return $"{baseUrl}?{query}";
            };

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                FirstPageUrl = totalPages > 0 ? BuildUrl(1) : string.Empty,
                LastPageUrl = totalPages > 0 ? BuildUrl(totalPages) : string.Empty,
                PreviousPageUrl = pageNumber > 1 ? BuildUrl(pageNumber - 1) : string.Empty,
                NextPageUrl = pageNumber < totalPages ? BuildUrl(pageNumber + 1) : string.Empty,
                SelfPageUrl = BuildUrl(pageNumber),
            };

        }
    }
}
