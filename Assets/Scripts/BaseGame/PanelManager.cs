using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelItem
    {
        public Button button;       // Nút mở panel
        public BasePanel panel;     // Panel tương ứng

    }

    [Header("Panels List")]
    public List<PanelItem> panels = new List<PanelItem>();

    private BasePanel currentPanel;

    private void Start()
    {
        foreach (var item in panels)
        {
            if (item.button != null && item.panel != null)
            {
                item.button.onClick.AddListener(() => ShowPanel(item.panel));
            }



            item.panel.Hide(); // Ẩn hết khi start
        }

        currentPanel = null; // Không panel nào mở
    }

    /// <summary>
    /// Show panel mới, tự ẩn panel cũ nếu có
    /// </summary>
    public void ShowPanel(BasePanel panel)
    {
        if (currentPanel != null && currentPanel != panel)
        {

            currentPanel.Hide();
        }

        panel.Show();
        currentPanel = panel;
    }

    /// <summary>
    /// Ẩn panel cụ thể
    /// </summary>
    public void HidePanel(BasePanel panel)
    {
        if (panel != null)

            panel.Hide();
        if (currentPanel == panel)
        {
            currentPanel = null;
        }
    }
}


