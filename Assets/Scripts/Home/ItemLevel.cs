using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemLevel : MonoBehaviour
{
    [Header("UI References")]
    public Button btnLevel;          // nút bấm
    public TMP_Text txtLevel;        // text hiển thị level
    public GameObject lockObj;       // icon lock (blocked)
    public GameObject tickDone;      // icon tick (đã hoàn thành)

    [Header("Config")]
    public int indexLevel;           // index 0-based

    public System.Action<int> onClickLevel;

    /// <summary>
    /// state: 0 = blocked (chưa chơi), 1 = current (đang mở), 2 = done (đã pass)
    /// isUnlocked: vẫn giữ để tương thích, nhưng state sẽ quyết định UI cuối cùng.
    /// </summary>
    public void Init(int levelIndex, bool isUnlocked, System.Action<int> onClick, int state = 0)
    {
        indexLevel = levelIndex;
        if (txtLevel) txtLevel.text = (levelIndex + 1).ToString();

        // Chuẩn hoá state vào [0..2]
        state = Mathf.Clamp(state, 0, 2);

        // Suy ra UI theo state
        bool isBlocked = (state == 0);
        bool isCurrent = (state == 1);
        bool isDone = (state == 2);

        // Hiển thị
        if (txtLevel) txtLevel.gameObject.SetActive(!isBlocked); // chỉ hiện số khi current/done
        if (lockObj) lockObj.SetActive(isBlocked);              // block mới hiện lock
        if (tickDone) tickDone.SetActive(isDone);                // done mới hiện tick

        // Click
        onClickLevel = onClick;
        if (btnLevel)
        {
            btnLevel.onClick.RemoveAllListeners();
            btnLevel.interactable = isCurrent; // chỉ current mới click được

            if (isCurrent)
            {
                btnLevel.onClick.AddListener(() =>
                {
                    onClickLevel?.Invoke(indexLevel);
                });
            }
        }
    }
}
