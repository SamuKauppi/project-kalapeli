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
            if (!PlayerPrefs.HasKey(entry.keyValue.ToString()))
            {
                if (entry.floatValue != 0f)
                {
                    PlayerPrefs.SetFloat(entry.keyValue.ToString(), entry.floatValue);
                }
                if (entry.intValue != 0)
                {
                    PlayerPrefs.SetInt(entry.keyValue.ToString(), entry.intValue);
                }
                if (entry.stringValue != "")
                {
                    PlayerPrefs.SetString(entry.keyValue.ToString(), entry.stringValue);
                }
            }

            if (PlayerPrefs.HasKey(entry.keyValue.ToString()))
            {
                if (IsFloat(entry.keyValue.ToString()))
                {
                    storedFloats.Add(entry.keyValue.ToString(), PlayerPrefs.GetFloat(entry.keyValue.ToString()));
                }
                else if (IsInt(entry.keyValue.ToString()))
                {
                    storedInts.Add(entry.keyValue.ToString(), PlayerPrefs.GetInt(entry.keyValue.ToString()));
                }
                else
                {
                    storedStrings.Add(entry.keyValue.ToString(), PlayerPrefs.GetString(entry.keyValue.ToString()));
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
    private SaveValue SaveValueFromFish(FishSpecies species)
    {
        return species switch
        {
            FishSpecies.Dipfish => SaveValue.dipfishes,
            FishSpecies.Bobber => SaveValue.bobbers,
            FishSpecies.Fry => SaveValue.fries,
            FishSpecies.Pickley => SaveValue.pickleys,
            FishSpecies.Boot => SaveValue.boots,
            FishSpecies.Muddle => SaveValue.muddlers,
            FishSpecies.Peeper => SaveValue.peepers,
            _ => SaveValue.boots,
        };
    }

    public void SavePrefValue<T>(SaveValue keyValue, T value)
    {
        if (value is float floatValue)
        {
            storedFloats[keyValue.ToString()] = floatValue;
            PlayerPrefs.SetFloat(keyValue.ToString(), floatValue);
        }
        else if (value is int intValue)
        {
            storedInts[keyValue.ToString()] = intValue;
            PlayerPrefs.SetInt(keyValue.ToString(), intValue);
        }
        else if (value is string stringValue)
        {
            storedStrings[keyValue.ToString()] = stringValue;
            PlayerPrefs.SetString(keyValue.ToString(), stringValue);
        }

        PlayerPrefs.Save();
    }

    public T GetPrefValue<T>(SaveValue keyValue, T defaultValue)
    {
        if (!PlayerPrefs.HasKey(keyValue.ToString()))
            return defaultValue;


        if (typeof(T) == typeof(float))
        {
            return (T)(object)PlayerPrefs.GetFloat(keyValue.ToString(), Convert.ToSingle(defaultValue));
        }
        else if (typeof(T) == typeof(int))
        {
            return (T)(object)PlayerPrefs.GetInt(keyValue.ToString(), Convert.ToInt32(defaultValue));
        }
        else if (typeof(T) == typeof(string))
        {
            return (T)(object)PlayerPrefs.GetString(keyValue.ToString(), Convert.ToString(defaultValue));
        }
        else
        {
            throw new ArgumentException($"Type {typeof(T)} is not supported by PlayerPrefs.");
        }
    }

    public T GetFishValue<T>(FishSpecies species, T defaultValue)
    {
        SaveValue keyValue = SaveValueFromFish(species);
        return GetPrefValue(keyValue, defaultValue);
    }

    public void SaveFishValue<T>(FishSpecies species, T value)
    {
        SaveValue keyValue = SaveValueFromFish(species);
        SavePrefValue(keyValue, value);
    }
}
