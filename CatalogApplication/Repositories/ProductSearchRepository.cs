using System.Data;
using System.Text;
using CatalogApplication.Database;
using CatalogApplication.Types.Search.Dtos;
using CatalogApplication.Types.Search.Local;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class ProductSearchRepository( IDapperContext dapper, ILogger<ProductSearchRepository> logger ) 
    : BaseRepository<ProductSearchRepository>( logger )
{
    readonly IDapperContext _dapper = dapper;
    
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
    const string CategorySql = " AND pc.CategoryId = @categoryId";
    // language=sql
    const string SearchTextSql = " AND p.Name LIKE '%' + @searchText + '%' OR pt.Name LIKE '%' + @searchText + '%';";
    // language=sql
    const string BrandsSql = " AND p.BrandId IN (SELECT Id FROM @brandIds)";
    // language=sql
    const string MinPriceSql = " AND (p.Price >= @minPrice OR p.SalePrice >= @minPrice)";
    // language=sql
    const string MaxPriceSql = " AND (p.Price <= @maxPrice OR (p.SalePrice > 0 AND p.SalePrice <= @maxPrice))";
    // language=sql
    const string StockSql = " AND p.IsInStock = 1";
    // language=sql
    const string FeaturedSql = " AND p.IsFeatured = 1";
    // language=sql
    const string SaleSql = " AND p.SalePrice >= 0";

    internal async Task<SearchQueryReply?> GetSearch( SearchFilters filters )
    {
        LogInformation( $"{filters.CategoryId} {filters.Page} {filters.IsInStock}" );
        
        try {
            await using SqlConnection connection = await _dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                LogError( $"Invalid connection state: {connection.State}" );
                return null;
            }

            BuildSqlQuery( filters, out string sql, out DynamicParameters parameters );
            await using SqlMapper.GridReader multi = await connection.QueryMultipleAsync( sql, parameters, commandType: CommandType.Text );

            SearchQueryReply queryReply = new(
                await multi.ReadSingleAsync<int>(),
                (await multi.ReadAsync<SearchItemDto>()).ToList() );

            return queryReply;
        }
        catch ( Exception e ) {
            LogException( e, $"An exception occured while executing product search: {e.Message}" );
            return null;
        }
    }
    internal void BuildSqlQuery( SearchFilters filters, out string sql, out DynamicParameters parameters )
    {
        // START
        StringBuilder productBuilder = new();
        StringBuilder countBuilder = new();
        DynamicParameters p = new();
        bool hasCategory = filters.CategoryId is not null;
        bool hasSearchText = !string.IsNullOrWhiteSpace( filters.SearchText );

        // MAIN SELECT
        productBuilder.Append( ProductSql );
        countBuilder.Append( CountSql );
        
        // CATEGORY JOIN
        if (hasCategory) {
            productBuilder.Append( CategoryJoinSql );
            countBuilder.Append( CategoryJoinSql );
        }
        // SEARCH TEXT JOIN
        if (hasSearchText) {
            productBuilder.Append( SearchTextJoinSql );
            countBuilder.Append( SearchTextJoinSql );
        }
        
        // DEFAULT WHERE CLAUSE
        productBuilder.Append( WhereStatementSql );
        countBuilder.Append( WhereStatementSql );
        
        // FILTER BY CATEGORY
        if (hasCategory) {
            productBuilder.Append( CategorySql );
            countBuilder.Append( CategorySql );
            p.Add( "CategoryId", filters.CategoryId );
        }
        // FILTER BY SEARCH TEXT
        if (hasSearchText) {
            productBuilder.Append( SearchTextSql );
            countBuilder.Append( SearchTextSql );
            p.Add( "searchText", filters.SearchText );
        }

        // BRANDS
        if (filters.BrandIds is not null) {
            productBuilder.Append( BrandsSql );
            countBuilder.Append( BrandsSql );
            p.Add( "brandIds", GetDataTable( filters.BrandIds ).AsTableValuedParameter( "CatalogApi.BrandIdsTvp" ) );
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
        
        // language=sql
        string orderPaginationSql = $" ORDER BY {GetOrderType( filters.SortBy )} OFFSET @offset ROWS FETCH NEXT @rows ROWS ONLY";
        productBuilder.Append( orderPaginationSql );
        sql = $"{countBuilder}; {productBuilder};";

        p.Add( "orderBy", filters.SortBy );
        p.Add( "rows", filters.PageSize );
        p.Add( "offset", Math.Max( 0, filters.Page - 1 ) * filters.PageSize );
        parameters = p;
        Logger.LogInformation( sql );
        return;

        static string GetOrderType( int t )
        {
            return (OrderType) t switch {
                OrderType.PriceLow => "p.Price ASC",
                OrderType.OnSale => "p.IsOnSale DESC",
                OrderType.Rating => "p.Rating DESC",
                OrderType.PriceHigh => "p.Price DESC",
                _ => "p.Price ASC"
            };
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
    
    enum OrderType
    {
        Featured,
        OnSale,
        Rating,
        PriceLow,
        PriceHigh
    }
}