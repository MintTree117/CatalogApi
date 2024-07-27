using System.Data;
using System.Text;
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

        foreach ( Category primaryCategory in primaryCategories ) 
            for ( int i = 0; i < ProductsPerPrimaryCategory; i++ )
            {
                Guid productId = Guid.NewGuid();
                List<Category> sc = PickRandomCategories( primaryCategory, secondaryCategories, random );
                Brand brand = PickBrand( sc, brands, brandCategories, random );
                int numSold = PickNumberSold( random );
                (int,float) rating = PickRating( numSold, random );
                (ProductXml, XmlElement) px = GenerateProductXml( productId, primaryCategory, random );
                DateTime? saleEndDate = PickSaleDate( random );
                decimal weight = PickWeight( random );
                string dimensions = "L x W x H";
                decimal? shippingPrice = PickShippingPrice( weight, random );
                Product p = new(
                    productId,
                    brand.Id,
                    PickName( brand, primaryCategory, i, px.Item2 ),
                    brand.Name,
                    PickImage( random ),
                    PickIsFeatured( random ),
                    PickIsInStock( random ),
                    PickPrice( random, out decimal price ),
                    saleEndDate is null ? null : PickSalePrice( price, random ),
                    shippingPrice,
                    saleEndDate,
                    PickReleaseDate( random ),
                    rating.Item2,
                    rating.Item1,
                    numSold,
                    weight,
                    dimensions );
                List<ProductCategory> pc = 
                    GenerateProductCategories( p, sc );
                ProductDescription pd =
                    GenerateProductDescription( p, random );
                
                products.Add( p );
                productCategories.AddRange( pc );
                productDescriptions.Add( pd );
                productXmls.Add( px.Item1 );
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
        table.Columns.Add( nameof( Product.BrandId ), typeof( Guid ) );
        table.Columns.Add( nameof( Product.Name ), typeof( string ) );
        table.Columns.Add( nameof( Product.BrandName ), typeof( string ) );
        table.Columns.Add( nameof( Product.Image ), typeof( string ) );
        table.Columns.Add( nameof( Product.IsFeatured ), typeof( bool ) );
        table.Columns.Add( nameof( Product.IsInStock ), typeof( bool ) );
        table.Columns.Add( nameof( Product.Price ), typeof( decimal ) );
        table.Columns.Add( nameof( Product.SalePrice ), typeof( decimal ) );
        table.Columns.Add( nameof( Product.ShippingPrice ), typeof( decimal ) );
        table.Columns.Add( nameof( Product.SaleEndDate ), typeof( DateTime ) );
        table.Columns.Add( nameof( Product.ReleaseDate ), typeof( DateTime ) );
        table.Columns.Add( nameof( Product.Rating ), typeof( float ) );
        table.Columns.Add( nameof( Product.NumberRatings ), typeof( int ) );
        table.Columns.Add( nameof( Product.NumberSold ), typeof( int ) );
        table.Columns.Add( nameof( Product.Weight ), typeof( decimal ) );
        table.Columns.Add( nameof( Product.Dimensions ), typeof( string ) );
        
        foreach ( Product p in products ) {
            DataRow row = table.NewRow();
            row[nameof( Product.Id )] = p.Id;
            row[nameof( Product.BrandId )] = p.BrandId;
            row[nameof( Product.Name )] = p.Name;
            row[nameof( Product.BrandName )] = p.BrandName;
            row[nameof( Product.Image )] = p.Image;
            row[nameof( Product.IsFeatured )] = p.IsFeatured;
            row[nameof( Product.IsInStock )] = p.IsInStock;
            row[nameof( Product.Price )] = p.Price;
            row[nameof( Product.SalePrice )] = p.SalePrice.HasValue ? p.SalePrice.Value : DBNull.Value;
            row[nameof( Product.ShippingPrice )] = p.ShippingPrice.HasValue ? p.ShippingPrice.Value : DBNull.Value;
            row[nameof( Product.SaleEndDate )] = p.SaleEndDate.HasValue ? p.SaleEndDate.Value : DBNull.Value;
            row[nameof( Product.ReleaseDate )] = p.ReleaseDate;
            row[nameof( Product.Rating )] = p.Rating;
            row[nameof( Product.NumberRatings )] = p.NumberRatings;
            row[nameof( Product.NumberSold )] = p.NumberSold;
            row[nameof( Product.Weight )] = p.Weight;
            row[nameof( Product.Dimensions )] = p.Dimensions;
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
    static (ProductXml, XmlElement) GenerateProductXml( Guid productId, Category category, RandomUtility random )
    {
        // FORMAT: XML_DOCUMENT -> ROOT_ELEMENT -> NODES[]
        // NODE = KVP (NAME, VALUES (",")
        
        // INITIALIZATION
        Dictionary<string, string[]> specs = ProductXmlSeedData.ProductSpecsByCategory[category.Name];
        List<string> keys = specs.Keys.ToList();
        HashSet<int> specUsed = [];
        int numSpecs = random.GetRandomInt( keys.Count - 1 );
        
        // SELECT SPECS
        Dictionary<string, List<string>> selectedSpecsAndValues = [];
        for ( int i = 0; i <= numSpecs; i++ ) {
            for ( int j = 0; j < LoopSafety; j++ ) {
                
                // TRY GET NEW
                int specIndex = random.GetRandomInt( keys.Count - 1 );
                if (!specUsed.Add( specIndex ))
                    continue;
                
                // INIT NEW SPEC
                string specName = keys[specIndex];
                string[] values = specs[keys[specIndex]];
                int selectedValueCount = random.GetRandomInt( 1, Math.Max( values.Length - 1, 1 ) );
                
                // MULTIPLE CHOICE TYPE
                List<string> selectedValues = [];
                HashSet<int> indexUsed = [];
                for ( int k = 0; k < selectedValueCount; k++ ) {
                    for ( int l = 0; l <= LoopSafety; l++ ) {
                        int valueIndex = random.GetRandomInt( values.Length - 1 );
                        if (!indexUsed.Add( valueIndex ))
                            continue;
                        selectedValues.Add( values[valueIndex] );
                    }
                }
                selectedSpecsAndValues.Add( specName, selectedValues );
            }
        }
        
        // GENERATE XML
        XmlDocument xmlDoc = new();
        XmlElement root = xmlDoc.CreateElement( "ProductSpecs" );
        xmlDoc.AppendChild( root );

        foreach ( KeyValuePair<string, List<string>> kvp in selectedSpecsAndValues ) {
            string xmlName = kvp.Key.Replace( ' ', '-' );
            XmlElement specElement = xmlDoc.CreateElement( xmlName );
            string xmlValue = string.Join( ",", kvp.Value );
            XmlElement valueElement = xmlDoc.CreateElement( "Value" );
            valueElement.InnerText = xmlValue;
            specElement.AppendChild( valueElement );
            root.AppendChild( specElement );
        }
        return (new ProductXml( productId, xmlDoc.InnerXml ), root);
    }

    static List<Category> PickRandomCategories( Category primaryCategory, Dictionary<Guid, List<Category>> secondaryCategories, RandomUtility random )
    {
        List<Category> selectedSecondary = [];
        int numSecondary = random.GetRandomInt( 3, 6 );
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
    static Brand PickBrand( List<Category> categories, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
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
                    return brand;
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
    static string PickName( Brand brand, Category primaryCategory, int iteration, XmlElement root )
    {
        const int NameCharacterLength = 256;
        StringBuilder builder = new();
        builder.Append( $"{brand.Name} {ProductSeedData.ProductNamesByPrimaryCategory[primaryCategory.Name]} {iteration}" );
        
        XmlNodeList elements = root.ChildNodes;
        foreach ( XmlNode node in elements )
        {
            if (node.NodeType != XmlNodeType.Element) 
                continue;

            XmlElement element = (XmlElement) node;
            var split = element.InnerText
                .Trim()
                .Split( "," );
            
            const int MaxPer = 3;
            int count = 0;
            foreach ( string s in split )
            {
                if (count >= MaxPer)
                    break;
                if (string.IsNullOrWhiteSpace( s ))
                    continue;
                if (builder.Length + s.Length + 1 >= NameCharacterLength)
                    break;
                string trimmed = s.Trim();
                builder.Append( trimmed );
                builder.Append( " " );
                count++;
            }
        }

        string result = builder.ToString();
        //result = result.Trim();
        if (result.Length >= NameCharacterLength)
            result = result.Substring( 0, NameCharacterLength );
        return result;
    }
    static string PickImage( RandomUtility random )
    {
        int i = random.GetRandomInt( ProductSeedData.NumberOfProductImages - 1 );
        string image = $"/images/p{i}.jpg";
        return image;
    }
    static decimal PickPrice( RandomUtility random, out decimal price )
    {
        price = (decimal) random.GetRandomDouble( ProductSeedData.MaxPrice );
        return price;
    }
    static decimal PickSalePrice( decimal mainPrice, RandomUtility random )
    {
        double maxSalePrice = 0.9 * (double) mainPrice;
        double salePrice = random.GetRandomDouble( maxSalePrice );
        return (decimal) salePrice;
    }
    static DateTime? PickSaleDate( RandomUtility random )
    {
        bool hasSale = random.GetRandomBool( 0.2 );
        if (!hasSale)
            return null;

        int days = random.GetRandomInt( 1, 30 );
        DateTime saleEndDate = DateTime.Now + TimeSpan.FromDays( days );
        return saleEndDate;
    }
    static DateTime PickReleaseDate( RandomUtility random )
    {
        int months = random.GetRandomInt( 1, 120 );
        int days = months * 30;
        DateTime releaseDate = DateTime.Now - TimeSpan.FromDays( days );
        return releaseDate;
    }
    static int PickNumberSold( RandomUtility random )
    {
        int number = random.GetRandomInt( 0, 10000 );
        return number;
    }
    static (int, float) PickRating( int numSold, RandomUtility random )
    {
        int numRatings = random.GetRandomInt( 0, numSold );
        float rating = (float) random.GetRandomDouble( 1, 5 );
        return (numRatings, rating);
    }
    static decimal PickWeight( RandomUtility random )
    {
        var weight = random.GetRandomDouble( 1, 1000 );
        return (decimal) weight;
    }
    static decimal? PickShippingPrice( decimal weight, RandomUtility random )
    {
        double? cost;

        if (weight > 5)
        {
            cost = random.GetRandomInt( 1, 100 ) > 70
                ? 9.99
                : null;
            return (decimal?) cost;
        }
        if (weight > 10)
        {
            cost = random.GetRandomInt( 1, 100 ) > 60
                ? 12.99
                : null;
            return (decimal?) cost;
        }
        if (weight > 20)
        {
            cost = random.GetRandomInt( 1, 100 ) > 50
                ? 15.99
                : null;
            return (decimal?) cost;
        }
        if (weight > 30)
        {
            cost = random.GetRandomInt( 1, 100 ) > 40
                ? 19.99
                : null;
            return (decimal?) cost;
        }

        cost = random.GetRandomInt( 1, 100 ) > 20
            ? 29.99
            : null;
        return (decimal?) cost;
    }
}