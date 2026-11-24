namespace Core.Reports;

// lab07 Step 1 - DTO групи: ключ, кількість позицій у групі та сумарний залишок.
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);
