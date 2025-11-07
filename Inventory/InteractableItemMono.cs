// InteractableItemMono.cs
using Godot;

public partial class InteractableItemMono : RigidBody3D
{
    // A PONTE DE LIGAÇÃO:
    // Agora, cada item no mundo terá um "RG" associado a ele.
    [Export] public ItemDataMono ItemData { get; private set; }
    
    [Export] public MeshInstance3D ItemHighlightMesh { get; set; }

    // Função a ser chamada pelo player quando ele interagir com este objeto.
    public void Interact(MasterInventoryManager inventoryManager)
    {
        // Verificação de segurança: o item foi configurado no editor?
        if (ItemData == null)
        {
            GD.PrintErr($"O item '{this.Name}' no mundo não tem um ItemData associado!");
            return;
        }
        
        // Manda o "RG" do item para o gerente do inventário.
        inventoryManager.PickupItem(ItemData);
        
        // O item foi coletado, então ele deve desaparecer do mundo.
        QueueFree();
    }

    public void GainFocus()
    {
        if (ItemHighlightMesh != null)
            ItemHighlightMesh.Visible = true;
    }

    public void LoseFocus()
    {
        if (ItemHighlightMesh != null)
            ItemHighlightMesh.Visible = false;
    }
}