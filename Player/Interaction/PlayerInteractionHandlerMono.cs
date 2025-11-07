// PlayerInteractionHandlerMono.cs (Versão Simplificada e Corrigida)
using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class PlayerInteractionHandlerMono : Area3D
{
    // Crie uma referência direta para o gerente do inventário.
    // Você vai arrastar o nó do TabContainer para cá no editor.
    [Export] public MasterInventoryManager InventoryManager { get; set; }

    private List<InteractableItemMono> NearbyBodies = new List<InteractableItemMono>();

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Interact"))
        {
            PickupNearestItem();
        }
    }

    private void PickupNearestItem()
    {
        // A lógica para encontrar o item mais próximo continua a mesma
        InteractableItemMono nearestItem = NearbyBodies.OrderBy(x => x.GlobalPosition.DistanceTo(GlobalPosition)).FirstOrDefault();

        if (nearestItem != null)
        {
            // Verificação de segurança: O item no mundo tem um "RG" (ItemData) configurado?
            if (nearestItem.ItemData == null)
            {
                GD.PrintErr($"O item '{nearestItem.Name}' foi pego, mas não tem um ItemData associado no editor!");
                return;
            }

            // Entrega o ItemData diretamente para o inventário!
            InventoryManager.PickupItem(nearestItem.ItemData);

            // Remove o item da lista e do mundo
            NearbyBodies.Remove(nearestItem);
            nearestItem.QueueFree();
        }
    }

    public void OnObjectEnteredArea(Node3D body)
    {
        if (body is InteractableItemMono item)
        {
            item.GainFocus();
            NearbyBodies.Add(item);
        }
    }

    public void OnObjectExitedArea(Node3D body)
    {
        if (body is InteractableItemMono item && NearbyBodies.Contains(item))
        {
            item.LoseFocus();
            NearbyBodies.Remove(item);
        }
    }
}