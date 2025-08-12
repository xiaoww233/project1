using System;
using System.Collections.Generic;
using System.Data;
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

    public class Event<T> where T : Delegate
    {
        List<EventSubscriber<T>> CallBackList;
        Event()
        {
            CallBackList = new();
        }
        public void Sign(EventSubscriber<T> subscriber)
        {
            CallBackList.Add(subscriber);
            CallBackList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
        public EventSubscriber<T> Sign(T callback, int priority)
        {
            var temp = new EventSubscriber<T>(callback, priority);
            CallBackList.Add(temp);
            CallBackList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return temp;
        }
        public void Unsign(EventSubscriber<T> subscriber)
        {
            CallBackList.Remove(subscriber);
        }
        public void Unsign(T callback)
        {
            CallBackList.RemoveAll(s => s.CallBackFuction == callback);
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