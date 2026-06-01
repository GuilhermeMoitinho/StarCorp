namespace StarCorp.Business.Pagination;

public record PageQuery(int Page = 1, int PageSize = 10)
{
    public int NormalizedPage => Page < 1 ? 1 : Page;
    public int NormalizedPageSize => PageSize is < 1 or > 100 ? 10 : PageSize;
    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
