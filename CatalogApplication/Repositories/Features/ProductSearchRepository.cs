using System.Data;
using System.Text;
using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Products.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories.Features;

internal sealed class ProductSearchRepository( IDapperContext dapper, ILogger<ProductSearchRepository> logger ) 
    : BaseRepository<ProductSearchRepository>( dapper, logger )
{
    // language=sql
    const string SqlTextSearchNewCross =
        """
         CROSS APPLY (
            SELECT value AS ProductWord
                FROM STRING_SPLIT(p.Name, ' ')
            UNION ALL
            SELECT value
                FROM STRING_SPLIT(p.Name, ',')
        ) AS SplitProduct
        """;
    // language=sql
    const string SqlTextSearchNewFilter =
        """
         EXISTS (
            SELECT 1
            FROM STRING_SPLIT(@SearchText, ' ') AS SearchWords
            WHERE SplitProduct.ProductWord LIKE '%' + SearchWords.value + '%'
        )
        """;
    
    internal async Task<Reply<SearchQueryReply>> SearchFull( SearchFilters filters )
    {
        try 
        {
            await using SqlConnection connection = await Dapper.GetOpenConnection();
            if (connection.State != ConnectionState.Open) 
            {
                LogError( $"Invalid connection state: {connection.State}" );
                return Reply<SearchQueryReply>.ServerError();
            }
            
            SearchQueryBuilder builder = BuildCatalogSearchSql( filters );
            await using SqlMapper.GridReader multi = await connection.QueryMultipleAsync( builder.GetSql(), builder.parameters, commandType: CommandType.Text );
            
            SearchQueryReply queryReply = new(
                await multi.ReadSingleAsync<int>(),
                (await multi.ReadAsync<ProductSummaryDto>()).ToList() );

            return Reply<SearchQueryReply>.Success( queryReply );
        }
        catch ( Exception e ) 
        {
            LogException( e, $"An exception occured while executing product search: {e.Message}" );
            return Reply<SearchQueryReply>.ServerError();
        }
    }
    internal async Task<Replies<ProductSummaryDto>> SearchIds( List<Guid> productIds )
    {
        // language=sql
        const string sql = "SELECT TOP 10 p.* FROM CatalogApi.Products p WHERE p.Id IN (SELECT Id FROM @productIds)";
        var idsTable = GetIdsDataTable( productIds );
        var parameters = new DynamicParameters();
        parameters.Add( "productIds", idsTable.AsTableValuedParameter( "CatalogApi.IdsTvp" ) );
        var reply = await Dapper.QueryAsync<ProductSummaryDto>( sql, parameters );
        return reply;
    }
    internal async Task<Replies<ProductSuggestionDto>> SearchSuggestions( string searchText )
    {
        // language=sql
        const string sql = $"SELECT TOP 10 p.Id, p.Name FROM CatalogApi.Products p {SqlTextSearchNewCross} WHERE {SqlTextSearchNewFilter}";
        var parameters = new DynamicParameters();
        parameters.Add( "searchText", searchText );
        var replies = await Dapper.QueryAsync<ProductSuggestionDto>( sql, parameters );
        return replies;
    }
    internal async Task<Reply<ProductsSimilarDto>> SearchSimilar( Guid productId, Guid brandId )
    {
        // language=sql
        const string sql =
            """
            DECLARE @categoryIds TABLE (CategoryId UNIQUEIDENTIFIER);
            
            INSERT INTO @categoryIds (CategoryId)
            SELECT CategoryId
            FROM CatalogApi.ProductCategories
            WHERE ProductId = @productId;
            
            SELECT TOP 10 p.*
            FROM CatalogApi.Products p
                JOIN CatalogApi.ProductCategories pc ON p.Id = pc.ProductId
                    WHERE pc.CategoryId IN (SELECT CategoryId FROM @categoryIds)
                    AND p.BrandId = @brandId
                    AND p.Id <> @productId
            ORDER BY NEWID();

            DECLARE @categoryIds2 TABLE (CategoryId UNIQUEIDENTIFIER);
            
            INSERT INTO @categoryIds2 (CategoryId)
            SELECT CategoryId
            FROM CatalogApi.ProductCategories
            WHERE ProductId = @productId;
            
            SELECT TOP 10 p.*
            FROM CatalogApi.Products p
                JOIN CatalogApi.ProductCategories pc ON p.Id = pc.ProductId
                    WHERE pc.CategoryId IN (SELECT CategoryId FROM @categoryIds2)
                    AND p.Id <> @productId
            ORDER BY NEWID();
            """;

        DynamicParameters parameters = new();
        parameters.Add( "productId", productId );
        parameters.Add( "brandId", brandId );

        try
        {
            await using var connection = await Dapper.GetOpenConnection();
            if (connection.State != ConnectionState.Open)
                return Reply<ProductsSimilarDto>.ServerError();

            var multi = await connection.QueryMultipleAsync( sql, parameters );

            ProductsSimilarDto dto = new(
                (await multi.ReadAsync<ProductSummaryDto>()).ToList(),
                (await multi.ReadAsync<ProductSummaryDto>()).ToList() );

            return Reply<ProductsSimilarDto>.Success( dto );
        }
        catch ( Exception e )
        {
            LogException( e, "An exception was thrown while trying to query dapper for similar products." );
            return Reply<ProductsSimilarDto>.ServerError();
        }
    }
    
    static SearchQueryBuilder BuildCatalogSearchSql( SearchFilters filters )
    {
        SearchQueryBuilder searchQueryBuilder = new(
            new StringBuilder(),
            new StringBuilder(),
            new DynamicParameters() );

        return searchQueryBuilder
            .SelectProducts()
            .SelectCount()
            .JoinCategories( filters.CategoryId )
            .CrossSearchText( filters.SearchText )
            .StartFiltering()
            .FilterByIsFeatured( filters.IsFeatured.HasValue )
            .FilterByIsInStock( filters.IsInStock.HasValue )
            .FilterByIsOnSale( filters.IsOnSale.HasValue )
            .FilterByIsFreeShipping( filters.IsFreeShipping.HasValue )
            .FilterByBrands( filters.BrandIds )
            .FilterByCategory( filters.CategoryId )
            .FilterBySearchText( filters.SearchText )
            .FilterByMinPrice( filters.MinPrice )
            .FilterByMaxPrice( filters.MaxPrice )
            .OrderBy( filters.SortBy )
            .Paginate( filters.Page, filters.PageSize );
    }
    static DataTable GetIdsDataTable( List<Guid> ids )
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

    readonly record struct SearchQueryBuilder(
        StringBuilder productBuilder,
        StringBuilder countBuilder,
        DynamicParameters parameters )
    {
        // language=sql
        const string SelectProductsSql =
            """
            SELECT DISTINCT
                p.Id, p.BrandId, p.Name, p.BrandName, p.Image, p.IsFeatured, p.IsInStock, p.Price, p.SalePrice, p.ShippingPrice, p.SaleEndDate, p.ReleaseDate, p.Rating, p.NumberRatings, p.NumberSold, p.Weight, p.Dimensions
            FROM CatalogApi.Products p
            """;
        // language=sql
        const string SelectCountSql = "SELECT COUNT(*) FROM CatalogApi.Products p";
        // language=sql
        const string JoinCategoriesSql = " INNER JOIN CatalogApi.ProductCategories pc ON p.Id = pc.ProductId";
        // language=sql
        const string BaseWhereClauseSql = " WHERE 1=1";
        // language=sql
        const string CategorySql = " AND pc.CategoryId = @categoryId";
        // language=sql
        const string SearchTextSql = $" AND {SqlTextSearchNewFilter}";
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
        const string SaleSql = " AND p.SalePrice IS NOT NULL";
        // language=sql
        const string ShippingSql = " AND p.ShippingPrice IS NULL";
        // language=sql
        const string PaginationSql = " OFFSET @offset ROWS FETCH NEXT @rows ROWS ONLY";
        
        internal string GetSql() =>
            $"{countBuilder}; {productBuilder};";
        internal SearchQueryBuilder SelectProducts()
        {
            productBuilder.Append( SelectProductsSql );
            return this;
        }
        internal SearchQueryBuilder SelectCount()
        {
            countBuilder.Append( SelectCountSql );
            return this;
        }
        internal SearchQueryBuilder CrossSearchText( string? searchText = null )
        {
            if (string.IsNullOrWhiteSpace( searchText ))
                return this;
            productBuilder.Append( SqlTextSearchNewCross );
            countBuilder.Append( SqlTextSearchNewCross );
            return this;
        }
        internal SearchQueryBuilder JoinCategories( Guid? categoryId = null )
        {
            if (categoryId is null) 
                return this;
            productBuilder.Append( JoinCategoriesSql );
            countBuilder.Append( JoinCategoriesSql );
            return this;
        }
        internal SearchQueryBuilder StartFiltering()
        {
            productBuilder.Append( BaseWhereClauseSql );
            countBuilder.Append( BaseWhereClauseSql );
            return this;
        }
        internal SearchQueryBuilder FilterByIsFeatured( bool shouldFilter )
        {
            if (!shouldFilter)
                return this;
            productBuilder.Append( FeaturedSql );
            countBuilder.Append( FeaturedSql );
            parameters.Add( "isFeatured", true );
            return this;
        }
        internal SearchQueryBuilder FilterByIsOnSale( bool shouldFilter )
        {
            if (!shouldFilter)
                return this;
            productBuilder.Append( SaleSql );
            countBuilder.Append( SaleSql );
            return this;
        }
        internal SearchQueryBuilder FilterByIsInStock( bool shouldFilter )
        {
            if (!shouldFilter)
                return this;
            productBuilder.Append( StockSql );
            countBuilder.Append( StockSql );
            parameters.Add( "isInStock", true );
            return this;
        }
        internal SearchQueryBuilder FilterByIsFreeShipping( bool shouldFilter )
        {
            if (!shouldFilter)
                return this;
            productBuilder.Append( ShippingSql );
            countBuilder.Append( ShippingSql );
            parameters.Add( "isFreeShipping", true );
            return this;
        }
        internal SearchQueryBuilder FilterByCategory( Guid? categoryId = null )
        {
            if (categoryId is null)
                return this;
            productBuilder.Append( CategorySql );
            countBuilder.Append( CategorySql );
            parameters.Add( "CategoryId", categoryId );
            return this;
        }
        internal SearchQueryBuilder FilterBySearchText( string? searchText = null )
        {
            if (string.IsNullOrWhiteSpace( searchText ))
                return this;
            productBuilder.Append( SearchTextSql );
            countBuilder.Append( SearchTextSql );
            parameters.Add( "searchText", searchText );
            return this;
        }
        internal SearchQueryBuilder FilterByBrands( List<Guid>? brandIds )
        {
            if (brandIds is null)
                return this;
            productBuilder.Append( BrandsSql );
            countBuilder.Append( BrandsSql );
            parameters.Add( "brandIds", GetIdsDataTable( brandIds ).AsTableValuedParameter( "CatalogApi.IdsTvp" ) );
            return this;
        }
        internal SearchQueryBuilder FilterByMinPrice( int? minPrice )
        {
            if (minPrice is null)
                return this;
            productBuilder.Append( MinPriceSql );
            countBuilder.Append( MinPriceSql );
            parameters.Add( "minPrice", minPrice.Value );
            return this;
        }
        internal SearchQueryBuilder FilterByMaxPrice( int? maxPrice )
        {
            if (maxPrice is null)
                return this;
            productBuilder.Append( MaxPriceSql );
            countBuilder.Append( MaxPriceSql );
            parameters.Add( "maxPrice", maxPrice.Value );
            return this;
        }
        internal SearchQueryBuilder OrderBy( int orderBy )
        {
            // language=sql
            string orderBySql = $" ORDER BY {GetOrderByType( orderBy )}";
            productBuilder.Append( orderBySql );
            parameters.Add( "orderBy", orderBy );
            return this;
        }
        internal SearchQueryBuilder Paginate( int page, int pageSize )
        {
            int offset = Math.Max( 0, page - 1 ) * pageSize;
            productBuilder.Append( PaginationSql );
            parameters.Add( "offset", offset );
            parameters.Add( "rows", pageSize );
            return this;
        }
        static string GetOrderByType( int t )
        {
            return (OrderType) t switch {
                // language=sql
                OrderType.BestSelling => "p.NumberSold DESC",
                // language=sql
                OrderType.BestRating => "p.Rating DESC",
                // language=sql
                OrderType.MostRatings => "p.NumberRatings DESC",
                // language=sql
                OrderType.PriceLow => "p.Price ASC, CASE WHEN p.SalePrice = NULL THEN NULL ELSE p.SalePrice END ASC",
                // language=sql
                OrderType.PriceHigh => "p.Price DESC, CASE WHEN p.SalePrice = NULL THEN NULL ELSE p.SalePrice END DESC",
                // language=sql
                OrderType.Newest => "p.ReleaseDate DESC",
                // language=sql
                OrderType.Oldest => "p.ReleaseDate ASC",
                // language=sql
                _ => "p.NumberSold DESC"
            };
        }
        enum OrderType
        {
            BestSelling,
            BestRating,
            MostRatings,
            PriceLow,
            PriceHigh,
            Newest,
            Oldest
        }
    }
}