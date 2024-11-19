using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LureSelectorEntry : MonoBehaviour
{
    public TMP_Text lureNameField;
    public Button entryBtn;

    public void SetTargetForEntry(GameObject lureToPickUp)
    {
        // Clear existing listeners to prevent duplicates
        entryBtn.onClick.RemoveAllListeners();

        // Add a listener to execute the required actions
        entryBtn.onClick.AddListener(() =>
        {
            FishingLureBox.Instance.CloseLureSelector();
            FishManager.Instance.PickUpLure(lureToPickUp);
        });
    }
}
