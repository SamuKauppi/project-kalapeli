using System.Collections;
using UnityEngine;

public class ParticleEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem player;
    public ParticleType type;

    private IEnumerator DestroyWhenEnd()
    {
        yield return new WaitForSeconds(player.main.duration);
        yield return null;
        Destroy(gameObject);
    }

    public void PlayEffect(Vector3 position)
    {
        transform.position = position;
        player.Play();
        StartCoroutine(DestroyWhenEnd());
    }
}
