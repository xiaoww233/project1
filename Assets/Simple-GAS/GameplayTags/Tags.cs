using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//标签树节点类
public class GameplayTagsNode
{
    public string name { get; private set; }//这个节点的名称，注意是全称，即从根节点一直到此节点的名称
    public string comment;//这个节点注释
    public GameplayTagsNode father;//节点的父节点
    public Dictionary<string, GameplayTagsNode> children;//这个节点的子节点们，使用字典可以避免同级标签出现重复节点，并且这里的键是当前层的名称而不是全称

    //构造函数
    public GameplayTagsNode(GameplayTagsNode father = null, string name = null, string comment = null)
    {
        this.father = father;
        this.name = name;
        this.comment = comment;
        children = new();
    }
    //这个函数得到全名中的最后一个标签（即当前节点所在层级的标签）
    public string GetLastName()
    {
        return name.Split(".")[^1];//"^1"表示倒数第一个元素
    }
    //这个函数的作用是修改节点的名字（也会递归地修改所有子树的名字）
    public void ChangeName(string newfullname)
    {
        father.children.Remove(GetLastName());
        name = newfullname + name.Substring(newfullname.Length);
        father.children.Add(GetLastName(), this);
        Debug.Log(name);
        foreach (GameplayTagsNode child in children.Values)
        {
            child.ChangeName(newfullname);
        }
    }
}

//在游戏进行时将标签加载至这个类的实例中
public class GameplayTags
{
    public Dictionary<string, GameplayTagsNode> alltags;//储存所有根标签
    public GameplayTags()
    {
        alltags = new();
    }

    //序列化为Json文件
    public void SerializeToJson(string filename)
    {
        string json = JsonUtility.ToJson(new GameplayTagsSerialize(this), true);
        Debug.Log(json);
        string path = Path.Combine(Application.dataPath, "config", "GameplayTags", filename);
        File.WriteAllText(path, json);
    }
    //从json文件中加载tag树
    public void CopyFromJson(string json)
    {
        if (json == "")
        {
            Debug.LogError("Json文件是空的");
            return;
        }
        var tool = JsonUtility.FromJson<GameplayTagsSerialize>(json);
        this.alltags = tool.AllTags.alltags;
    }
}

/*
序列化工具类,继承了一个unity自带的接口，unity虽然不能让我们自定义序列化逻辑，
但这个接口提供了两个函数，让我们能在序列化前后做一些事情，
比如将不可序列化的对象转化为可以序列化的新对象
*/
[Serializable]
class GameplayTagsSerialize : ISerializationCallbackReceiver
{
    [NonSerialized]
    public GameplayTags AllTags;//GameplayTags类的引用，之后从此处复制序列化后的节点，也从此处传入节点进行序列化
    [SerializeField]
    List<TagGroup> groups;//只有这个列表会被序列化为一个Json文件
    //因为是与GameplayTags绑定的工具类，所以规定必须传入一个GameplayTags
    public GameplayTagsSerialize(GameplayTags tags = null)
    {
        this.AllTags = tags ?? new();
        // 在没有实例传入时实例化一个gameplaytag对象，防止下面的函数空引用。
        Debug.Log(AllTags);
        groups = new();
    }
    public void OnAfterDeserialize()
    {
        AllTags ??= new();
        // 原因是JsonUtilty在反序列化时只会调用无参的构造函数，并且会直接写入所有非[NonSerialized]字段，并且忽略属性。
        // 在构造函数中进行的所有赋值都会被覆盖（除非是[NonSerialized]的）。
        // 在这个类中AllTags字段是[NonSerialized]的，所以不会被反序列化系统理会，而又由于无参构造函数是空的。
        // 而有参构造函数又不会被调用（尽管它可以被无参地调用），所以最终在反序化生成的实例中AllTags字段是NUll
        // 因而，在这里将其赋值为一个新的实例。
        foreach (TagGroup group in groups)
        {
            RestoreFromJson(group);
        }
    }

    public void OnBeforeSerialize()
    {
        if (AllTags.alltags.Count == 0)
        {
            Debug.LogError("在序列化前GameplayTags字段为空，可能是在构造时没有传入");
            return;
        }
        foreach (GameplayTagsNode root in AllTags.alltags.Values)
        {
            TraverseAndAdd(root);
        }
    }
    //辅助函数，用来遍历一棵树中的所有节点并将其添加进List中
    void TraverseAndAdd(GameplayTagsNode root)
    {
        groups.Add(new(root.name, root.comment));
        if (root.children.Count == 0) return;
        foreach (GameplayTagsNode node in root.children.Values)
        {
            TraverseAndAdd(node);
        }
    }
    //辅助函数，将一个list表项还原至GameplayTags类中
    void RestoreFromJson(TagGroup group)
    {
        string[] taglist = group.name.Split(".", StringSplitOptions.RemoveEmptyEntries);//一个列表，储存名称被拆分后的列表层次
        GameplayTagsNode lastnode = null;//上一个标签树的节点，初始为空
        Dictionary<string, GameplayTagsNode> currentdic = AllTags.alltags;//当前的列表字典，初始为根标签字典
        string currentname = "";//记录当前的标签全名，初始为空
        for (int i = 0; i < taglist.Length; i++)
        {
            currentname = string.Join(".", taglist, 0, i + 1);//使用join函数拼接字符串，避免最前方的“.”符号
            if (currentdic.ContainsKey(taglist[i]))
            {
                lastnode = currentdic[taglist[i]];
                currentdic = currentdic[taglist[i]].children;
                //若是当前字典已经有标签，修改lastnode与currentdic，前进到下一个节点
            }
            else
            {
                GameplayTagsNode tempnode = new(lastnode, currentname);
                if (i == taglist.Length - 1)
                {
                    tempnode.comment = group.comment;
                }
                currentdic.Add(taglist[i], tempnode);
                lastnode = tempnode;
                currentdic = tempnode.children;
                //若是当前字典没有标签，添加标签后再修改
            }
        }
    }

}
[Serializable]
//可序列化的打包类
public class TagGroup
{
    public string name;//同样是全名
    public string comment;//注释
    public TagGroup(string name, string comment)
    {
        this.name = name;
        this.comment = comment;
    }
}