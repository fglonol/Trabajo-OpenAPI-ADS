namespace MonetizacionEcosistemaCreador.Common;

/// <summary>Parámetros de paginación: ?page=1&amp;pageSize=20 (pageSize máx. 100).</summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : (value > MaxPageSize ? MaxPageSize : value);
    }

    public int Skip => (Page - 1) * PageSize;
}
