using UnityEngine;
using UnityEngine.UI;

public class FishCatalogUIEntry : MonoBehaviour
{
    public Button m_button;
    public Image m_selected_image;

    public void DefineFishToDisplay(FishSpecies species)
    {
        m_button.onClick.AddListener(()  =>  FishCatalog.Instance.DisplayFish(species));
    }

    public void SetSelected(bool value)
    {
        m_selected_image.gameObject.SetActive(value);
    }
}