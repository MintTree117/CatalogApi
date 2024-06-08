using System.Data;
using System.Text;
using CatalogApplication.Database;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Products.Models;
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
    const string ProductSql = "SELECT DISTINCT p.Id, p.BrandId, p.Name, p.Image, p.Rating, p.Price, p.SalePrice FROM CatalogApi.Products p";
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
    const string RatingSql = " AND p.Rating >= @minimumRating";
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
                (await multi.ReadAsync<SearchDto>()).ToList() );

            return queryReply;
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"An exception occured while executing product search: {e.Message}" );
            return null;
        }
    }

    static void BuildSqlQuery( SearchQueryRequest searchQuery, out string sql, out DynamicParameters parameters )
    {
        // START
        StringBuilder productBuilder = new();
        StringBuilder countBuilder = new();
        DynamicParameters p = new();
        bool hasCategories = searchQuery.CategoryIds is not null && searchQuery.CategoryIds.Count > 0;
        bool hasSearchText = !string.IsNullOrWhiteSpace( searchQuery.SearchText );
        
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
            p.Add( "categoryIds", GetDataTable( searchQuery.CategoryIds! ) );
        }
        // FILTER BY SEARCH TEXT
        if (hasSearchText) {
            productBuilder.Append( SearchTextSql );
            countBuilder.Append( SearchTextSql );
            p.Add( "searchText", searchQuery.SearchText );
        }
        
        // EARLY IF NO OTHER FILTERS
        if (searchQuery.ProductSearchFilters is null) {
            Finish( out sql, out parameters );
            return;
        }
        
        // OTHER FILTERS
        SearchFiltersDto filtersDto = searchQuery.ProductSearchFilters.Value;
        productBuilder.Append( WhereStatementSql );
        
        // BRANDS
        if (filtersDto.BrandIds is not null) {
            productBuilder.Append( BrandsSql );
            countBuilder.Append( BrandsSql );
            p.Add( "brandIds", GetDataTable( filtersDto.BrandIds ) );
        }
        // MIN PRICE
        if (filtersDto.MinimumPrice is not null) {
            productBuilder.Append( MinPriceSql );
            countBuilder.Append( MinPriceSql );
            p.Add( "minPrice", filtersDto.MinimumPrice );
        }
        // MAX PRICE
        if (filtersDto.MaximumPrice is not null) {
            productBuilder.Append( MaxPriceSql );
            countBuilder.Append( MaxPriceSql );
            p.Add( "maxPrice", filtersDto.MaximumPrice );
        }
        // MIN RATING
        if (filtersDto.MinimumRating is not null) {
            productBuilder.Append( RatingSql );
            countBuilder.Append( RatingSql );
            p.Add( "minRating", filtersDto.MinimumRating );
        }
        // IN STOCK
        if (filtersDto.IsInStock is not null) {
            productBuilder.Append( StockSql );
            countBuilder.Append( StockSql );
            p.Add( "isInStock", filtersDto.IsInStock );
        }
        // IS FEATURED
        if (filtersDto.IsFeatured is not null) {
            productBuilder.Append( FeaturedSql );
            countBuilder.Append( FeaturedSql );
            p.Add( "isFeatured", filtersDto.IsFeatured );
        }
        // IS ON SALE
        if (filtersDto.IsOnSale) {
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
            string OrderPaginationSql = $" ORDER BY {GetOrderType( searchQuery.Pagination.OrderBy )} OFFSET @offset ROWS FETCH NEXT @rows ONLY";
            productBuilder.Append( OrderPaginationSql );
            sql = $"{countBuilder}; {productBuilder}";

            p.Add( "orderBy", searchQuery.Pagination.OrderBy );
            p.Add( "rows", searchQuery.Pagination.Rows );
            p.Add( "offset", searchQuery.Pagination.Offset() );
            parameters = p;
        }
    }
    
    static DataTable GetDataTable( List<Guid> ids )
    {
        const string IdColumn = "Id";
        DataTable table = new();

        table.Columns.Add( IdColumn, typeof( Guid ) );

        foreach ( Guid i in ids ) {
            DataRow row = table.NewRow();
            row[IdColumn] = i;
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