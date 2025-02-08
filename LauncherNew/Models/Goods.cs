namespace LauncherNew.Models;

public class Goods
{
    public int Id { get; set; } // Уникальный идентификатор
    public string Name { get; set; } // Название товара
    public decimal Price { get; set; } // Цена товара
    public byte[]? Photo { get; set; } // Фото (опционально)
    public string Category { get; set; }    
}