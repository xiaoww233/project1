using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Build.Player;
using UnityEngine;

interface IBlackBoard
{
    /// <summary>
    /// ��ȡָ������ֵ
    /// </summary>
    /// <typeparam name="T">ֵ�����ͣ����պڰ��д�������ݽṹ�Ĳ�ͬ�仯</typeparam>
    /// <param name="key">����</param>
    /// <returns>��Ӧ��ֵ������������򷵻�Ĭ��ֵ</returns>
    T GetValue<T>(string key);
    /// <summary>
    /// �趨ָ����ֵ
    /// </summary>
    /// <typeparam name="T">ֵ�����ͣ����պڰ��д�������ݽṹ�Ĳ�ͬ�仯</typeparam>
    /// <param name="key">����</param>
    /// <param name="value">Ҫ�����ֵ</param>
    void SetValue<T>(string key, T value);
}

class FSMNodeBlackBoard : IBlackBoard
{
    public GameObject player;
    public Animator playerAnimator;
    public Dictionary<string, float> contextValue;
    public List<string> contextKeys;
    public string currentState;
    public int currentstateHash;
    public FSMNodeBlackBoard()
    {
        contextValue = new Dictionary<string, float>();
        contextKeys = new List<string>();
    }
    public T GetValue<T>(string key)
    {
        switch (typeof(T).Name)
        {
            case "GameObject":
                if (key == "player")
                {
                    return (T)(object)player;
                }
                break;
            case "Animator":
                if (key == "playerAnimator")
                {
                    return (T)(object)playerAnimator;
                }
                break;
            case "Single":
                if (contextValue.ContainsKey(key))
                {
                    return (T)(object)contextValue[key];
                }
                break;
            case "Int32":
                if (key == "currentStateHash")
                {
                    return (T)(object)currentstateHash;
                }
                break;
            case "String":
                if (key == "currentState")
                {
                    return (T)(object)currentState;
                }
                break;
            default:
                break;
        }
        return default;
    }

    public void SetValue<T>(string key, T value)
    {
        switch (typeof(T).Name)
        {
            case "GameObject":
                if (key == "player")
                {
                    player = (GameObject)(object)value;
                }
                break;
            case "Animator":
                if (key == "playerAnimator")
                {
                    playerAnimator = (Animator)(object)value;
                }
                break;
            case "Single":
                if (contextKeys.Contains(key))
                {
                    contextValue[key] = (float)(object)value;
                }
                break;
            case "Int32":
                if (key == "currentstateHash")
                {
                    currentstateHash = (int)(object)value;
                }
                break;
            case "String":
                if (key == "currentState")
                {
                    currentState = (string)(object)value;
                }else if(key=="stateKeys"){
                    contextKeys.Add((string)(object)value);
                }
                break;
            case "List`1":
                if (key == "stateKeys")
                {
                    contextKeys = (List<string>)(object)value;
                }
                break;
            default:
                break;
        }
    }
}

class TotalBlackBoard : IBlackBoard
{
    public T GetValue<T>(string key)
    {
        throw new NotImplementedException();
    }

    public void SetValue<T>(string key, T value)
    {
        throw new NotImplementedException();
    }
}

class CommonBlackBorad : IBlackBoard
{
    public T GetValue<T>(string key)
    {
        throw new NotImplementedException();
    }

    public void SetValue<T>(string key, T value)
    {
        throw new NotImplementedException();
    }
}