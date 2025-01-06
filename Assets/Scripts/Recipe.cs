using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Recipe
{
    public ItemAmount[] m_items;

    public Recipe()
    {
        m_items = new ItemAmount[9];
    }
    public Recipe(ItemAmount[] items)
    {
        m_items = items;
    }
}

//#if UNITY_EDITOR
//[CustomEditor(typeof(Recipe))]
//public class RecipeEditor : Editor
//{
//    SerializedProperty items;
//    void OnEnable()
//    {
//        items = serializedObject.FindProperty("m_items");
//    }
//    public override VisualElement CreateInspectorGUI()
//    {
//        VisualElement customInspector = new VisualElement();
//        customInspector.style.height = 150;
//        customInspector.style.width = 150;
//        customInspector.style.display = DisplayStyle.Flex;
//        customInspector.style.flexDirection = FlexDirection.Row;
//        customInspector.style.flexWrap = Wrap.Wrap;
//        customInspector.style.justifyContent = Justify.SpaceEvenly;
//        customInspector.style.alignItems = Align.Center;
//        for (int i = 0; i < 9; i++)
//            customInspector.Add(CreateSlot(i));
//        return customInspector;
//    }
//    VisualElement CreateSlot(int i)
//    {
//        VisualElement slot = new VisualElement() { name = "slot" };
//        slot.style.width = Length.Percent(33);
//        slot.style.height = Length.Percent(33);
//        slot.style.borderBottomWidth = 1;
//        slot.style.borderBottomColor = Color.black;
//        slot.style.borderRightWidth = 1;
//        slot.style.borderRightColor = Color.black;
//        slot.style.backgroundColor = Color.red;
//        return slot;
//    }


//}
//#endif