namespace Core.Dto;

public sealed record StockBatchDto(
    string Id,
    string ProductId,
    string BatchNumber,
    int Quantity);