using System.Collections.Generic;
using UnityEditor;
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
    public List<Recipe> m_recipes;
}

#if UNITY_EDITOR
[CustomEditor(typeof(IIData))]
public class IIDataEditor : Editor
{
    SerializedProperty recipeProp;
    bool[] foldouts = new bool[10];
    int deleteIndex = -1;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        IIData item = target as IIData;
        //Recipe recipe = (Recipe)recipeProp.boxedValue;
        serializedObject.Update();
        recipeProp = serializedObject.FindProperty("m_recipes");
        EditorGUILayout.Space(15);
        GUILayout.Label("Recipes");
        for (int i = 0; i < item.m_recipes.Count; i++)
        {
            bool show = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[i], $"Recipe {i}");
            foldouts[i] = show;
            if (!foldouts[i]) { EditorGUILayout.EndFoldoutHeaderGroup(); continue; }
            Recipe recipe = item.m_recipes[i];
            for (int j = 0; j < 9; j += 3)
            {
                EditorGUILayout.BeginHorizontal();
                recipe.m_items[j].Item = (IIData)EditorGUILayout.ObjectField(recipe.m_items[j].Item, typeof(IIData), true, new GUILayoutOption[] { GUILayout.Height(30) });
                recipe.m_items[j + 1].Item = (IIData)EditorGUILayout.ObjectField(recipe.m_items[j + 1].Item, typeof(IIData), true, new GUILayoutOption[] { GUILayout.Height(30) });
                recipe.m_items[j + 2].Item = (IIData)EditorGUILayout.ObjectField(recipe.m_items[j + 2].Item, typeof(IIData), true, new GUILayoutOption[] { GUILayout.Height(30) });
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                recipe.m_items[j].Amount = EditorGUILayout.IntField(recipe.m_items[j].Amount);
                recipe.m_items[j + 1].Amount = EditorGUILayout.IntField(recipe.m_items[j + 1].Amount);
                recipe.m_items[j + 2].Amount = EditorGUILayout.IntField(recipe.m_items[j + 2].Amount);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        EditorGUILayout.Space(15);
        if (GUILayout.Button("Add Recipe"))
        {
            recipeProp.arraySize++;
            recipeProp.GetArrayElementAtIndex(recipeProp.arraySize - 1).boxedValue = new Recipe();
        }
        if (item.m_recipes.Count > 0)
        {
            if (deleteIndex == -1) deleteIndex = item.m_recipes.Count - 1;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete Recipe"))
                recipeProp.DeleteArrayElementAtIndex(deleteIndex);
            deleteIndex = EditorGUILayout.IntField(deleteIndex, new GUILayoutOption[] { GUILayout.Width(30) });
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(15);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif