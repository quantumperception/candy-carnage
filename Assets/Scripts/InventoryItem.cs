using UnityEngine;



public class InventoryItem
{
    [Header("Base Data")]
    public int m_amount;
    public IIData m_item;
    public Vector2Int m_inventorySize = new(1, 1);
    public Inventory m_inventory;
    public int m_slot;

    public InventoryItem(IIData item, int amount = 1, Inventory inventory = null, int slot = -1)
    {
        m_inventory = inventory;
        m_item = item;
        m_slot = slot;
        m_amount = amount;
    }
    public InventoryItem(InventoryItem ii)
    {
        m_inventory = ii.m_inventory;
        m_item = ii.m_item;
        m_slot = ii.m_slot;
        m_amount = ii.m_amount;
    }

    public int TakeAll() => Take(m_amount);
    public int Take(int amount)
    {
        int result = 0;
        if (m_amount - amount < 0)
        {
            result = m_amount;
            m_amount = 0;
        }
        else
        {
            m_amount -= amount;
            result = amount;
        }
        if (BattleUI.Instance.m_draggedElement == this) BattleUI.Instance.SetDraggedElement(m_amount > 0 ? BattleUI.Instance.m_draggedElement : null);
        if (m_inventory != null) BattleUI.Instance.SetItem(m_inventory.m_inventoryGrid, m_slot, this);
        return result;
    }

    public void Add(int amount)
    {
        m_amount += amount;
        if (BattleUI.Instance.m_draggedElement == this) BattleUI.Instance.SetDraggedElement(BattleUI.Instance.m_draggedElement);
        if (m_inventory != null) BattleUI.Instance.SetItem(m_inventory.m_inventoryGrid, m_slot, this);
    }

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
