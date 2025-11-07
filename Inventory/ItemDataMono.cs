using Godot;

public enum ItemType
{
    Equipment,
    Consumable,
    Quest,
    Generic
}

[GlobalClass]
public partial class ItemDataMono : Resource
{
    [Export] public ItemType Type { get; set; }

    // ... suas outras propriedades (nome, descrição, prefab, etc.)
    [Export] public string ItemName { get; set; }
    [Export] public Texture2D Icon { get; set; }
    [Export] public PackedScene ItemModelPrefab { get; set; }
}
