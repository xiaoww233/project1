using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using GAS;

internal class GameplayAttributeData
{
    string name;
    public float BaseValue { get; private set; }//不会变化的基础值
    public float CurrentValue { get; private set; }//被被BUFF改变的当前值
    internal GameplayAttributeData(string name)
    {
        this.name = name;
        BaseValue = default;
        CurrentValue = default;
    }

    internal void SetBaseValue(float newvalue)
    {
        BaseValue = newvalue;
    }
    internal void ChangeCurrentValue(float newvalue)
    {
        CurrentValue = newvalue;
    }


}

public struct PreAttributeChangeData
{
    GameplayAttributeData data;//要修改的属性
    float ChangeValue;//要修改的数值
    bool IsCancel;//是否要取消修改
}
public struct AfterAttributeChangeData
{
    Enum dataname;//改变了的属性
    float ChangeValue;//最终改变了多少
    bool IsChanged;//改变是否真的应用了（改变量为0并不代表改变没有应用）

}

public class AttributeSet<TEnum> where TEnum : Enum
{
    GameAbilitySystem Owner;
    Dictionary<TEnum, GameplayAttributeData> Attributes;
    public delegate void PreAttritudeChange(ref PreAttributeChangeData data);
    public delegate void AfterAttritudeChange(ref AfterAttributeChangeData data);
    public Event<PreAttritudeChange> OnPreAttritudeChange;
    public Event<AfterAttritudeChange> OnAfterAttritudeChange;

    AttributeSet(GameAbilitySystem Owner=null)
    {
        this.Owner = Owner;
        foreach (TEnum attribute in Enum.GetValues(typeof(TEnum)))
        {
            Attributes.Add(attribute, new(attribute.ToString()));
        }
    }
    void InitAttributeValue(List<float> newvalue)
    {
        if (newvalue.Count != Attributes.Count)
        {
            Debug.LogError($"在初始化使用枚举：{typeof(TEnum)}构建的AttributeSet时发生错误：初始化所用List的元素数量不为枚举中与只能说数量");
            return;
        }
        for (int i = 0; i < newvalue.Count; i++)
        {
            Attributes[(TEnum)Enum.GetValues(typeof(TEnum)).GetValue(i)].SetBaseValue(newvalue[i]);
            Attributes[(TEnum)Enum.GetValues(typeof(TEnum)).GetValue(i)].ChangeCurrentValue(newvalue[i]);
        }

    }
    GameplayAttributeData GetAttributeRef(TEnum name)
    {
        return Attributes[name];
    }
    internal void AttributeModifier()//这里应该传入一个参数，是GE的修改数据
    {
        PreAttributeChangeData data = new();
        //这里应该使用传入的参数填充data


    }
}