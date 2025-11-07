using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class InventorySlotMono : Control
{

    [Signal] public delegate void OnItemEquipedEventHandler(int SlotID);
    [Signal] public delegate void OnItemDroppedEventHandler(int fromSlotID, int toSlotID);

    [Export] public Panel EquippedHighlight { get; set; }
    [Export] public TextureRect IconSlot { get; set; }

    public int InventorySlotID { get; set; } = -1;
    public ItemType AllowedItemType { get; set; }
	public bool SlotFilled { get; set; } = false;

	public ItemDataMono SlotData { get; set; }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick)
            {
                EmitSignal(SignalName.OnItemEquiped, InventorySlotID);
            }
        }
    }

    public bool CanAcceptItem(ItemDataMono item)
    {
        if (item == null) return true; // Sempre pode aceitar um slot vazio
        return item.Type == AllowedItemType || AllowedItemType == ItemType.Generic;
    }

    public void FillSlot(ItemDataMono data, bool equipped)
    {
        SlotData = data;
        EquippedHighlight.Visible = equipped;
        if (SlotData != null)
        {
            SlotFilled = true;
            IconSlot.Texture = data.Icon;
        }
        else
        {
            SlotFilled = false;
            IconSlot.Texture = null;
        }
    }

    //public override Variant _GetDragData(Vector2 atPosition)
    //{
    //    if (SlotFilled)
    //    {
    //        TextureRect preview = new TextureRect();
    //        preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
    //        preview.Size = IconSlot.Size;
    //        preview.PivotOffset = IconSlot.Size / 2.0f;
    //        preview.Rotation = 2.0f;
    //        preview.Texture = IconSlot.Texture;
    //        SetDragPreview(preview);
    //        return new Dictionary { { "Type", "Item" }, { "ID", InventorySlotID} };
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    //public override bool _CanDropData(Vector2 atPosition, Variant data)
    //{
    //    return data.VariantType == Variant.Type.Dictionary && (string)data.AsGodotDictionary()["Type"] == "Item";
    //}

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (data.VariantType == Variant.Type.Dictionary)
        {
            var dict = data.AsGodotDictionary();
            // Precisamos recriar o ItemData a partir dos dados do drop para verificar o tipo.
            // Isso é um pouco avançado. Uma forma mais simples é passar o tipo no dicionário.
            // Vamos modificar _GetDragData para incluir o tipo.
            if (dict.ContainsKey("ItemType"))
            {
                ItemType droppedItemType = (ItemType)dict["ItemType"].AsInt32();
                return droppedItemType == AllowedItemType || AllowedItemType == ItemType.Generic;
            }
        }
        return false;
    }
    
    // Modifique _GetDragData para incluir o tipo
    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (SlotFilled)
        {
            TextureRect preview = new TextureRect();
            preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            preview.Size = IconSlot.Size;
            preview.PivotOffset = IconSlot.Size / 2.0f;
            preview.Rotation = 2.0f;
            preview.Texture = IconSlot.Texture;
            SetDragPreview(preview);
            //return new Dictionary { { "Type", "Item" }, { "ID", InventorySlotID} };
            
            var data = new Godot.Collections.Dictionary
            {
                { "Type", "Item" }, // "Type" genérico para o GroundDropArea
                { "ID", InventorySlotID },
                { "ItemType", (int)SlotData.Type } // TIPO ESPECÍFICO para outros slots
            };
            return data;
        }
        return new Variant();
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        EmitSignal(SignalName.OnItemDropped, (int)data.AsGodotDictionary()["ID"], InventorySlotID);
    }
}
