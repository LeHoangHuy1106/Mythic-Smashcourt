using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Header : MonoBehaviour
{

    [SerializeField] private bool avoidNotch = false;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        AdjustHeaderPosition();
          
    }

    private void AdjustHeaderPosition()
    {
        Vector2 anchoredPos = rectTransform.anchoredPosition;

        if (avoidNotch)
        {
            anchoredPos.y = -50f;
        }
        else
        {
            anchoredPos.y = -20f;
        }

        rectTransform.anchoredPosition = anchoredPos;

        Debug.Log($"[Header] Set Y = {anchoredPos.y} (AvoidNotch = {avoidNotch})");
    }
}
