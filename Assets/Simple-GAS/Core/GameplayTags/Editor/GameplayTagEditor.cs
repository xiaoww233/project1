using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common.TreeGrouper;
using Codice.CM.Common.Merge;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayTagEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    GameplayTags tags;//一个编辑器的的临时引用，用于构建树
    DropdownField json;
    bool isnull;//表示json选择是否为空
    string filename;//储存当前打开的文件名
    TextField namefield;
    TextField commentfield;
    Button createbutton;
    Button savebutton;//这个按钮很重要，一定要保存啊
    TreeView tagdisplay;

    [MenuItem("Simple-GAS/GameplayTag编辑器")]
    public static void ShowExample()
    {
        GameplayTagEditor wnd = GetWindow<GameplayTagEditor>();
        wnd.minSize = new(320, 300);
        wnd.titleContent = new GUIContent("GameplayTag编辑器");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset.CloneTree(root);
        tags = new();

        json = root.Q<DropdownField>("JsonSelect");
        namefield = root.Q<TextField>("NameField");
        commentfield = root.Q<TextField>("CommentField");
        createbutton = root.Q<Button>("CreateNodeButton");
        savebutton = root.Q<Button>("SaveButton");
        tagdisplay = root.Q<TreeView>("TagsDisplay");

        //Json选择部分
        //这一句是在GameplayTags文件夹中搜索所有的gameplaytag配置文件，并返回它们的GUID为一个string数组
        string[] guides = AssetDatabase.FindAssets("", new[] { "Assets/config/GameplayTags" });
        //然后将GUID反向转化为具体的路径,并设置数据源
        List<string> tagsfile = guides
        .Select(AssetDatabase.GUIDToAssetPath)
        .Where(path => path.EndsWith(".json"))
        .Select(System.IO.Path.GetFileName)
        .ToList();
        tagsfile.Insert(0, "NULL");
        json.choices = tagsfile;
        json.value = "NULL";//设定一个默认值
        //设置值变化的回调
        json.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue == "NULL")
            {
                Debug.Log("切换为空");
                isnull = true;
                filename = null;
                return;
            }
            tags.CopyFromJson(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/config/GameplayTags/" + evt.newValue).text);
            isnull = false;
            filename = evt.newValue;
            Debug.Log("从" + "Assets/config/GamplayTags/" + evt.newValue + "处加载了GameplayTags资源");
            RefreshTreeView();
        });

        //创建新节点按钮部分
        createbutton.clicked += MakeNewNode;
        //保存节点部分
        savebutton.clicked += SaveToJson;

        //tag展示部分
        tagdisplay.makeItem = () =>
        {
            VisualElement container = new();//使用一个容器包装treeview的元素，便于进行元素附加
            var label = new Label
            {
                name = "labelitem"
            };
            var menu = new ContextualMenuManipulator(evt =>
            {
                //得到储存在userdata里面的当前节点的引用
                GameplayTagsNode currentnode = (evt.currentTarget as VisualElement).userData as GameplayTagsNode;
                evt.menu.AppendAction("创建子节点", (menudata) =>
                {
                    namefield.value = currentnode.name + ".";
                    namefield.Focus();
                });
                evt.menu.AppendAction("重命名节点", (menudata) =>
                {
                    NodeEditorWindow.ShowWindow(currentnode);
                    RefreshTreeView();
                });
                evt.menu.AppendAction("删除该节点", (menudata) =>
                {
                    if (currentnode.father == null)
                    {
                        tags.alltags.Remove(currentnode.name);
                        RefreshTreeView();
                        return;
                    }
                    currentnode.father.children.Remove(currentnode.name.Split(".", System.StringSplitOptions.RemoveEmptyEntries)[^1]);
                    RefreshTreeView();
                });
            });
            container.Add(label);
            container.AddManipulator(menu);
            return container;
        };
        tagdisplay.bindItem = (elemnt, index) =>
        {
            var label = elemnt.Q<Label>("labelitem");
            var item = tagdisplay.GetItemDataForIndex<GameplayTagsNode>(index);
            label.text = item.GetLastName();
            elemnt.tooltip = item.comment;
            elemnt.userData = item;
        };
    }
    void RefreshTreeView()
    {
        if (tags.alltags.Count == 0)
        {
            Debug.Log("在构建treeview数据时发现标签树为空");
            tagdisplay.SetRootItems(new List<TreeViewItemData<GameplayTagsNode>>());
            tagdisplay.Rebuild();
            return;
        }
        tagdisplay.SetRootItems(BuildTreeViewData());
        tagdisplay.Rebuild();
    }
    List<TreeViewItemData<GameplayTagsNode>> BuildTreeViewData()
    {
        List<TreeViewItemData<GameplayTagsNode>> result = new();
        int id = 0;
        foreach (GameplayTagsNode root in tags.alltags.Values)
        {
            result.Add(CreateOneTreeData(root, ref id));
        }
        return result;
    }
    TreeViewItemData<GameplayTagsNode> CreateOneTreeData(GameplayTagsNode root, ref int id)
    {
        List<TreeViewItemData<GameplayTagsNode>> children = new();
        foreach (GameplayTagsNode node in root.children.Values)
        {
            children.Add(CreateOneTreeData(node, ref id));
        }
        TreeViewItemData<GameplayTagsNode> result = new(id, root, children);
        id++;
        return result;
    }
    //这个函数使用namefield与commentfield里面的内容新建节点
    void MakeNewNode()
    {
        if (namefield.value == null)
        {
            Debug.LogWarning("请输入节点名称");
            return;
        }
        //这个函数的逻辑是复用的Tags脚本下GameplayTagsSerialize类的RestoreFromJson函数的逻辑
        string[] taglist = namefield.text.Split(".", System.StringSplitOptions.RemoveEmptyEntries);
        GameplayTagsNode lastnode = null;//上一个标签树的节点，初始为空
        Dictionary<string, GameplayTagsNode> currentdic = tags.alltags;//当前的列表字典，初始为根标签字典
        string currentname = "";//记录当前的标签全名，初始为空
        for (int i = 0; i < taglist.Length; i++)
        {
            currentname = string.Join(".", taglist, 0, i + 1);//使用join函数拼接字符串，避免最前方的“.”符号
            if (currentdic.ContainsKey(taglist[i]))
            {
                lastnode = currentdic[taglist[i]];
                currentdic = currentdic[taglist[i]].children;
                if (i == taglist.Length - 1)
                {
                    lastnode.comment = commentfield.value;
                }
                //若是当前字典已经有标签，修改lastnode与currentdic，前进到下一个节点
            }
            else
            {
                GameplayTagsNode tempnode = new(lastnode, currentname);
                if (i == taglist.Length - 1)
                {
                    tempnode.comment = commentfield.value;
                }
                currentdic.Add(taglist[i], tempnode);
                lastnode = tempnode;
                currentdic = tempnode.children;
                //若是当前字典没有标签，添加标签后再修改
            }
        }
        RefreshTreeView();
    }
    //这个函数保存标签树到Json文件
    void SaveToJson()
    {
        if (isnull)
        {
            Debug.LogError("Json文件为空，请选择一个可用的Json文件");
            return;
        }
        tags.SerializeToJson(filename);
        EditorUtility.DisplayDialog("提示", "保存完成", "好的", "知道了");
    }
}

