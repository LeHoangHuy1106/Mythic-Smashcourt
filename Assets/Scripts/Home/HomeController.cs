using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeController : MonoBehaviour
{
    [Header("UI References")]
    public Animator uiAnimator;            // animator điều khiển UI
    public Button btnGo;                   // nút Go
    public Transform contentListLevel;     // cha chứa danh sách level
    [SerializeField] private Transition transition; // tham chiếu Transition (optional auto-find)

    [Header("Runtime")]
    public List<ItemLevel> listItemLevel = new List<ItemLevel>();

    private void Awake()
    {
        // Lấy tất cả ItemLevel từ các con của contentListLevel
        listItemLevel.Clear();
        listItemLevel.AddRange(contentListLevel.GetComponentsInChildren<ItemLevel>(true));

        // Thử auto-find Transition nếu chưa gán
        if (transition == null) transition = FindObjectOfType<Transition>(true);
    }

    private void Start()
    {
        // Gắn event cho button Go
        if (btnGo != null)
        {
            btnGo.onClick.RemoveAllListeners();
            btnGo.onClick.AddListener(OnClickGo);
        }

        // Khởi tạo danh sách level (demo: unlock level 0, các level còn lại lock)
        for (int i = 0; i < listItemLevel.Count; i++)
        {
            bool isUnlocked = (i == 0); // ví dụ: chỉ mở khóa level đầu
            int index = i;
            listItemLevel[i].Init(index, isUnlocked, OnClickLevel);
        }
    }

    private void OnClickGo()
    {
        if (uiAnimator != null)
        {
            uiAnimator.SetTrigger("Hide");
        }
    }

    private void OnClickLevel(int levelIndex)
    {
        Debug.Log("Click level: " + levelIndex);

        // Gọi transition load sang LobbyScene
        if (transition != null && transition.gameObject.activeInHierarchy)
        {
            transition.LoadingScene("LobbyScene");
        }
        else
        {
            Debug.LogWarning("Transition not found or inactive. Please assign Transition in the scene.");

        }
    }
}
