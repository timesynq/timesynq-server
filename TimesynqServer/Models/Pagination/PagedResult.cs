namespace TimesynqServer.Models.Pagination
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
        public required IEnumerable<T> Items { get; set; } = [];

        /// <summary>
        /// The current page number (1-indexed).
        /// </summary>
        public required int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public required int PageSize { get; set; }

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public required int TotalItems { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public required int TotalPages { get; set; }

        /// <summary>
        /// Link to the first page (if applicable, string.Empty otherwise).
        /// </summary>
        public required string FirstPageUrl { get; set; }

        /// <summary>
        /// Link to the last page (if applicable, string.Empty otherwise).
        /// </summary>
        public required string LastPageUrl { get; set; }

        /// <summary>
        /// Link to the previous page (if applicable, string.Empty otherwise).
        /// </summary>
        public required string PreviousPageUrl { get; set; }

        /// <summary>
        /// Link to the next page (if applicable, string.Empty otherwise).
        /// </summary>
        public required string NextPageUrl { get; set; }

        /// <summary>
        /// Link to the current page.
        /// </summary>
        public required string SelfPageUrl { get; set; }
    }
}
