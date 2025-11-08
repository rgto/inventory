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

        var dict = data.AsGodotDictionary();
        int slotId = dict["ID"].As<int>();

        var foundSlot = FindSlotBySlotID(slotId, InventoryHandler);
        if (foundSlot == null)
        {
            GD.PrintErr($"[Drop] Nenhum slot com ID {slotId} encontrado!");
            return;
        }

        if (foundSlot.SlotData == null)
        {
            GD.PrintErr($"[Drop] Slot {slotId} está vazio (sem dados de item)!");
            return;
        }

        var itemPrefab = foundSlot.SlotData.ItemModelPrefab;

        if (itemPrefab == null)
        {
            GD.PrintErr($"[Drop] ItemModelPrefab nulo para {foundSlot.SlotData.ItemName}");
            return;
        }


        // Remove o item do slot.
        foundSlot.FillSlot(null, false);

        // Verifica se é um PackedScene vazio (subrecurso inválido)
        if (!itemPrefab.CanInstantiate())
        {
            // Extrai o caminho real do arquivo principal .tscn
            string basePath = itemPrefab.ResourcePath;

            if (basePath.Contains("::"))
            {
                basePath = basePath.Split("::")[0];
            }

            GD.Print($"[Fallback] Recarregando cena real de '{basePath}'");

            if (ResourceLoader.Exists(basePath))
            {
                itemPrefab = GD.Load<PackedScene>(basePath);
            }
        }

        if (itemPrefab == null || !itemPrefab.CanInstantiate())
        {
            GD.PrintErr($"[Drop] Falha ao carregar prefab real de '{foundSlot.SlotData.ItemName}'!");
            return;
        }

        
        Node3D newItem = itemPrefab.Instantiate<Node3D>();
        PlayerBody.GetParent().AddChild(newItem);
        newItem.GlobalPosition = InventoryHandler.GetWorldMousePosition();
        GD.Print($"[Drop] '{foundSlot.SlotData.ItemName}' instanciado com sucesso!");
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
