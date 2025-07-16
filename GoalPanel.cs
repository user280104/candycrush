using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalPanel : MonoBehaviour
{
    public UnityEngine.UI.Image thisImage;
    public Sprite thisSprite;
    public TMP_Text thisText;
    public string thisString;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        thisImage.sprite = thisSprite;
        thisText.text = thisString;

        // Ensure scale and positioning are correct
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one; // Reset scale to 1 on all axes
            rectTransform.anchoredPosition = Vector2.zero; // Reset position relative to the parent
            rectTransform.offsetMin = Vector2.zero; // Reset minimum anchor
            rectTransform.offsetMax = Vector2.zero; // Reset maximum anchor
        }
    }
}
