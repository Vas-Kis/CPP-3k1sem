namespace Core.Reports;

// lab07 Step 1 - DTO рядка звіту: незмінний record, який порівнюється за значенням (зручно для тестів).
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);
