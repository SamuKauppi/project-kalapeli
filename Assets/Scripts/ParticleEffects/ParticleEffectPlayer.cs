using System.Collections;
using UnityEngine;

public class ParticleEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem player;
    [SerializeField] private float duration = 0f;
    public ParticleType particleType;

    private IEnumerator DestroyWhenEnd()
    {
        yield return new WaitForSeconds(duration);
        yield return null;

        if (duration > 0f)
        {
            ParticleEffectManager.Instance.DeletePartilceEffect(particleType);
        }
    }

    public void PlayEffect()
    {
        player.Play();
        StartCoroutine(DestroyWhenEnd());
    }
}
