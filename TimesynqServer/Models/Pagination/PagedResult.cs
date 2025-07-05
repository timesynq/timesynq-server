namespace TimesynqServer.Models.Pagination
{
    public class PagedResult<T>
    {
        public required IEnumerable<T> Items { get; set; } = [];
        public required int PageNumber { get; set; }
        public required int PageSize { get; set; }
        public required int TotalItems { get; set; }
        public required int TotalPages { get; set; }
        public required string FirstPageUrl { get; set; }
        public required string LastPageUrl { get; set; }
        public required string PreviousPageUrl { get; set; }
        public required string NextPageUrl { get; set; }
        public required string SelfPageUrl { get; set; }
    }
}
