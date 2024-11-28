using System;
using UnityEngine;

public class PlayerPrefManager : MonoBehaviour
{
    public static PlayerPrefManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
            FishSpecies.Grouchy => SaveValue.gourchies,
            _ => SaveValue.boots,
        };
    }

    public void SavePrefValue<T>(SaveValue keyValue, T value)
    {
        if (value is float floatValue)
        {
            PlayerPrefs.SetFloat(keyValue.ToString(), floatValue);
        }
        else if (value is int intValue)
        {
            PlayerPrefs.SetInt(keyValue.ToString(), intValue);
        }
        else if (value is string stringValue)
        {
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
