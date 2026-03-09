namespace ConstructionManagement.DTOs.Common
{
    public class PagedResultDto<T>
    {
        public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public int TotalCount { get; init; }
    }
}
