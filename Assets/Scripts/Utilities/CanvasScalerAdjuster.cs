using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerAdjuster : MonoBehaviour
{
    public CanvasScaler canvasScaler;

    void Start()
    {
        Debug.Log("AdjustCanvasScaler script is running!");
        AdjustCanvasScaler();
    }

    void AdjustCanvasScaler()
    {
        if (canvasScaler == null)
        {
            canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler == null) return;
        }

        float width = Screen.width;
        float height = Screen.height;
        float ratio = width / height;
        float dpi = Screen.dpi;

        Debug.Log($"[CanvasScalerAdjuster] Resolution: {width}x{height} | Ratio: {ratio} | DPI: {dpi}");

        bool isTablet = false;

        // Kiểm tra bằng DPI
        if (dpi > 0 && dpi < 200)
        {
            isTablet = true;
        }
        // Hoặc theo kích thước thực tế
        else if (width >= 2000 && height >= 1500)
        {
            isTablet = true;
        }

        if (isTablet)
        {
            canvasScaler.matchWidthOrHeight = 0f;
            Debug.Log("[CanvasScalerAdjuster] Detected: Tablet/iPad → match = 0 (Height)");
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 1f;
            Debug.Log("[CanvasScalerAdjuster] Detected: Phone → match = 1 (Width)");
        }
    }

}
