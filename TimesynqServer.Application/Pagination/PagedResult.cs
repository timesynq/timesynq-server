using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using System.Web;

namespace TimesynqServer.Application.Pagination
{
    /// <summary>
    /// Represents a paginated result containing a collection of items and associated metadata including pagination URLs.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// The collection of items for the current page.
        /// </summary>
        public IEnumerable<T> Items { get; }

        /// <summary>
        /// The current page number (1-indexed).
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Link to the first page (if applicable, string.Empty otherwise).
        /// </summary>
        public string FirstPageUrl { get; }

        /// <summary>
        /// Link to the last page (if applicable, string.Empty otherwise).
        /// </summary>
        public string LastPageUrl { get; }

        /// <summary>
        /// Link to the previous page (if applicable, string.Empty otherwise).
        /// </summary>
        public string PreviousPageUrl { get; }

        /// <summary>
        /// Link to the next page (if applicable, string.Empty otherwise).
        /// </summary>
        public string NextPageUrl { get; }

        /// <summary>
        /// Link to the current page.
        /// </summary>
        public string SelfPageUrl { get; }

        /// <summary>
        /// Constructs a PagedResult object that provides navigation URLs.
        /// </summary>
        /// <param name="items">The page of items.</param>
        /// <param name="pageNumber">The requested page number.</param>
        /// <param name="pageSize">The requested number of items to return per page.</param>
        /// <param name="totalItems">The total number of items in the database.</param>
        /// <param name="totalPages">The total number of pages based on the pageSize.</param>
        /// <param name="request">The HttpRequest associated with the call.</param>
        public PagedResult(
            IEnumerable<T> items,
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
                return $"{baseUrl}?{query}";
            }
            ;

            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = totalPages;
            FirstPageUrl = totalPages > 0 ? BuildUrl(1) : string.Empty;
            LastPageUrl = totalPages > 0 ? BuildUrl(totalPages) : string.Empty;
            PreviousPageUrl = pageNumber > 1 ? BuildUrl(pageNumber - 1) : string.Empty;
            NextPageUrl = pageNumber < totalPages ? BuildUrl(pageNumber + 1) : string.Empty;
            SelfPageUrl = BuildUrl(pageNumber);

        }

        private PagedResult()
        {
            Items = [];
            PageNumber = 0;
            PageSize = 0;
            TotalItems = 0;
            TotalPages = 0;
            FirstPageUrl = string.Empty;
            LastPageUrl = string.Empty;
            PreviousPageUrl = string.Empty;
            NextPageUrl = string.Empty;
            SelfPageUrl = string.Empty;
        }

        /// <summary>
        /// Constructs an empty PagedResult object for when there are 0 total items.
        /// </summary>
        /// <returns>A PagedResult with no items or navigation URLs.</returns>
        public static PagedResult<T> CreateEmpty()
        {
            return new PagedResult<T>();
        }

    }
}
