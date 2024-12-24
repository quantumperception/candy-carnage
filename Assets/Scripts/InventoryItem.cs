using UnityEngine;

public class IIData : MonoBehaviour
{
    public enum ItemType
    {
        Material,
        Consumable,
        Equipment,
        Unit
    }

    [Header("Base Data")]
    public string m_name;
    public string m_description;
    public Sprite m_icon;
    public ItemType m_type;
}

public class InventoryItem
{
    [Header("Base Data")]
    public int m_amount;
    public IIData m_item;
    public Vector2Int m_inventorySize = new(1, 1);
    public Inventory m_inventory;

    public InventoryItem(IIData item, int amount = 1)
    {
        m_item = item;
        m_amount = amount;
    }
    public int Take(int amount)
    {
        if (m_amount - amount < 0)
        {
            int result = m_amount;
            m_amount = 0;
            return result;
        };
        m_amount -= amount;
        return amount;
    }

    public void Add(int amount) { m_amount += amount; }

    public override int GetHashCode()
    {
        return m_item.m_name.GetHashCode() ^ m_amount.GetHashCode();
    }

    public override bool Equals(object other)
    {
        if (other is InventoryItem) return m_item.m_name == (other as InventoryItem).m_item.m_name;
        if (other is Item) return m_item.m_name == (other as Item).m_name;
        if (other is Unit) return m_item.m_name == (other as Unit).m_name;
        return false;
    }
}
