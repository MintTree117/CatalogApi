using System.Data;
using System.Text;
using CatalogApplication.Database;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Products.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class ProductSearchRepository
{
    readonly IServiceProvider _provider;
    readonly ILogger<ProductSearchRepository> _logger;

    // language=sql
    const string CategorySql =
        """
        WITH ProductCategoryFilter AS (
            SELECT DISTINCT ProductId
            FROM ProductCategories
            WHERE CategoryId IN (SELECT Id FROM @categoryIds)
        ),
        """;
    // language=sql
    const string ProductSql = "SELECT p.Id, p.BrandId, p.Name, p.Image, p.Rating, p.Price, p.SalePrice FROM Products p";
    // language=sql
    const string CountSql = "SELECT COUNT(*) FROM Products p";
    // language=sql
    const string CategoryJoinSql = " INNER JOIN ProductCategoryFilter pc ON p.Id = pc.ProductId";
    // language=sql
    const string WhereStatementSql = " WHERE 1=1";
    // language=sql
    const string BrandsSql = " AND p.BrandId IN (SELECT Id FROM @brandIds)";
    // language=sql
    const string PriceSql = " AND p.PriceRangeId IN (SELECT Id FROM @priceRangeIds)";
    // language=sql
    const string RatingSql = " AND p.RatingLevelId IN (SELECT Id FROM @ratingLevelIds)";
    // language=sql
    const string StockSql = " AND p.IsInStock = @isInStock";
    // language=sql
    const string FeaturedSql = " AND p.IsFeatured = @isFeatured";
    // language=sql
    const string SaleSql = " AND p.IsOnSale = @isOnSale";

    public ProductSearchRepository( IServiceProvider provider, ILogger<ProductSearchRepository> logger )
    {
        _provider = provider;
        _logger = logger;
    }
    internal async Task<SearchQueryReply?> GetSearch( SearchQueryRequest queryRequest )
    {
        try {
            IDapperContext dapper = IDapperContext.GetContext( _provider );
            await using SqlConnection connection = await dapper.GetOpenConnection();

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
        
        // CATEGORIES
        if (hasCategories) {
            productBuilder.Append( CategorySql );
            countBuilder.Append( CategorySql );
            p.Add( "categoryIds", GetDataTable( searchQuery.CategoryIds! ) );
        }
        
        // PRODUCTS & COUNT
        productBuilder.Append( ProductSql );
        countBuilder.Append( CountSql );
        
        // CATEGORIES
        if (hasCategories) {
            productBuilder.Append( CategoryJoinSql );
            countBuilder.Append( CategoryJoinSql );
        }
        
        // EARLY IF NO FILTERS
        if (searchQuery.ProductSearchFilters is null) {
            Finish( out sql, out parameters );
            return;
        }
        
        // FILTERS
        SearchFiltersDto filtersDto = searchQuery.ProductSearchFilters.Value;
        productBuilder.Append( WhereStatementSql );
        
        if (filtersDto.BrandIds is not null) {
            productBuilder.Append( BrandsSql );
            countBuilder.Append( BrandsSql );
            p.Add( "brandIds", GetDataTable( filtersDto.BrandIds ) );
        }
        if (filtersDto.PriceRangeIds is not null) {
            productBuilder.Append( PriceSql );
            countBuilder.Append( PriceSql );
            p.Add( "priceRangeIds", GetDataTable( filtersDto.PriceRangeIds ) );
        }
        if (filtersDto.RatingLevelIds is not null) {
            productBuilder.Append( RatingSql );
            countBuilder.Append( RatingSql );
            p.Add( "ratingLevelIds", GetDataTable( filtersDto.RatingLevelIds ) );
        }
        if (filtersDto.IsInStock is not null) {
            productBuilder.Append( StockSql );
            countBuilder.Append( StockSql );
            p.Add( "isInStock", filtersDto.IsInStock );
        }
        if (filtersDto.IsFeatured is not null) {
            productBuilder.Append( FeaturedSql );
            countBuilder.Append( FeaturedSql );
            p.Add( "isFeatured", filtersDto.IsFeatured );
        }
        if (filtersDto.IsOnSale is not null) {
            productBuilder.Append( SaleSql );
            countBuilder.Append( SaleSql );
            p.Add( "isOnSale", filtersDto.IsOnSale );
        }
        
        // PAGINATION
        Finish( out sql, out parameters );
        return;

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