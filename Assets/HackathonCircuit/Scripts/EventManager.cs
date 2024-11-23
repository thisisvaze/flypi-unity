using System;
using System.Collections.Generic;
using UnityEngine;


public class EventManager : MonoBehaviour
{
    private Dictionary<string, Action<string>> eventDictionary;

    private static EventManager eventManager;

    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(eventManager);
                }
            }
            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, Action<string>>();
        }
    }

    public static void StartListening(string eventName, Action<string> listener)
    {
        Action<string> thisEvent;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, Action<string> listener)
    {
        if (eventManager == null) return;
        Action<string> thisEvent;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            instance.eventDictionary[eventName] = thisEvent;
        }
    }

    public static void TriggerEvent(string eventName, string message)
    {
        Action<string> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(message);
        }
        else
        {
            Debug.Log(eventName + "not found");
        }
    }
}
