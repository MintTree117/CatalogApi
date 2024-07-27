namespace CatalogApplication.Seeding.SeedData;

internal static class BrandSeedData
{
    internal static readonly Dictionary<string, string[]> BrandsPerCategory = new() {
        // Computers & Laptops
        {
            CategorySeedData.PrimaryCategories[0], [
                "Dell", "HP", "Lenovo", "Apple", "Asus", "Acer", "Microsoft",
                "Samsung", "Amazon", "LG", "BenQ", "Logitech", "Razer", "Corsair",
                "Anker", "Alienware", "MSI", "HP", "Sony PlayStation", "Microsoft Xbox",
                "Electronic Arts", "Activision", "Ubisoft", "Nintendo",
                "Sony", "SteelSeries", "HyperX", "Astro Gaming"
            ]
        },
        // Smartphones & Tablets
        {
            CategorySeedData.PrimaryCategories[1], [
                "Apple", "Samsung", "Google", "OnePlus", "Huawei", "Xiaomi", "Sony",
                "LG", "Motorola", "HTC", "Nokia", "ASUS", "OPPO", "Vivo", "Amazon",
                "Microsoft", "Lenovo", "Fitbit", "Garmin", "Fossil", "TicWatch",
                "OtterBox", "Spigen", "Anker", "Belkin", "PopSockets", "JBL",
                "RAVPower", "Mophie", "Aukey"
            ]
        },
        // TVs & Home Entertainment
        {
            CategorySeedData.PrimaryCategories[2], [
                "Samsung", "LG", "Sony", "TCL", "Vizio", "Hisense", "Panasonic", "Philips", "Sharp",
                "Insignia", "Toshiba", "Sceptre", "Element", "RCA", "Bose", "Yamaha", "Onkyo", "Apple",
                "Roku", "Amazon", "Google", "Nvidia", "Xiaomi", "Western Digital", "Ematic", "Epson",
                "BenQ", "Optoma", "ViewSonic"
            ]
        },
        // Cameras & Photography
        {
            CategorySeedData.PrimaryCategories[3], [
                "Canon", "Nikon", "Sony", "Fujifilm", "Panasonic", "Olympus", "Leica", "Samsung", "Ricoh",
                "Pentax", "Kodak", "GoPro", "DJI", "YI Technology", "Insta360", "Akaso", "Campark",
                "Sigma", "Tamron", "Rokinon", "Zeiss", "Manfrotto", "Lowepro", "Peak Design", "Joby",
                "Think Tank", "Vanguard", "Parrot", "Autel Robotics"
            ]
        },
        // Audio & Headphones
        {
            CategorySeedData.PrimaryCategories[4], [
                "Sony", "Bose", "Sennheiser", "Beats by Dre", "Audio-Technica", "JBL", "AKG", "Beyerdynamic",
                "Skullcandy", "Jabra", "Plantronics", "Shure", "Marshall", "Anker", "Samsung", "Apple",
                "Sonos", "Harman Kardon", "UE (Ultimate Ears)", "LG", "Polk Audio", "Klipsch",
                "Amazon", "Google", "Creative", "Yamaha", "Denon", "Onkyo", "SanDisk"
            ]
        },
        // Wearable Technology
        {
            CategorySeedData.PrimaryCategories[5], [
                "Apple", "Samsung", "Fitbit", "Garmin", "Huawei", "Fossil",
                "TicWatch", "Amazfit", "Sony", "Xiaomi", "Polar", "Withings",
                "Misfit", "Letsfit", "YAMAY", "Scosche Rhythm+", "Oculus", 
                "HTC", "Valve", "Google", "Pimax", "HP", "Lenovo", "Acer", "Anker", "Spigen"
            ]
        },
        // Cables & Adapters
        {
            CategorySeedData.PrimaryCategories[6], [
                "Amazon", "Anker", "Belkin", "UGREEN", "Cable Matters", "Syncwire", "iVanky",
                "Monoprice", "JSAUX", "AUKEY", "Sabrent", "Rankie", "TUDIA", "StarTech", "Native Union", 
                "MFi Certified", "RND", "CableCreation", "Ventev", "Spigen", "PowerBear", "Twisted Veins",
                "BlueRigger", "Capshi", "SecurOMax", "Mediabridge", "Fosmon", "TNP Products", 
                "KabelDirekt", "DTECH", "Xiaomi"
            ]
        },
        // Storage Devices
        {
            CategorySeedData.PrimaryCategories[7], [
                "SanDisk", "Samsung", "Kingston", "ADATA", "PNY", "Sony", "Transcend", "HP", "Corsair",
                "Lexar", "Toshiba", "Verbatim", "LaCie", "Silicon Power", "WD (Western Digital)",
                "Seagate", "Hitachi", "G-Technology", "Buffalo", "Maxtor",
                "Fantom Drives", "QNAP", "Synology", "Drobo", "Crucial", "Intel", "SK Hynix",
                "Gigabyte", "Team Group"
            ]
        },
        // Home Appliances
        {
            CategorySeedData.PrimaryCategories[8], [
                "LG", "Samsung", "Whirlpool", "GE Appliances", "Maytag", "Kenmore", "Bosch",
                "Electrolux", "Haier", "Amana", "Frigidaire", "Miele", "Siemens", "Panasonic", 
                "Dyson", "Shark", "iRobot Roomba", "Bissell", "Hoover", "Eufy", "Neato Robotics", 
                "Ecovacs", "Black+Decker", "Tineco", "Roborock", "Dirt Devil", "Hamilton Beach",
                "Cuisinart", "KitchenAid", "Farberware"
            ]
        },
        // Office Electronics
        {
            CategorySeedData.PrimaryCategories[9], [
                "HP", "Epson", "Canon", "Brother", "Samsung", "Xerox", "Lexmark", "Dell", "Ricoh", "Kyocera",
                "Panasonic", "Fujitsu", "BenQ", "Optoma", "ViewSonic", "Logitech", "Microsoft", "3M", "Fellowes",
                "AmazonBasics", "Belkin", "Acer", "ASUS", "SanDisk", "Lexar", "Texas Instruments", "Casio", "Sharp",
                "Victor"
            ]
        }
    };
}