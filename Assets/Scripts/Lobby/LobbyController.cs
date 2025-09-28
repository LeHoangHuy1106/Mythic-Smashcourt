using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button btnBattle;
    [SerializeField] private TextMeshProUGUI txtLevel;   // Text hiển thị "Level X"

    [Header("Skill UI")]
    [SerializeField] private SkillPanel skillPanel; // tham chiếu SkillPanel
    [SerializeField] private Button btnPower;       // nút mở skill Power
    [SerializeField] private Button btnSpeed;       // nút mở skill Speed
    [SerializeField] private Button btnTime;        // nút mở skill Time

    [Header("Transition (reference)")]
    [SerializeField] private Transition transition;   // Kéo thả trong Inspector

    [Header("Config")]
    [SerializeField] private string gameplaySceneName = "GamePlay";

    private void Awake()
    {
        if (btnBattle != null)
        {
            btnBattle.onClick.RemoveAllListeners();
            btnBattle.onClick.AddListener(OnClickBattle);
        }

        if (btnPower != null)
        {
            btnPower.onClick.RemoveAllListeners();
            btnPower.onClick.AddListener(() => OnClickSkill(SkillPanel.Skill.Power));
        }
        if (btnSpeed != null)
        {
            btnSpeed.onClick.RemoveAllListeners();
            btnSpeed.onClick.AddListener(() => OnClickSkill(SkillPanel.Skill.Speed));
        }
        if (btnTime != null)
        {
            btnTime.onClick.RemoveAllListeners();
            btnTime.onClick.AddListener(() => OnClickSkill(SkillPanel.Skill.Time));
        }
    }

    private void OnEnable()
    {
        if (btnBattle) btnBattle.interactable = true;

        // Cập nhật text level
        int currentLevel = (DataGame.Instance != null) ? Mathf.Max(1, DataGame.Instance.Level) : 1;
        if (txtLevel != null)
            txtLevel.text = $"Level {currentLevel}";
    }

    private void OnClickBattle()
    {
        if (btnBattle) btnBattle.interactable = false;
        SettingPanel.Instance.PlaySound(0);
        if (transition != null && transition.gameObject.activeInHierarchy)
        {
            transition.LoadingScene(gameplaySceneName);
        }
        else
        {
            Debug.LogWarning("[LobbyController] Transition chưa được gán — dùng SceneManager fallback.");
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    private void OnClickSkill(SkillPanel.Skill skill)
    {
        SettingPanel.Instance.PlaySound(0);
        if (skillPanel != null)
        {
            skillPanel.Show(skill);
        }
        else
        {
            Debug.LogWarning("[LobbyController] SkillPanel chưa được gán.");
        }
    }
}
