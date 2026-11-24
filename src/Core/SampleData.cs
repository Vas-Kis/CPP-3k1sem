using Core.Domain;

namespace Core;

public static class SampleData
{
    // lab07 Step 5, DoD 8 - 20 записів із різними одиницями та залишками.
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400", "мішок", 120),
        Product.Create("P-002", "SKU-002", "Пісок річковий", "т", 35),
        Product.Create("P-003", "SKU-003", "Щебінь фракція 5-20", "т", 48),
        Product.Create("P-004", "SKU-004", "Цегла керамічна", "шт", 2400),
        Product.Create("P-005", "SKU-005", "Дошка соснова", "м3", 18),
        Product.Create("P-006", "SKU-006", "Арматура 12 мм", "м", 650),
        Product.Create("P-007", "SKU-007", "Гіпсокартон", "лист", 90),
        Product.Create("P-008", "SKU-008", "Фарба біла", "відро", 24),
        Product.Create("P-009", "SKU-009", "Плитка керамічна", "м2", 75),
        Product.Create("P-010", "SKU-010", "Кабель мідний", "м", 500),
        Product.Create("P-011", "SKU-011", "Труба ПВХ", "м", 140),
        Product.Create("P-012", "SKU-012", "Утеплювач мінеральний", "пакунок", 60),
        Product.Create("P-013", "SKU-013", "Саморізи 50 мм", "коробка", 32),
        Product.Create("P-014", "SKU-014", "Ґрунтовка глибока", "каністра", 16),
        Product.Create("P-015", "SKU-015", "Профіль сталевий", "шт", 210),
        // lab07 Step 5 - додані записи: три із залишком менше 10 для report-low (SKU-016, SKU-017, SKU-020),
        // SKU-019 має такий самий залишок, як SKU-011 (140), - для перевірки другого ключа в топ-5.
        Product.Create("P-016", "SKU-016", "Цвяхи 100 мм", "коробка", 7),
        Product.Create("P-017", "SKU-017", "Двері міжкімнатні", "шт", 4),
        Product.Create("P-018", "SKU-018", "Клей для плитки", "мішок", 85),
        Product.Create("P-019", "SKU-019", "Дріт сталевий", "м", 140),
        Product.Create("P-020", "SKU-020", "Вікно металопластикове", "шт", 9)
    ];
}
