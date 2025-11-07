// GroundDropArea.cs
using Godot;

public partial class GroundDropArea : ColorRect
{
    [Export] public CharacterBody3D PlayerBody;
    [Export] public MasterInventoryManager InventoryHandler;

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        // Esta lógica está segura e deve funcionar.
        if (data.VariantType == Variant.Type.Dictionary)
        {
            var dict = data.AsGodotDictionary();
            if (dict.ContainsKey("Type") && dict["Type"].AsString() == "Item")
            {
                return true;
            }
        }
        return false;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (InventoryHandler == null || PlayerBody == null)
        {
            GD.PrintErr("GroundDropArea: PlayerBody ou InventoryHandler não foram definidos no Inspetor!");
            return;
        }

        GD.Print("Item dropped on the ground!");
        var dict = data.AsGodotDictionary();
        if (!dict.ContainsKey("ID"))
        {
            GD.PrintErr("DropData não contém a chave 'ID'!");
            return;
        }

        int slotId = dict["ID"].As<int>();

        // Procura o slot com esse ID dentro do InventoryHandler
        var foundSlot = FindSlotBySlotID(slotId, InventoryHandler);
        if (foundSlot == null)
        {
            GD.PrintErr($"Nenhum slot com ID {slotId} encontrado!");
            return;
        }

        if (foundSlot.SlotData == null)
        {
            GD.PrintErr($"Slot {slotId} não contém nenhum item!");
            return;
        }

        //var itemPrefab = foundSlot.SlotData.ItemModelPrefab;
        var itemPrefabPath = $"res://Inventory/{foundSlot.SlotData.ItemName}.tscn";

        if (itemPrefabPath == null)
        {
            GD.PrintErr($"ItemModelPrefab não definido para o item '{foundSlot.SlotData.ItemName}'!");
            return;
        }

        // Remove o item do slot.
        foundSlot.FillSlot(null, false);

        if (ResourceLoader.Exists(itemPrefabPath))
        {
            var itemPrefab = GD.Load<PackedScene>(itemPrefabPath);
            GD.Print($"[Fallback] Usando prefab carregado de '{itemPrefabPath}'");

            // Instancia o item na cena
            var newItem = itemPrefab.Instantiate() as Node3D;
            PlayerBody.GetParent().AddChild(newItem);
            newItem.GlobalPosition = InventoryHandler.GetWorldMousePosition();

            GD.Print($"Item '{foundSlot.SlotData.ItemName}' dropado com sucesso!");
        }
       
    }

    private InventorySlotMono FindSlotBySlotID(int slotId, MasterInventoryManager inventoryHandler)
    {
        foreach (var slot in inventoryHandler.AllSlots)
        {
            if (slot.InventorySlotID == slotId)
                return slot;
        }

        return null;
    }
}
