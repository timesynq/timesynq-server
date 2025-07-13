using System.Collections.Specialized;
using System.Web;
using TimesynqServer.Models.Pagination;

namespace TimesynqServer.Extensions
{
    public static class PaginationExtensions
    {
        /// <summary>
        /// Converts a collection of items into a paginated <see cref="PagedResult{T}"/> with navigation URLs.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="pageNumber">The current page number (1-indexed).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="totalItems">The total number of items across all pages.</param>
        /// <param name="totalPages">The total number of pages.</param>
        /// <param name="request">The current <see cref="HttpRequest"/> used to construct pagination URLs.</param>
        /// <returns>
        /// A <see cref="PagedResult{T}"/> containing the paged items and metadata including pagination URLs.
        /// </returns>
        /// <remarks>
        /// The returned <see cref="PagedResult{T}"/> includes the following navigation URLs:
        /// <list type="bullet">
        /// <item><description><c>FirstPageUrl</c> – Link to the first page (if applicable, string.Empty otherwise).</description></item>
        /// <item><description><c>LastPageUrl</c> – Link to the last page (if applicable, string.Empty otherwise).</description></item>
        /// <item><description><c>PreviousPageUrl</c> – Link to the previous page (if applicable, string.Empty otherwise).</description></item>
        /// <item><description><c>NextPageUrl</c> – Link to the next page (if applicable, string.Empty otherwise).</description></item>
        /// <item><description><c>SelfPageUrl</c> – Link to the current page.</description></item>
        /// </list>
        /// </remarks>
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
            }
            ;

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
