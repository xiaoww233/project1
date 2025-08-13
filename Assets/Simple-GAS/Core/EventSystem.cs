using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Codice.Client.Commands;
using Sirenix.Utilities;
using UnityEngine;

namespace GAS
{
    public class EventSubscriber<T> where T : Delegate
    {
        public T CallBackFuction;
        public int Priority;

        public EventSubscriber(T callback, int priority)
        {
            CallBackFuction = callback;
            Priority = priority;
        }
    }

    //和c#自带的event没什么不同的Event类，只是事件的执行顺序由优先级决定
    public class Event<T> where T : Delegate
    {
        List<EventSubscriber<T>> CallBackList;
        public Event()
        {
            CallBackList = new();
        }

        //使用EventSubscriber对象注册函数
        public void Sign(EventSubscriber<T> subscriber)
        {
            CallBackList.Add(subscriber);
            CallBackList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
        //使用T callback, int priority注册函数
        public EventSubscriber<T> Sign(T callback, int priority)
        {
            var temp = new EventSubscriber<T>(callback, priority);
            CallBackList.Add(temp);
            CallBackList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return temp;
        }

        //使用EventSubscriber对象注销函数
        public void Unsign(EventSubscriber<T> subscriber)
        {
            CallBackList.Remove(subscriber);
        }
        //使用callback注销函数
        public void Unsign(T callback)
        {
            CallBackList.RemoveAll(s => s.CallBackFuction == callback);
        }
        //使用优先级注销函数（注意会注销所有对应优先级的函数）
        public void Unsign(int priority)
        {
            CallBackList.RemoveAll(s => s.Priority == priority);
        }

        //使用callback获取优先级
        public int GetPriority(T callback)
        {
            return GetEventSubscriber(callback).Priority;
        }

        public EventSubscriber<T> GetEventSubscriber(T callback)
        {
            return CallBackList.FirstOrDefault(s => s.CallBackFuction == callback);
        }
        public List<EventSubscriber<T>> GetEventSubscriber(int priority)
        {
            return CallBackList.Where(s => s.Priority == priority).ToList();
        }

        public void SetNewPriority(int newpriority, T callback)
        {
            GetEventSubscriber(callback).Priority = newpriority;
            CallBackList.Sort();
        }
        public void SetNewPriority(int newpriority, int oldpriority)
        {
            var list = GetEventSubscriber(oldpriority);
            foreach (EventSubscriber<T> subscriber in list)
            {
                subscriber.Priority = newpriority;
            }
            CallBackList.Sort();
        }

        public void InvokeByPriority(params object[] args)
        {
            var subscribersToInvoke = new List<EventSubscriber<T>>(CallBackList);
            foreach (var subscriber in subscribersToInvoke)
            {
                try
                {
                    subscriber.CallBackFuction?.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"在执行 GASEvent<{typeof(T).Name}> 的回调时发生错误: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}