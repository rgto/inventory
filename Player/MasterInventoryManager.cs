// MasterInventoryManager.cs
using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class MasterInventoryManager : Control
{
    [Export] public CharacterBody3D PlayerBody { get; set; }
    [Export(PropertyHint.Layers3DPhysics)] public uint CollisionMask { get; set; }
    [Export] public PackedScene InventorySlotPrefab { get; set; }

    [ExportGroup("Inventory Grids")]
    [Export] public GridContainer EquipmentGrid { get; set; }
    [Export] public GridContainer ConsumableGrid { get; set; }
    [Export] public GridContainer QuestGrid { get; set; }

    [ExportGroup("Inventory Sizes")]
    [Export] public int EquipmentSlotsCount { get; set; } = 8;
    [Export] public int ConsumableSlotsCount { get; set; } = 16;
    [Export] public int QuestSlotsCount { get; set; } = 10;

    int EquippedSlot = -1;

    // Uma única lista para todos os slots. Facilita o acesso pelo ID.
    public List<InventorySlotMono> AllSlots = new List<InventorySlotMono>();
    private int _slotIdCounter = 0;

    public override void _Ready()
    {
        // Cria os slots para cada tipo de inventário
        PopulateInventory(EquipmentGrid, EquipmentSlotsCount, ItemType.Equipment);
        PopulateInventory(ConsumableGrid, ConsumableSlotsCount, ItemType.Consumable);
        PopulateInventory(QuestGrid, QuestSlotsCount, ItemType.Quest);
    }

    private void PopulateInventory(GridContainer grid, int slotCount, ItemType allowedType)
    {
        if (grid == null) return;

        for (int i = 0; i < slotCount; i++)
        {
            var slot = InventorySlotPrefab.Instantiate<InventorySlotMono>();
            grid.AddChild(slot);

            slot.InventorySlotID = _slotIdCounter++; // ID único global
            slot.AllowedItemType = allowedType; // Diz ao slot que tipo de item ele aceita

            slot.OnItemDropped += ItemDroppedOnSlot;
            slot.OnItemEquiped += ItemEquipped;

            AllSlots.Add(slot);
        }
    }

    // Lógica de pegar item agora é inteligente
    public void PickupItem(ItemDataMono item)
    {
        bool foundSlot = false;
        // Procura um slot vazio do tipo correto
        foreach (var slot in AllSlots)
        {
            if (!slot.SlotFilled && (slot.AllowedItemType == item.Type || slot.AllowedItemType == ItemType.Generic))
            {
                slot.FillSlot(item, false);
                foundSlot = true;
                break;
            }
        }

        if (!foundSlot)
        {
            var newItem = item.ItemModelPrefab.Instantiate() as Node3D;

            PlayerBody.GetParent().AddChild(newItem);

            newItem.GlobalPosition = PlayerBody.GlobalPosition + PlayerBody.GlobalTransform.Basis.X * 2.0f;
        }
    }

    public void ItemEquipped(int slotID)
	{
        if (EquippedSlot != -1)
        {
            AllSlots[EquippedSlot].FillSlot(AllSlots[EquippedSlot].SlotData, false);
        }

        if (slotID != EquippedSlot && AllSlots[slotID].SlotData != null)
        {
            AllSlots[slotID].FillSlot(AllSlots[slotID].SlotData, true);
            EquippedSlot = slotID;
        }
        else
        {
            EquippedSlot = -1;
        }
    }


    public void ItemDroppedOnSlot(int fromSlotID, int toSlotID)
    {
        if (EquippedSlot != -1)
        {
            if (EquippedSlot == fromSlotID)
            {
                EquippedSlot = toSlotID;
            }
            else if (EquippedSlot == toSlotID)
            {
                EquippedSlot = fromSlotID;
            }
        }

        var toSlotItem = AllSlots[toSlotID].SlotData;
        var fromSlotItem = AllSlots[fromSlotID].SlotData;

        AllSlots[toSlotID].FillSlot(fromSlotItem, EquippedSlot == toSlotID);
        AllSlots[fromSlotID].FillSlot(toSlotItem, EquippedSlot == fromSlotID);
    }


   	public Vector3 GetWorldMousePosition()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();
		Camera3D cam = GetViewport().GetCamera3D();
        Vector3 ray_start = cam.ProjectRayOrigin(mousePos);
		Vector3 ray_end = ray_start + cam.ProjectRayNormal(mousePos) * cam.GlobalPosition.DistanceTo(PlayerBody.GlobalPosition) * 2.0f;


		World3D world3d = PlayerBody.GetWorld3D();
		PhysicsDirectSpaceState3D space_state = world3d.DirectSpaceState;

		var query = PhysicsRayQueryParameters3D.Create(ray_start, ray_end, CollisionMask);

		var results = space_state.IntersectRay(query);
		if (results.Count > 0) {
			return (Vector3)results["position"] + new Vector3(0.0f, 0.5f, 0.0f);
		}
		else {
			return ray_start.Lerp(ray_end, 0.5f) + new Vector3(0.0f, 0.5f, 0.0f);
		}
    }
}
