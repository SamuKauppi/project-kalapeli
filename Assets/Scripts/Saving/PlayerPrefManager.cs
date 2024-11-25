using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefManager : MonoBehaviour
{
    public static PlayerPrefManager Instance { get; private set; }

    [SerializeField] private PlayerPrefEntry[] playerPrefs;

    private readonly Dictionary<string, float> storedFloats = new();
    private readonly Dictionary<string, int> storedInts = new();
    private readonly Dictionary<string, string> storedStrings = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        foreach (PlayerPrefEntry entry in playerPrefs)
        {
            if (!PlayerPrefs.HasKey(entry.keyValue))
            {
                if (entry.floatValue != 0f)
                {
                    PlayerPrefs.SetFloat(entry.keyValue, entry.floatValue);
                }
                if (entry.intValue != 0)
                {
                    PlayerPrefs.SetInt(entry.keyValue, entry.intValue);
                }
                if (entry.stringValue != "")
                {
                    PlayerPrefs.SetString(entry.keyValue, entry.stringValue);
                }
            }

            if (PlayerPrefs.HasKey(entry.keyValue))
            {
                if (IsFloat(entry.keyValue))
                {
                    storedFloats.Add(entry.keyValue, PlayerPrefs.GetFloat(entry.keyValue));
                }
                else if (IsInt(entry.keyValue))
                {
                    storedInts.Add(entry.keyValue, PlayerPrefs.GetInt(entry.keyValue));
                }
                else
                {
                    storedStrings.Add(entry.keyValue, PlayerPrefs.GetString(entry.keyValue));
                }
            }
        }
    }

    private bool IsFloat(string key)
    {
        return float.TryParse(PlayerPrefs.GetString(key, ""), out _);
    }

    private bool IsInt(string key)
    {
        return int.TryParse(PlayerPrefs.GetString(key, ""), out _);
    }


    public void SaveValue<T>(string keyValue, T value)
    {
        if (value is float floatValue)
        {
            storedFloats[keyValue] = floatValue;
            PlayerPrefs.SetFloat(keyValue, floatValue);
        }
        else if (value is int intValue)
        {
            storedInts[keyValue] = intValue;
            PlayerPrefs.SetInt(keyValue, intValue);
        }
        else if (value is string stringValue)
        {
            storedStrings[keyValue] = stringValue;
            PlayerPrefs.SetString(keyValue, stringValue);
        }

        PlayerPrefs.Save();
    }

    public T GetValue<T>(string keyValue, T defaultValue)
    {
        if(!PlayerPrefs.HasKey(keyValue))
            return defaultValue;


        if (typeof(T) == typeof(float))
        {
            return (T)(object)PlayerPrefs.GetFloat(keyValue, Convert.ToSingle(defaultValue));
        }
        else if (typeof(T) == typeof(int))
        {
            return (T)(object)PlayerPrefs.GetInt(keyValue, Convert.ToInt32(defaultValue));
        }
        else if (typeof(T) == typeof(string))
        {
            return (T)(object)PlayerPrefs.GetString(keyValue, Convert.ToString(defaultValue));
        }
        else
        {
            throw new ArgumentException($"Type {typeof(T)} is not supported by PlayerPrefs.");
        }
    }
}
