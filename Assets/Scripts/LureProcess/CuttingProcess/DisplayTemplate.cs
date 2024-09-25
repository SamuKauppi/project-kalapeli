using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTemplate : MonoBehaviour
{
    [SerializeField] private Image templateImage;
    [SerializeField] private Sprite[] templateSprites;

    private void OnEnable()
    {
        BlockRotation.OnRotation += ChangeTemplate;
    }
    private void OnDisable()
    {
        BlockRotation.OnRotation -= ChangeTemplate;
    }

    private void Start()
    {
        ChangeTemplate(0, 0);
    }

    private void ChangeTemplate(int sideRot, int upRot)
    {
        int id = upRot > 0 ? sideRot + (4 * upRot) : sideRot;
        templateImage.sprite = templateSprites[id];
    }
}
