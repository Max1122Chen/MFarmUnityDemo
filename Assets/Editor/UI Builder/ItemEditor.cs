using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using InventorySystem;
public class ItemEditor : EditorWindow
{
    private ItemDataList_SO dataBase;

    private List<ItemDefinition> itemList = new List<ItemDefinition>();

    private ListView itemListView;
    private VisualTreeAsset itemRowTemplate;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("MyTools/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // Get the item row template
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");

        // Get the ListView
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");

        LoadDataBase();

        GenerateItemRows();

    }

    private void LoadDataBase()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataList_SO");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            dataBase = AssetDatabase.LoadAssetAtPath<ItemDataList_SO>(path);
        }

        itemList = dataBase.itemDataList;

        // Set the dataBase as dirty to enable saving changes
        EditorUtility.SetDirty(dataBase);

    }

    private void GenerateItemRows()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if(i < itemList.Count)
            {
                ItemDefinition item = itemList[i];

                if(item.itemIcon != null)
                    e.Q<VisualElement>("ItemIcon").style.backgroundImage = item.itemIcon.texture;
                e.Q<Label>("ItemName").text = item.itemName == null ? "Undefined" : item.itemName;
            }
            
        };

        // Set items source
        itemListView.itemsSource = itemList;

        itemListView.fixedItemHeight = 50;

        // Bind callbacks
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;
    }
}
