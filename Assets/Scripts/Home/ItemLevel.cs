using UnityEngine;
using UnityEngine.UI;
using TMPro; // nếu bạn dùng TextMeshPro

public class ItemLevel : MonoBehaviour
{
    [Header("UI References")]
    public Button btnLevel;          // nút bấm
    public TMP_Text txtLevel;        // text hiển thị level
    public GameObject lockObj;       // gameobject lock

    [Header("Config")]
    public int indexLevel;           // index level (level thứ mấy)

    // Event cho button (có thể gắn ngoài hoặc trong code)
    public System.Action<int> onClickLevel;

    /// <summary>
    /// Hàm khởi tạo thông tin cho ItemLevel
    /// </summary>
    /// <param name="levelIndex">số thứ tự level</param>
    /// <param name="isUnlocked">trạng thái mở khóa</param>
    /// <param name="onClick">hàm gọi khi click</param>
    public void Init(int levelIndex, bool isUnlocked, System.Action<int> onClick)
    {
        indexLevel = levelIndex;
        txtLevel.text = (levelIndex + 1).ToString(); // hiển thị số level

        // Set active đối nhau: nếu mở khóa thì text hiển thị, lock tắt
        txtLevel.gameObject.SetActive(isUnlocked);
        lockObj.SetActive(!isUnlocked);

        onClickLevel = onClick;

        // Xóa listener cũ để tránh bị chồng nhiều lần
        btnLevel.onClick.RemoveAllListeners();
        btnLevel.onClick.AddListener(() =>
        {
            if (isUnlocked)
            {
                onClickLevel?.Invoke(indexLevel);
            }
        });
    }
}
