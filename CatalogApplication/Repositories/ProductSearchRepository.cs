using System.Data;
using System.Text;
using CatalogApplication.Database;
using CatalogApplication.Types.Search.Dtos;
using CatalogApplication.Types.Search.Local;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class ProductSearchRepository( IDapperContext dapper, ILogger<ProductSearchRepository> logger )
{
    readonly IDapperContext _dapper = dapper;
    readonly ILogger<ProductSearchRepository> _logger = logger;

    // language=sql
    const string CategorySql =
        """
        WITH ProductCategoryFilter AS (
            SELECT DISTINCT ProductId
            FROM CatalogApi.ProductCategories
            WHERE CategoryId IN (SELECT Id FROM @categoryIds)
        ),
        """;
    // language=sql
    const string SearchTextSql =
        """
        WHERE p.Name LIKE '%' + @searchText + '%'
        OR pt.Name LIKE '%' + @searchText + '%';
        """;
    // language=sql
    const string CategoryJoinSql = " INNER JOIN CatalogApi.ProductCategories pc ON p.Id = pc.ProductId";
    // language=sql
    const string SearchTextJoinSql = " INNER JOIN CatalogApi.ProductXmls pt ON p.Id = pt.ProductId";
    // language=sql
    const string ProductSql = "SELECT DISTINCT p.Id, p.BrandId, p.Name, p.Image, p.IsInStock, p.IsFeatured, p.Price, p.SalePrice FROM CatalogApi.Products p";
    // language=sql
    const string CountSql = "SELECT COUNT(*) FROM CatalogApi.Products p";
    // language=sql
    const string WhereStatementSql = " WHERE 1=1";
    // language=sql
    const string BrandsSql = " AND p.BrandId IN (SELECT Id FROM @brandIds)";
    // language=sql
    const string MinPriceSql = " AND (p.Price >= @minPrice OR p.SalePrice >= @minPrice)";
    // language=sql
    const string MaxPriceSql = " AND (p.Price <= @maxPrice OR p.SalePrice <= @maxPrice)";
    // language=sql
    const string StockSql = " AND p.IsInStock = @isInStock";
    // language=sql
    const string FeaturedSql = " AND p.IsFeatured = @isFeatured";
    // language=sql
    const string SaleSql = " AND p.SalePrice >= 0";

    internal async Task<SearchQueryReply?> GetSearch( SearchQueryRequest queryRequest )
    {
        try {
            await using SqlConnection connection = await _dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                _logger.LogError( $"Invalid connection state: {connection.State}" );
                return null;
            }

            BuildSqlQuery( queryRequest, out string sql, out DynamicParameters parameters );
            await using SqlMapper.GridReader multi = await connection.QueryMultipleAsync( sql, parameters, commandType: CommandType.Text );

            SearchQueryReply queryReply = new(
                await multi.ReadSingleAsync<int>(),
                (await multi.ReadAsync<SearchItemDto>()).ToList() );

            return queryReply;
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"An exception occured while executing product search: {e.Message}" );
            return null;
        }
    }

    internal static void BuildSqlQuery( SearchQueryRequest request, out string sql, out DynamicParameters parameters )
    {
        // START
        StringBuilder productBuilder = new();
        StringBuilder countBuilder = new();
        DynamicParameters p = new();
        bool hasCategories = request.CategoryIds is not null && request.CategoryIds.Count > 0;
        bool hasSearchText = !string.IsNullOrWhiteSpace( request.SearchText );
        
        // CATEGORY JOIN
        if (hasCategories) {
            productBuilder.Append( CategoryJoinSql );
            countBuilder.Append( CategoryJoinSql );
        }
        // SEARCH TEXT JOIN
        if (hasSearchText) {
            productBuilder.Append( SearchTextJoinSql );
            countBuilder.Append( SearchTextJoinSql );
        }
        
        // MAIN SELECT
        productBuilder.Append( ProductSql );
        countBuilder.Append( CountSql );
        
        // FILTER BY CATEGORY
        if (hasCategories) {
            productBuilder.Append( CategorySql );
            countBuilder.Append( CategorySql );
            p.Add( "categoryIds", GetDataTable( request.CategoryIds! ) );
        }
        // FILTER BY SEARCH TEXT
        if (hasSearchText) {
            productBuilder.Append( SearchTextSql );
            countBuilder.Append( SearchTextSql );
            p.Add( "searchText", request.SearchText );
        }
        
        // EARLY IF NO OTHER FILTERS
        if (request.ProductSearchFilters is null) {
            Finish( out sql, out parameters );
            return;
        }
        
        // OTHER FILTERS
        SearchFilters filters = request.ProductSearchFilters.Value;
        productBuilder.Append( WhereStatementSql );
        
        // BRANDS
        if (filters.BrandIds is not null) {
            productBuilder.Append( BrandsSql );
            countBuilder.Append( BrandsSql );
            p.Add( "brandIds", GetDataTable( filters.BrandIds ) );
        }
        // MIN PRICE
        if (filters.MinPrice is not null) {
            productBuilder.Append( MinPriceSql );
            countBuilder.Append( MinPriceSql );
            p.Add( "minPrice", filters.MinPrice );
        }
        // MAX PRICE
        if (filters.MaxPrice is not null) {
            productBuilder.Append( MaxPriceSql );
            countBuilder.Append( MaxPriceSql );
            p.Add( "maxPrice", filters.MaxPrice );
        }
        // IN STOCK
        if (filters.IsInStock is not null) {
            productBuilder.Append( StockSql );
            countBuilder.Append( StockSql );
            p.Add( "isInStock", filters.IsInStock );
        }
        // IS FEATURED
        if (filters.IsFeatured is not null) {
            productBuilder.Append( FeaturedSql );
            countBuilder.Append( FeaturedSql );
            p.Add( "isFeatured", filters.IsFeatured );
        }
        // IS ON SALE
        if (filters.IsOnSale) {
            productBuilder.Append( SaleSql );
            countBuilder.Append( SaleSql );
        }
        
        // PAGINATION
        Finish( out sql, out parameters );
        return;
        
        // UTIL METHOD
        void Finish( out string sql, out DynamicParameters parameters )
        {
            // language=sql
            string orderPaginationSql = $" ORDER BY {GetOrderType( request.Pagination.SortBy )} OFFSET @offset ROWS FETCH NEXT @rows ONLY";
            productBuilder.Append( orderPaginationSql );
            sql = $"{countBuilder}; {productBuilder}";

            p.Add( "orderBy", request.Pagination.SortBy );
            p.Add( "rows", request.Pagination.PageSize );
            p.Add( "offset", request.Pagination.Offset() );
            parameters = p;
        }
    }
    
    static DataTable GetDataTable( List<Guid> ids )
    {
        const string idColumn = "Id";
        DataTable table = new();

        table.Columns.Add( idColumn, typeof( Guid ) );

        foreach ( Guid i in ids ) {
            DataRow row = table.NewRow();
            row[idColumn] = i;
            table.Rows.Add( row );
        }

        return table;
    }
    static string GetOrderType( string typeString )
    {
        if (!Enum.TryParse( typeString, true, out OrderType type ))
            return "IsFeatured ASC";

        return type switch {
            OrderType.Featured => "IsFeatured DESC, IsFeatured ASC",
            OrderType.OnSale => "IsOnSale DESC, IsOnSale ASC",
            OrderType.Rating => "Rating DESC",
            OrderType.PriceLow => "Price ASC",
            OrderType.PriceHigh => "Price DESC",
            _ => "IsFeatured ASC"
        };
    }
    
    enum OrderType
    {
        Featured,
        OnSale,
        Rating,
        PriceLow,
        PriceHigh
    }
}