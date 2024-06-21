using System.Data;
using System.Xml;
using CatalogApplication.Seeding.SeedData;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Products.Models;

namespace CatalogApplication.Seeding.Generators;

internal static class ProductGenerator
{
    const int LoopSafety = 1000;
    const int ProductsPerPrimaryCategory = 20;

    internal static ProductSeedingModel GenerateProducts( List<Category> primaryCategories, Dictionary<Guid, List<Category>> secondaryCategories, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
    {
        List<Product> products = [];
        List<ProductCategory> productCategories = [];
        List<ProductDescription> productDescriptions = [];
        List<ProductXml> productXmls = [];

        foreach ( Category primaryCategory in primaryCategories ) {
            for ( int i = 0; i < ProductsPerPrimaryCategory; i++ ) {
                
                List<Category> sc = 
                    PickRandomCategories( primaryCategory, secondaryCategories, random );
                int numSold = PickNumberSold( random );
                (int,float) rating = PickRating( numSold, random );
                Product p = new(
                    Guid.NewGuid(),
                    primaryCategory.Id,
                    PickBrandId( sc, brands, brandCategories, random ),
                    PickIsInStock( random ),
                    PickIsFeatured( random ),
                    PickName( primaryCategory, i ),
                    PickImage( primaryCategory, random ),
                    PickPrice( random, out decimal price ),
                    PickSalePrice( price, random ),
                    numSold,
                    rating.Item1,
                    rating.Item2 );
                List<ProductCategory> pc = 
                    GenerateProductCategories( p, sc );
                ProductDescription pd =
                    GenerateProductDescription( p, random );
                ProductXml px =
                    GenerateProductXml( p, primaryCategories, random );
                
                products.Add( p );
                productCategories.AddRange( pc );
                productDescriptions.Add( pd );
                productXmls.Add( px );
            }
        }
        
        return new ProductSeedingModel( 
            products, 
            productCategories, 
            productDescriptions, 
            productXmls );
    }
    internal static DataTable GenerateProductsTable( List<Product> products )
    {
        DataTable table = new();
        table.Columns.Add( nameof( Product.Id ), typeof( Guid ) );
        table.Columns.Add( nameof( Product.PrimaryCategoryId ), typeof( Guid ) );
        table.Columns.Add( nameof( Product.BrandId ), typeof( Guid ) );
        table.Columns.Add( nameof( Product.IsInStock ), typeof( bool ) );
        table.Columns.Add( nameof( Product.IsFeatured ), typeof( bool ) );
        table.Columns.Add( nameof( Product.Name ), typeof( string ) );
        table.Columns.Add( nameof( Product.Image ), typeof( string ) );
        table.Columns.Add( nameof( Product.Price ), typeof( decimal ) );
        table.Columns.Add( nameof( Product.SalePrice ), typeof( decimal ) );
        table.Columns.Add( nameof( Product.NumberSold ), typeof( int ) );
        table.Columns.Add( nameof( Product.NumberRatings ), typeof( int ) );
        table.Columns.Add( nameof( Product.Rating ), typeof( float ) );
        
        foreach ( Product p in products ) {
            DataRow row = table.NewRow();
            row[nameof( Product.Id )] = p.Id;
            row[nameof( Product.PrimaryCategoryId )] = p.PrimaryCategoryId;
            row[nameof( Product.BrandId )] = p.BrandId;
            row[nameof( Product.IsInStock )] = p.IsInStock;
            row[nameof( Product.IsFeatured )] = p.IsFeatured;
            row[nameof( Product.Name )] = p.Name;
            row[nameof( Product.Image )] = p.Image;
            row[nameof( Product.Price )] = p.Price;
            row[nameof( Product.SalePrice )] = p.SalePrice;
            row[nameof( Product.NumberSold )] = p.NumberSold;
            row[nameof( Product.NumberRatings )] = p.NumberRatings;
            row[nameof( Product.Rating )] = p.Rating;
            table.Rows.Add( row );
        }

        return table;
    }
    internal static DataTable GenerateProductCategoriesTable( List<ProductCategory> productCategories )
    {
        DataTable table = new();
        table.Columns.Add( nameof( ProductCategory.ProductId ), typeof( Guid ) );
        table.Columns.Add( nameof( ProductCategory.CategoryId ), typeof( Guid ) );

        foreach ( ProductCategory pc in productCategories ) {
            DataRow row = table.NewRow();
            row[nameof( ProductCategory.ProductId )] = pc.ProductId;
            row[nameof( ProductCategory.CategoryId )] = pc.CategoryId;
            table.Rows.Add( row );
        }

        return table;
    }
    internal static DataTable GenerateProductDescriptionsTable( List<ProductDescription> productDescriptions )
    {
        DataTable table = new();
        table.Columns.Add( nameof( ProductDescription.ProductId ), typeof( Guid ) );
        table.Columns.Add( nameof( ProductDescription.Description ), typeof( string ) );

        foreach ( ProductDescription d in productDescriptions ) {
            DataRow row = table.NewRow();
            row[nameof( ProductDescription.ProductId )] = d.ProductId;
            row[nameof( ProductDescription.Description )] = d.Description;
            table.Rows.Add( row );
        }

        return table;
    }
    internal static DataTable GenerateProductXmlTable( List<ProductXml> productXmls )
    {
        DataTable table = new();
        table.Columns.Add( nameof( ProductXml.ProductId ), typeof( Guid ) );
        table.Columns.Add( nameof( ProductXml.Xml ), typeof( string ) );

        foreach ( ProductXml x in productXmls ) {
            DataRow row = table.NewRow();
            row[nameof( ProductXml.ProductId )] = x.ProductId;
            row[nameof( ProductXml.Xml )] = x.Xml;
            table.Rows.Add( row );
        }

        return table;
    }
    
    static List<ProductCategory> GenerateProductCategories( Product product, List<Category> selectedCategories )
    {
        List<ProductCategory> pc = [];
        foreach ( Category c in selectedCategories )
            pc.Add( new ProductCategory( product.Id, c.Id ) );
        return pc;
    }
    static ProductDescription GenerateProductDescription( Product product, RandomUtility random )
    {
        int index = random.GetRandomInt( ProductSeedData.ProductDescriptions.Length - 1 );
        string text = ProductSeedData.ProductDescriptions[index];
        return new ProductDescription( product.Id, text );
    }
    static ProductXml GenerateProductXml( Product product, List<Category> primaryCategories, RandomUtility random )
    {
        // INITIALIZATION
        Category c = primaryCategories.First( pc => pc.Id == product.PrimaryCategoryId );
        Dictionary<string, string[]> specs = ProductXmlSeedData.ProductSpecsByCategory[c.Name];
        List<string> keys = specs.Keys.ToList();
        HashSet<int> specUsed = [];
        int numSpecs = random.GetRandomInt( keys.Count - 1 );
        
        // SELECT SPECS
        Dictionary<string, List<string>> specsAndValues = [];
        for ( int i = 0; i <= numSpecs; i++ ) {
            for ( int j = 0; j < LoopSafety; j++ ) {
                
                // TRY GET NEW
                int specIndex = random.GetRandomInt( keys.Count - 1 );
                if (!specUsed.Add( specIndex ))
                    continue;
                
                // INIT NEW SPEC
                string spec = keys[specIndex];
                string[] values = specs[keys[specIndex]];
                int valueCount = random.GetRandomInt( values.Length - 1 );
                
                // IF BOOLEAN VALUE TYPE
                if (valueCount == 2) {
                    string boolValue = values[random.GetRandomInt( 1 )];
                    specsAndValues.Add( spec, [boolValue] );
                    break;
                }
                
                // MULTIPLE CHOICE TYPE
                List<string> selectedValues = [];
                HashSet<int> valueUsed = [];
                for ( int k = 0; k < valueCount; k++ ) {
                    for ( int l = 0; l <= LoopSafety; l++ ) {
                        int valueIndex = random.GetRandomInt( valueCount );
                        if (!valueUsed.Add( valueIndex ))
                            continue;
                        selectedValues.Add( values[valueIndex] );
                    }
                }
                specsAndValues.Add( spec, selectedValues );
            }
        }
        
        // GENERATE XML
        XmlDocument xmlDoc = new();
        XmlElement root = xmlDoc.CreateElement( "ProductSpecs" );
        xmlDoc.AppendChild( root );

        foreach ( KeyValuePair<string, List<string>> kvp in specsAndValues ) {
            string xmlName = kvp.Key.Replace( ' ', '-' );
            XmlElement specElement = xmlDoc.CreateElement( xmlName );
            foreach ( string value in kvp.Value ) {
                string xmlValue = value.Replace( ' ', '-' );
                XmlElement valueElement = xmlDoc.CreateElement( "Value" );
                valueElement.InnerText = xmlValue;
                specElement.AppendChild( valueElement );
            }
            root.AppendChild( specElement );
        }
        return new ProductXml( product.Id, xmlDoc.InnerXml );
    }

    static List<Category> PickRandomCategories( Category primaryCategory, Dictionary<Guid, List<Category>> secondaryCategories, RandomUtility random )
    {
        List<Category> selectedSecondary = [];
        int numSecondary = random.GetRandomInt( 1, 4 );
        for ( int i = 0; i < numSecondary; i++ ) {
            for ( int j = 0; j < LoopSafety; j++ ) {
                List<Category> subCategories = secondaryCategories[primaryCategory.Id];
                int index = random.GetRandomInt( subCategories.Count - 1 );
                Category currentCategory = subCategories[index];
                if (selectedSecondary.Contains( currentCategory ))
                    continue;
                selectedSecondary.Add( currentCategory );
                break;
            }
        }

        List<Category> selectedCategories = [primaryCategory];
        selectedCategories.AddRange( selectedSecondary );
        return selectedCategories;
    }
    static Guid PickBrandId( List<Category> categories, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
    {
        HashSet<int> tried = [];
        
        for ( int i = 0; i < LoopSafety; i++ ) {
            int index = random.GetRandomInt( brands.Count - 1 );
            if (!tried.Add( index ))
                continue;

            Brand brand = brands[index];
            IEnumerable<BrandCategory> bc = brandCategories.Where( b => b.BrandId == brand.Id );
            foreach ( BrandCategory b in bc )
                if (categories.Any( c => b.CategoryId == c.Id ))
                    return brand.Id;
        }

        throw new Exception( "Failed to pick a BrandId for product during seeding: PickBrandId() in ProductSeeder()." );
    }
    static bool PickIsInStock( RandomUtility random )
    {
        bool value = random.GetRandomBool( 0.95 );
        return value;
    }
    static bool PickIsFeatured( RandomUtility random )
    {
        bool value = random.GetRandomBool( 0.2 );
        return value;
    }
    static string PickName( Category primaryCategory, int iteration )
    {
        string pName = ProductSeedData.ProductNamesByPrimaryCategory[primaryCategory.Name];
        string name = $"{pName} {iteration}";
        return name;
    }
    static string PickImage( Category primaryCategory, RandomUtility random )
    {
        return ProductSeedData.TestImage;
        /*int i = random.GetRandomInt( ProductSeedData.ImagesPerCategory - 1 );
        string image = $"{primaryCategory.Name}/{i}.png";
        return image;*/
    }
    static decimal PickPrice( RandomUtility random, out decimal price )
    {
        price = (decimal) random.GetRandomDouble( ProductSeedData.MaxPrice );
        return price;
    }
    static decimal PickSalePrice( decimal mainPrice, RandomUtility random )
    {
        bool hasSale = random.GetRandomBool( 0.2 );
        if (!hasSale)
            return decimal.Zero;

        double maxSalePrice = 0.9 * (double) mainPrice;
        double salePrice = random.GetRandomDouble( maxSalePrice );
        return (decimal) salePrice;
    }
    static int PickNumberSold( RandomUtility random )
    {
        int number = random.GetRandomInt( 0, 10000 );
        return number;
    }
    static (int, float) PickRating( int numSold, RandomUtility random )
    {
        int numRatings = random.GetRandomInt( 0, numSold );
        float rating = 0;
        for ( int i = 0; i < numRatings; i++ )
            rating += random.GetRandomInt( 1, 5 );
        rating /= numRatings;
        return (numRatings, rating);
    }
}