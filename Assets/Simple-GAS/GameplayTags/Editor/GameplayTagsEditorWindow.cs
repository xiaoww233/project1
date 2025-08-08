using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayTagsEditorWindow : EditorWindow
{
    ObjectField JsonRef;
    TextField ValueField;
    TextField NoteField;
    Button SaveButton;
    TreeView TagTreeDisplay;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Simple-GAS/GameplayTag编辑器")]
    public static void ShowExample()
    {
        GameplayTagsEditorWindow wnd = GetWindow<GameplayTagsEditorWindow>();
        wnd.titleContent = new GUIContent("标签编辑器");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        m_VisualTreeAsset.CloneTree(root);

        JsonRef = root.Q<ObjectField>("JsonFileSelect");
        ValueField = root.Q<TextField>("NameValue");
        NoteField = root.Q<TextField>("NoteValue");
        SaveButton = root.Q<Button>("SaveButton");
        TagTreeDisplay = root.Q<TreeView>("TreeViewDisplay");

        JsonRef.objectType = typeof(TextAsset);
        JsonRef.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue != null)
            {
                string path = AssetDatabase.GetAssetPath(evt.newValue);
                if (!path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("只能选择Json文件");
                    JsonRef.value = evt.previousValue;
                }
                else
                {
                    ListTagTree();
                }
            }
        });


    }
    void ListTagTree()
    {
        
    }
}
