namespace PublicidadMarketplaceAnunciantes.Common;

/// <summary>Envoltura de paginación consistente: ?page=1&amp;pageSize=20.</summary>
public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static PagedResult<T> Create(IReadOnlyCollection<T> items, int page, int pageSize, int totalItems)
        => new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
}
