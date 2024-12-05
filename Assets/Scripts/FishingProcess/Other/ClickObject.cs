using UnityEngine;

/// <summary>
/// Used for playing sounds when clicking objects
/// </summary>
public class ClickObject : MonoBehaviour
{
    [SerializeField] private SoundClipTrigger trigger;

    private void OnMouseDown()
    {
        SoundManager.Instance.PlaySound(trigger);
    }
}
