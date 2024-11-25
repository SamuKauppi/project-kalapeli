using System.Collections;
using UnityEngine;

public class ParticleEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem player;
    [SerializeField] private float duration = 0f;
    public ParticleType particleType;

    private bool isDestroyWhenNotPlayingRunning = false;

    private IEnumerator DisableWhenComplete()
    {
        yield return new WaitForSeconds(duration);

        // Notify the manager to delete the effect
        ParticleEffectManager.Instance.DeleteParticleEffect(particleType);
    }

    private IEnumerator DestroyWhenNotPlaying()
    {
        isDestroyWhenNotPlayingRunning = true;

        // Wait until the particle system stops emitting
        while (player.isEmitting || player.isPlaying)
        {
            yield return null;
        }

        // Destroy the GameObject after the particle system is done
        Destroy(gameObject);

        isDestroyWhenNotPlayingRunning = false;
    }

    public void PlayEffect()
    {
        player.Play();
        if (duration > 0f)
        {
            StartCoroutine(DisableWhenComplete());
        }
    }

    public void StopEffect()
    {
        player.Stop();
        if (!isDestroyWhenNotPlayingRunning)
        {
            StartCoroutine(DestroyWhenNotPlaying());
        }
    }
}
