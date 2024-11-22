using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    public static ParticleEffectManager Instance { get; private set; }

    [SerializeField] private ParticleEffectPlayer[] players;
    private Dictionary<ParticleType, ParticleEffectPlayer> playersDict = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        foreach (ParticleEffectPlayer player in players)
        {
            playersDict[player.type] = player;
        }
    }

    public void PlayParticleEffect(ParticleType type, Vector3 position)
    {
        if (playersDict.TryGetValue(type, out ParticleEffectPlayer player))
        {
            player.PlayEffect(position);
        }
    }
}
