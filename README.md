# Catalog API

This api interacts with 2 other repositories, the [ShoppingApp](https://github.com/MintTree117/ShoppingApp.git) and [OrderingApi](https://github.com/MintTree117/OrderingApi.git), to provide high-performance, read-only access to product and related information.

See the fully deployed application [here](https://happy-bush-0b0f3e80f.5.azurestaticapps.net/)

## Features

### Technologies Used

- .NET 8
- Web API
- Dapper
- SQL Server (hosted on Azure)

### Architecture

- Minimal Endpoints
- Repository Pattern
- Comprehensive Seeding Service on Application Startup
  - Seeds Products, Categories, Brands, and Inventory

### Functionality

- **High-Performance Read-Only API:** Designed for speed and efficiency.
- **Data Persistence:** Utilizes Dapper for data access and SQL Server for data storage.
- **Inventory Management:** Periodically pings all warehouses to check product counts, maintaining up-to-date inventory data.
- **Product Details:** Products hold comprehensive details generated during seeding, including full XML specifications, ratings, number sold, etc.
- **Product Search:** Comprehensive search capabilities, dynamically constructed queries that avoid unnecessary joins, recursions, and duplicate actions.
  - Finds actual product inventory data based on user location (limited to an x,y grid position for simplicity).
- **Shipping Estimates:** Allows for shipping estimates to be returned without re-doing the whole product search, decoupled from product search.
- **In-Memory Caching:** Repositories use an in-memory caching system with timers to prune stale data, balancing speed and accuracy.
- **Error Handling:** Robust error handling with both low-level and global exception catching.
- **Reply Pattern:** Uses a semi-functional immutable data type for return values, handling nullables gracefully and allowing error messages to bubble up.
- **Memory Optimization:** Reduces memory usage by using singleton repositories and structs wherever possible.

### Endpoints

- **CheckOrderItems:** Validates order items against inventory for the ordering API.
- **GetCategories:** Retrieves product categories.
- **GetBrands:** Retrieves product brands.
- **GetSpecials:** Provides front-page products for the shopping app.
- **SearchSuggestions:** Returns product suggestions as the user types in the client search bar.
- **SearchIds:** Used for cart and checkout processes.
- **SearchSimilar:** Retrieves similar products to the current one.
- **SearchFull:** Performs a full product search.
- **GetEstimates:** Returns shipping estimates without re-doing the whole product search.
- **GetDetails:** Retrieves all product details.
