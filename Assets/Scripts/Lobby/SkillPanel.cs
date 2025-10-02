using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillPanel : MonoBehaviour
{
    public enum Skill
    {
        Power,
        Speed,
        Time
    }

    [Header("Refs")]
    [SerializeField] private GameObject bgHint;
    [SerializeField] private GameObject container;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI textTitle;
    [SerializeField] private TextMeshProUGUI textLevel;
    [SerializeField] private TextMeshProUGUI textValue;
    [SerializeField] private TextMeshProUGUI textPrice;
    [SerializeField] private TextMeshProUGUI textCoin;
    [SerializeField] private TextMeshProUGUI txtDes;   // ✅ mô tả skill

    [Header("Icons")]
    [SerializeField] private GameObject iconPower;
    [SerializeField] private GameObject iconSpeed;
    [SerializeField] private GameObject iconTime;

    [Header("Button")]
    [SerializeField] private Button btnUpgrade;
    [SerializeField] private Button btnClose;

    private Skill currentSkill;

    private void Awake()
    {
        if (btnUpgrade)
        {
            btnUpgrade.onClick.RemoveAllListeners();
            btnUpgrade.onClick.AddListener(OnClickUpgrade);
        }
        if (btnClose)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Hide);
        }

        SetCoin(DataGame.Instance.Coin);
        Hide();
    }

    public void Show(Skill skill)
    {
        currentSkill = skill;

        // bật UI
        if (bgHint) bgHint.SetActive(true);
        if (container) container.SetActive(true);
        container.transform.localScale = Vector3.one * 0.4f;
        StartCoroutine(ScaleUp(container.transform, Vector3.one, 0.25f));

        // set title + icon
        iconPower.SetActive(skill == Skill.Power);
        iconSpeed.SetActive(skill == Skill.Speed);
        iconTime.SetActive(skill == Skill.Time);

        switch (skill)
        {
            case Skill.Power:
                textTitle.text = "Power";
                if (txtDes) txtDes.text = "Boosts the ball’s power, making it easier to destroy weaker balls.";
                break;
            case Skill.Speed:
                textTitle.text = "Speed";
                if (txtDes) txtDes.text = "Increases the ball’s movement speed, allowing faster attacks.";
                break;
            case Skill.Time:
                textTitle.text = "Time";
                if (txtDes) txtDes.text = "Reduces cooldown between shots, letting you prepare balls faster.";
                break;
        }


        RefreshUI();
    }

    public void Hide()
    {
        if (bgHint) bgHint.SetActive(false);
        if (container) container.SetActive(false);
    }

    private void RefreshUI()
    {
        if (DataGame.Instance == null) return;

        int index = SkillToIndex(currentSkill);
        int level = DataGame.Instance.GetSubLevel(index);

        if (textLevel) textLevel.text = $"Lv {level}";
        if (textValue) textValue.text = "0"; // TODO: sẽ set theo công thức bạn cung cấp

        int price = GetPrice(level + 1);
        if (textPrice) textPrice.text = price.ToString();
        SetCoin(DataGame.Instance.Coin);
    }

    private void OnClickUpgrade()
    {
        if (DataGame.Instance == null) return;

        int index = SkillToIndex(currentSkill);
        int level = DataGame.Instance.GetSubLevel(index);
        int price = GetPrice(level + 1);

        if (DataGame.Instance.Coin >= price)
        {
            DataGame.Instance.SpendCoin(price);
            DataGame.Instance.SetSubLevel(index, level + 1);
            SettingPanel.Instance.PlaySound(1);

            NotificationNode.Instance?.ShowNotification(
                $"{currentSkill} Upgrade Success!", NotificationType.Success);

            RefreshUI();
        }
        else
        {
            SettingPanel.Instance.PlaySound(3);
            NotificationNode.Instance?.ShowNotification(
                $"Not enough coins!", NotificationType.Error);
        }
    }

    private int SkillToIndex(Skill s)
    {
        return s == Skill.Speed ? 0 :
               s == Skill.Power ? 1 : 2;
    }

    private int GetPrice(int level)
    {
        if (level <= 1) return 5;
        if (level == 2) return 10;
        if (level == 3) return 20;

        int a = 10;  // level 2
        int b = 20;  // level 3
        int c = 0;

        for (int i = 4; i <= level; i++)
        {
            c = (a + b) * 2;
            a = b;
            b = c;
        }
        return b;
    }



    private IEnumerator ScaleUp(Transform target, Vector3 endScale, float duration)
    {
        Vector3 start = target.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = Mathf.SmoothStep(0f, 1f, k);
            target.localScale = Vector3.Lerp(start, endScale, e);
            yield return null;
        }
        target.localScale = endScale;
    }

    public void SetCoin(int coin)
    {
        if (textCoin != null)
            textCoin.text = coin.ToString();
    }
}
