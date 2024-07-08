namespace CatalogApplication.Types.Orders;

internal readonly record struct CatalogOrderDto(
    int PosX,
    int PosY,
    List<CartItemDto> Items );