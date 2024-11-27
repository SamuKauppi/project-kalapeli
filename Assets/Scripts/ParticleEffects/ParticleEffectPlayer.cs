using System.Collections;
using UnityEngine;

public class ParticleEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem player;
    [SerializeField] private float duration = 0f;
    public ParticleType particleType;

    private IEnumerator DestroyOnComplete()
    {
        yield return new WaitForSeconds(duration);

        // Notify the manager to delete the effect
        ParticleEffectManager.Instance.DeleteParticleEffect(particleType);
    }

    public void PlayEffect()
    {
        player.Play();
        if (duration > 0f)
        {
            StartCoroutine(DestroyOnComplete());
        }
    }

    public void StopEffect()
    {
        player.Stop();
        Destroy(gameObject);
    }
}
