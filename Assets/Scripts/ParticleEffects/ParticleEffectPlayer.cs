using System.Collections;
using UnityEngine;

public class ParticleEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem player;
    public ParticleType particleType;

    private IEnumerator DestroyWhenEnd()
    {
        yield return new WaitForSeconds(player.main.duration);
        yield return null;
        Destroy(gameObject);
    }

    public void PlayEffect()
    {
        player.Play();
        StartCoroutine(DestroyWhenEnd());
    }
}
