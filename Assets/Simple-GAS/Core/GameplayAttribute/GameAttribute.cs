using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using GAS;

public class GameplayAttributeData
{
    public string name;
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
    internal void ChangeCurrentValue(float newvalue, ValueChangeWay way)
    {
        if (way == ValueChangeWay.Replace)
        {
            CurrentValue = newvalue;
        }
        else if (way == ValueChangeWay.Plus)
        {
            CurrentValue += newvalue;
        }

    }
}

public struct AttributeModifierData<TEnum> where TEnum : Enum
{
    public TEnum TargetAttribute;
    public float ChangeValue;
    public ValueChangeWay ChangeWay;
    public object Source;
}

public class PreAttributeChangeData
{
    public GameplayAttributeData data;//要修改的属性
    public float ChangeValue;//要修改的数值
    public ValueChangeWay Changeway;
    public bool IsCancel;//是否要取消修改
}
public class AfterAttributeChangeData
{
    public GameplayAttributeData data;//改变了的属性
    public float ChangeValue;//最终改变了多少
    public bool IsChanged;//改变是否真的应用了（改变量为0并不代表改变没有应用）

}

public class AttributeSet<TEnum> where TEnum : Enum
{
    GameAbilitySystem Owner;
    protected Dictionary<TEnum, GameplayAttributeData> Attributes;
    public delegate void PreAttritudeChange(PreAttributeChangeData data);
    public delegate void ValueChangeCheck(AfterAttributeChangeData data);
    public delegate void AfterAttritudeChange(AfterAttributeChangeData data);
    public Event<PreAttritudeChange> OnPreAttritudeChange;
    public Event<AfterAttritudeChange> OnAfterAttritudeChange;

    public AttributeSet(GameAbilitySystem Owner = null)
    {
        Attributes = new();
        this.Owner = Owner;
        OnPreAttritudeChange = new();
        OnAfterAttritudeChange = new();
        foreach (TEnum attribute in Enum.GetValues(typeof(TEnum)))
        {
            Attributes.Add(attribute, new(attribute.ToString()));
        }
    }
    public void InitAttributeValue(List<float> newvalue)
    {
        if (newvalue.Count != Attributes.Count)
        {
            Debug.LogError($"在初始化使用枚举：{typeof(TEnum)}构建的AttributeSet时发生错误：初始化所用List的元素数量不为枚举中与只能说数量");
            return;
        }
        var AttributeTypes = (TEnum[])Enum.GetValues(typeof(TEnum));
        for (int i = 0; i < newvalue.Count; i++)
        {
            Attributes[AttributeTypes[i]].SetBaseValue(newvalue[i]);
            Attributes[AttributeTypes[i]].ChangeCurrentValue(newvalue[i], ValueChangeWay.Replace);
        }

    }
    public GameplayAttributeData GetAttributeRef(TEnum name)
    {
        return Attributes[name];
    }
    public float GetCurrentValue(TEnum name)
    {
        return GetAttributeRef(name).CurrentValue;
    }
    public float GetBaseValue(TEnum name)
    {
        return GetAttributeRef(name).BaseValue;
    }
    internal void AttributeModifier(AttributeModifierData<TEnum> modifierData)//这里应该传入一个参数，是GE的修改数据
    {
        if (!Attributes.ContainsKey(modifierData.TargetAttribute)) return;
        PreAttributeChangeData data = new()
        {
            data = Attributes[modifierData.TargetAttribute],
            ChangeValue = modifierData.ChangeValue,
            Changeway = modifierData.ChangeWay,
            IsCancel = false
        };
        //这里应该使用传入的参数填充data;

        float unchangeValue = data.data.CurrentValue;

        OnPreAttritudeChange.InvokeByPriority(data);//按照设定好的优先级调用函数

        AfterAttributeChangeData afterdata = new();

        if (!data.IsCancel == true)
        {
            data.data.ChangeCurrentValue(data.ChangeValue, data.Changeway);
        }

        afterdata.data = data.data;
        afterdata.ChangeValue = data.data.CurrentValue - unchangeValue;
        afterdata.IsChanged = !data.IsCancel;

        OnAfterAttritudeChange.InvokeByPriority(afterdata);
    }
}