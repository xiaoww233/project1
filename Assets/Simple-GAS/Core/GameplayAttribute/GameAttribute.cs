using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

internal class GameplayAttributeData
{
    public float BaseValue { get; private set; }//不会变化的基础值
    public float CurrentValue { get; private set; }//被被BUFF改变的当前值
    GameplayAttributeData()
    {
        BaseValue = default;
        CurrentValue = default;
    }

    internal void SetBase(float newvalue)
    {
        BaseValue = newvalue;
    }
    internal void ChangeCurrent(float newvalue)
    {
        CurrentValue = newvalue;
    }

}

//这个接口仅作一个约束，标记一个枚举是AttributeSet的数据源枚举
interface IAttributeSetSourceEnum { }

class AttributeSet<TEnum> where TEnum : Enum
{
    GameAbilitySystem Owner;
    Dictionary<TEnum, GameplayAttributeData> Attributes;
}