class NodeEditorWindow : EditorWindow
{
    GameplayTagsNode node;//要修改的节点
    string[] tags;//node节点被拆分后的标签数组
    TextField namefield;
    TextField commentfield;
    Button cancelbutton;
    Button confirmbutton;
    public static void ShowWindow(GameplayTagsNode node)
    {
        var wnd = CreateInstance<NodeEditorWindow>();
        wnd.titleContent = new GUIContent("节点编辑");
        wnd.minSize = new(250, 120);
        wnd.maxSize = new(250, 120);
        wnd.node = node;
        wnd.ShowUtility();
    }
    void CreateGUI()
    {
        var root = rootVisualElement;
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Simple-GAS/GameplayTags/Editor/NodeEditor.uxml");
        asset.CloneTree(root);

        namefield = root.Q<TextField>("NameField");
        commentfield = root.Q<TextField>("CommentField");
        cancelbutton = root.Q<Button>("CancelButton");
        confirmbutton = root.Q<Button>("ConfirmButton");

        //namefield部分
        tags = node.name.Split(".", System.StringSplitOptions.RemoveEmptyEntries);
        namefield.value = tags[^1];

        //commentfield部分
        commentfield.value = node.comment;

        //取消按钮部分
        cancelbutton.clicked += () =>
        {
            Close();
        };

        //确认按钮部分
        confirmbutton.clicked += () =>
        {
            tags[^1] = namefield.value;
            string newname = string.Join(".", tags);
            Debug.Log(node.name);
            node.comment = commentfield.value;
            node.ChangeName(newname);
            Close();
        };
    }
}
