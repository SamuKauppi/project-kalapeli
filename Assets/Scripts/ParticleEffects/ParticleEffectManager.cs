using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    public static ParticleEffectManager Instance { get; private set; }

    [SerializeField] private ParticleEffectPlayer[] playersPrefabs;
    private readonly Dictionary<ParticleType, ParticleEffectPlayer> playersDict = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < playersPrefabs.Length; i++)
        {
            playersDict.Add(playersPrefabs[i].particleType, playersPrefabs[i]);
        }
    }

    public void PlayParticleEffect(ParticleType type, Vector3 position, Transform parent = null)
    {
        if (playersDict.TryGetValue(type, out ParticleEffectPlayer player))
        {
            ParticleEffectPlayer effect = Instantiate(player, position, Quaternion.identity, parent);
            effect.PlayEffect();
        }
    }
}
