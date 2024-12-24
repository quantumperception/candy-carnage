using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : IIData
{
    public int m_team;

    private void Start()
    {
        GetComponent<ParticleSystemRenderer>().material.color = Team.colors[m_team];
    }

    private void OnMouseEnter()
    {
        GetTargetInventory().AddItem(this);
        Destroy(gameObject);
    }

    Inventory GetTargetInventory()
    {
        Inventory[] inventories = FindObjectsByType<Inventory>(FindObjectsSortMode.None);
        if (m_type == ItemType.Material) return inventories.First(i => i.m_type == InventoryType.Materials);
        if (m_type == ItemType.Consumable || m_type == ItemType.Equipment) return inventories.First(i => i.m_type == InventoryType.Items);
        return null;
    }

    public override string ToString() => m_name;
    
    public override int GetHashCode() => m_name.GetHashCode();

    public override bool Equals(object other)
    {
        if (other is InventoryItem) return m_name == (other as InventoryItem).m_item.m_name;
        if (other is Item) return m_name == (other as Item).m_name;
        return false;
    }

}
