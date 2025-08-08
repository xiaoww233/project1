using System.Collections.Generic;

//标签树节点类
class GameplayTagsNode
{
    public string name;//这个节点的名称，注意是全程，即从根节点一直到此节点的名称
    public string comment;//这个节点注释
    public GameplayTagsNode father;//节点的父节点
    public Dictionary<string, GameplayTagsNode> children;//这个节点的子节点们，使用字典可以避免同级标签出现重复节点

    //构造函数
    GameplayTagsNode(GameplayTagsNode father = null, string name = null, string comment = null)
    {
        this.father = father;
        this.name = name;
        this.comment = comment;
    }
}

//在游戏进行时将标签加载至这个类的实例中
class GameplayTags
{

}

class GameplayTagsSer
{
    
}