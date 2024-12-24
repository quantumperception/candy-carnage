using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUI : MonoBehaviour
{
    public UIDocument m_uiDocument;
    public AudioSource m_audioSource;
    public AudioClip m_hoverSound;
    public AudioClip m_clickSound;
    public static BattleUI Instance { get; private set; }
    public VisualElement root;
    Label debugInfo;
    Button team1;
    Button team2;
    Button pause;
    Button quarterSpeed;
    Button halfSpeed;
    Button normalSpeed;
    public InventoryItem m_draggedElement;
    VisualElement draggedElement;
    Label draggedElementAmount;
    public VisualElement battleUi;
    public VisualElement toolbar;
    public VisualElement itemInventory;
    public VisualElement unitInventory;
    public VisualElement materialInventory;
    public VisualElement craftingInventory;
    public bool m_ready { get; private set; } = false;

    private VisualElement _root;
    private readonly List<Button> m_buttons = new();

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        m_buttons.AddRange(root.Query<Button>().ToList());
        foreach (Button button in m_buttons)
        {
            button.RegisterCallback<ClickEvent>(OnPress, TrickleDown.TrickleDown);
        }
    }
    private void OnPress(ClickEvent evt)
    {
        var clickedButton = (Button)evt.target;
        PlayClickSound(); ;
    }

    private void OnHover(MouseEnterEvent evt)
    {
        var hoveredButton = (Button)evt.target;
        PlayHoverSound();
    }
    public void PlayHoverSound()
    {
        if (m_hoverSound != null)
        {
            PlayRandomizedPitchSfx(m_hoverSound);
        }
    }

    private void PlayClickSound()
    {
        if (m_clickSound != null)
        {
            PlayRandomizedPitchSfx(m_clickSound);
        }
    }

    private void PlayRandomizedPitchSfx(AudioClip clip)
    {
        float randomPitch = Eerp(0.8f, 1.1f, Random.value);
        m_audioSource.pitch = randomPitch;
        m_audioSource.PlayOneShot(clip);
    }

    private float Eerp(float a, float b, float t)
    {
        return a * Mathf.Exp(t * Mathf.Log(b / a));
    }
    void Start()
    {
        Setup();
    }

    void Update()
    {
        if (!m_ready) return;
        if (BattleGrid.Instance.m_hoveredCell != null) debugInfo.text = "Hovered cell: " + BattleGrid.Instance.m_hoveredCell.m_coords;
        else debugInfo.text = "Hovered cell:";
        debugInfo.text += $"\n{BattleManager.Instance.GetSelectedTeam()} - {Team.colors[BattleManager.Instance.GetSelectedTeam()]}";
        Vector2 mousePos = ScreenToPanel(Input.mousePosition);
        draggedElement.style.left = mousePos.x + 1;
        draggedElement.style.top = mousePos.y + 1;
    }

    async void Setup()
    {
        await UniTask.WaitUntil(() => BattleManager.Instance.m_ready);
        root = GetComponent<UIDocument>().rootVisualElement;
        debugInfo = root.Q<Label>("DebugInfo");
        team1 = root.Q<Button>("Team1");
        team2 = root.Q<Button>("Team2");
        team1.clicked += () => BattleManager.Instance.SetSelectedTeam(1);
        team2.clicked += () => BattleManager.Instance.SetSelectedTeam(2);
        battleUi = root.Q<VisualElement>("BattleUI");
        pause = root.Q<Button>("Pause");
        pause.clicked += () => { Time.timeScale = 0; };
        quarterSpeed = root.Q<Button>("QuarterSpeed");
        quarterSpeed.clicked += () => { Time.timeScale = 0.25f; };
        halfSpeed = root.Q<Button>("HalfSpeed");
        halfSpeed.clicked += () => { Time.timeScale = 0.5f; };
        normalSpeed = root.Q<Button>("NormalSpeed");
        normalSpeed.clicked += () => { Time.timeScale = 1f; };
        draggedElement = root.Q<VisualElement>("DraggedElement");
        draggedElement.visible = false;
        draggedElementAmount = draggedElement.Q<Label>("Amount");
        draggedElementAmount.visible = false;
        craftingInventory = root.Q<VisualElement>("CraftingInventory");
        craftingInventory.visible = false;
        toolbar = root.Q<VisualElement>("Toolbar");
        itemInventory = toolbar.Q<VisualElement>("ItemInventory");
        unitInventory = toolbar.Q<VisualElement>("UnitInventory");
        materialInventory = toolbar.Q<VisualElement>("MaterialInventory");
        m_ready = true;
    }

    Vector2 ScreenToPanel(Vector3 mousePosition)
    {
        return RuntimePanelUtils.ScreenToPanel(root.panel,
            new Vector2(mousePosition.x, Screen.height - mousePosition.y));
    }


    void ClearSlot(VisualElement slot)
    {
        VisualElement icon = slot.Q<VisualElement>("Icon");
        icon.style.backgroundImage = null;
        Label stackAmount = slot.Q<Label>("StackAmount");
        stackAmount.visible = false;
    }

    void SetSlotIcon(VisualElement slot, Texture2D tex, int amount = 1)
    {
        VisualElement icon = slot.Q<VisualElement>("Icon");
        icon.style.backgroundImage = tex;
        icon.style.opacity = amount > 0 ? 1 : 0.6f;
        Label stackAmount = slot.Q<Label>("StackAmount");
        stackAmount.text = amount.ToString();
        stackAmount.visible = true;
    }
    public void SetDraggedElement(InventoryItem item)
    {
        draggedElement.visible = item != null;
        draggedElement.SetEnabled(false);
        draggedElementAmount.visible = item != null && item.m_amount > 1;
        m_draggedElement = item;
        if (item == null) return;
        draggedElementAmount.text = item.m_amount.ToString();
        draggedElement.style.backgroundImage = item.m_item.m_icon.texture;
        Debug.Log($"Dragged element: {item.m_item.m_name}");
    }
    public void SetItem(VisualElement inventory, int slot, InventoryItem ii) { SetItem(inventory, slot, ii.m_item.m_icon.texture, ii.m_amount); }

    public void SetItem(VisualElement inventory, int slot, Texture2D tex, int amount)
    {
        VisualElement visualSlot = inventory.Children().ElementAt(slot);
        if (visualSlot == null) return;
        SetSlotIcon(visualSlot, tex, amount);
    }

    public void RemoveItem(VisualElement inventory, int slot)
    {
        VisualElement visualSlot = inventory.Children().ElementAt(slot);
        if (visualSlot == null) return;
        ClearSlot(visualSlot);
    }
}
