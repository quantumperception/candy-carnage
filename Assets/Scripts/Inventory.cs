using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum InventoryType
{
    Items,
    Units,
    Materials,
    Crafting
}


public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public struct DefaultItem
    {
        public IIData Item;
        public int Amount;
    }
    public Vector2Int m_size;
    public Vector2Int m_slotSize;
    public InventoryType m_type = InventoryType.Items;
    public InventoryItem[] m_items;
    public List<DefaultItem> m_defaultItems;

    private VisualElement m_root;
    public VisualElement m_inventoryGrid;

    private static Label m_itemDetailHeader;
    private static Label m_itemDetailBody;
    private static Label m_itemDetailPrice;
    private bool m_ready;

    private void Start()
    {
        Configure();
    }

    async void Configure()
    {
        await UniTask.WaitUntil(() => BattleUI.Instance.m_ready);
        SetupUI();
        VisualElement[] slots = m_inventoryGrid.Children().ToArray();
        for (int i = 0; i < slots.Length; i++)
        {
            Button btn = slots[i].Q<Button>();
            int slot = i;
            btn.RegisterCallback<MouseUpEvent>((evt) => RunSlotHandler(evt, slot), TrickleDown.TrickleDown);
            Debug.Log($"Added {m_type} handler for slot {i}");
        }
        m_items = new InventoryItem[slots.Length];
        for (int i = 0; i < m_defaultItems.Count; i++) AddItem(i, m_defaultItems[i].Item, m_defaultItems[i].Amount);
        m_ready = true;
    }


    void SetupUI()
    {
        if (m_type == InventoryType.Items) m_inventoryGrid = BattleUI.Instance.itemInventory;
        if (m_type == InventoryType.Units) m_inventoryGrid = BattleUI.Instance.unitInventory;
        if (m_type == InventoryType.Materials) m_inventoryGrid = BattleUI.Instance.materialInventory;
        if (m_type == InventoryType.Crafting) m_inventoryGrid = BattleUI.Instance.craftingInventory;

    }

    void RunSlotHandler(MouseUpEvent evt, int slot)
    {
        if (evt.button == 2) return;
        if (m_type == InventoryType.Items) { ItemSlotHandler(evt, slot); return; }
        if (m_type == InventoryType.Units) { UnitSlotHandler(evt, slot); return; }
        if (m_type == InventoryType.Materials) { MaterialSlotHandler(evt, slot); return; }
        if (m_type == InventoryType.Crafting) { CraftingSlotHandler(evt, slot); return; }
    }

    InventoryItem OnItemClick(int slot, bool takeAll)
    {
        InventoryItem ii = GetSlotItem(slot);
        //Debug.Log("Item: " + ii.m_item.m_name);
        if (ii == null) return null;
        InventoryItem newIi = TakeItem(slot, ii, takeAll ? ii.m_amount : 1);
        if (BattleUI.Instance.m_draggedElement != null) newIi.Add(BattleUI.Instance.m_draggedElement.m_amount);
        Debug.Log("New II: " + newIi.m_item.m_name);
        Debug.Log($"Took {newIi.m_amount} {newIi.m_item.m_name}");
        BattleUI.Instance.SetDraggedElement(newIi);
        return newIi;
    }

    void OnEmptySlotClick(int slot, bool depositAll)
    {
        Debug.Log($"OnEmptySlotClick: {slot} | {depositAll}");
        if (BattleUI.Instance.m_draggedElement == null) return;
        int deposited = 0;
        if (!depositAll) deposited = BattleUI.Instance.m_draggedElement.Take(1);
        AddItem(slot, BattleUI.Instance.m_draggedElement.m_item, depositAll ? BattleUI.Instance.m_draggedElement.m_amount : deposited);
        if (BattleUI.Instance.m_draggedElement.m_amount <= 0) BattleUI.Instance.SetDraggedElement(null);
    }

    bool DraggedItemIsValid()
    {
        InventoryItem draggedElement = BattleUI.Instance.m_draggedElement;
        if (draggedElement == null) return false;
        if (draggedElement.m_item.m_type == IIData.ItemType.Unit && m_type == InventoryType.Units) return true;
        if (draggedElement.m_item.m_type == IIData.ItemType.Material && m_type == InventoryType.Materials) return true;
        if ((draggedElement.m_item.m_type == IIData.ItemType.Equipment || draggedElement.m_item.m_type == IIData.ItemType.Consumable) && m_type == InventoryType.Items) return true;
        return false;
    }

    void ItemSlotHandler(MouseUpEvent evt, int slot)
    {
        Debug.Log("item handler");
        if (!DraggedItemIsValid()) return;
        InventoryItem slotItem = GetSlotItem(slot);
        if (evt.button == 0 && BattleUI.Instance.m_draggedElement != null && slotItem != null) { SwapItems(slot, BattleUI.Instance.m_draggedElement, slotItem); return; }
        InventoryItem ii = OnItemClick(slot, evt.button == 0);
        if (ii == null) OnEmptySlotClick(slot, evt.button == 0);
    }
    void UnitSlotHandler(MouseUpEvent evt, int slot)
    {
        Debug.Log("unit handler");
        InventoryItem ii = OnItemClick(slot, false);
        if (ii == null) OnEmptySlotClick(slot, false);
        Debug.Log("Selected unit: " + ii.m_item);
        BattleManager.Instance.m_selectedUnit = ii.m_item as Unit;
    }
    void MaterialSlotHandler(MouseUpEvent evt, int slot)
    {
        Debug.Log("mat handler");
        InventoryItem ii = OnItemClick(slot, evt.button == 0);
        if (ii == null) OnEmptySlotClick(slot, evt.button == 0);
    }
    void CraftingSlotHandler(MouseUpEvent evt, int slot)
    {
        Debug.Log("craft handler");
        InventoryItem ii = OnItemClick(slot, evt.button == 0);
        if (ii == null) OnEmptySlotClick(slot, evt.button == 0);
    }

    InventoryItem GetSlotItem(int slot) => m_items.ElementAtOrDefault(slot);


    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    int GetItemIndex(InventoryItem item) => m_items.ToList().FindIndex(i => i.m_item.m_name == item.m_item.m_name);


    void SwapItems(int slot, InventoryItem intoInventory, InventoryItem intoDragged)
    {
        m_items[slot] = intoInventory;
        BattleUI.Instance.m_draggedElement = intoDragged;
    }

    public void AddItem(IIData item, int amount = 1)
    {
        if (StackIfItemExists(item, amount)) return;
        AddNewItem(GetFirstEmptySlot(), item, amount);
    }

    public void AddItem(int slot, IIData item, int amount = 1)
    {
        if (StackIfItemExists(item, amount)) return;
        AddNewItem(slot, item, amount);
    }

    bool StackIfItemExists(IIData item, int amount)
    {
        Debug.Log("StackIfItemExists");
        for (int i = 0; i < m_items.Length; i++)
        {
            if (m_items[i] == null || m_items[i].m_item.m_name != item.m_name) continue;
            m_items[i].m_amount += amount;
            BattleUI.Instance.SetItem(m_inventoryGrid, i, item.m_icon.texture, m_items[i].m_amount);
            Debug.Log("Stacked");
            return true;
        }
        return false;
    }

    void AddNewItem(int slot, IIData item, int amount)
    {
        Debug.Log("AddNewItem");
        InventoryItem ii = new(item, amount);
        m_items[slot] = ii;
        BattleUI.Instance.SetItem(m_inventoryGrid, slot, ii);
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < m_items.Length; i++)
            if (GetSlotItem(i) == null) return i;
        return -1;
    }

    public InventoryItem TakeItem(int slot, InventoryItem ii, int amount = 1)
    {
        m_items[slot].Take(amount);
        if (m_type != InventoryType.Units && m_items[slot].m_amount == 0) RemoveItem(slot, m_items[slot]);
        else BattleUI.Instance.SetItem(m_inventoryGrid, slot, m_items[slot]);
        return new InventoryItem(ii.m_item, amount);
    }

    public void RemoveItem(int slot, InventoryItem ii, int amount = 1)
    {
        m_items[slot] = null;
        BattleUI.Instance.RemoveItem(m_inventoryGrid, slot);
    }

    public InventoryItem GetII(Item item) => m_items.First(i => i.m_item.m_name == item.m_name);

}
