using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Statue : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Collider col;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform model;                // nếu null sẽ dùng chính transform

    [Header("Meshes")]
    public Mesh defaultMesh;
    public List<Mesh> meshOptions = new List<Mesh>();

    [Header("UI")]
    public TextMeshProUGUI speedText;

    [Header("Facing Config")]
    public float faceTurnSpeed = 12f;      // tốc độ xoay mượt theo vận tốc

    [Header("Magic Circle")]
    public GameObject magicCircle;         // GameObject chứa Animator (mặc định inactive)
    public Animator magicAnimator;         // nếu null sẽ tự lấy từ magicCircle
    public string magicAnimName = "Play";  // tên clip/State để chạy
    public float magicCircleDuration = 1f; // thời gian hiện hiệu ứng (giây)
    private Coroutine magicRoutine;

    [Header("Explosion Prefabs")]
    public GameObject exploreStatue;       // prefab hiệu ứng nổ khi statue biến mất

    // Tổng lực tích lũy
    private int currentSpeed = 0;
    private Vector3 lastMoveDir = Vector3.forward;
    private float balanceFactor = 0.01f;

    public void SetBalanceFactor(float value)
    {
        balanceFactor = Mathf.Max(0.0001f, value);
    }

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!col) col = GetComponent<Collider>();
        if (!meshFilter) meshFilter = GetComponent<MeshFilter>();
        if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
        if (!model) model = transform;

        if (meshOptions != null && meshOptions.Count > 0)
            meshFilter.mesh = meshOptions[Random.Range(0, meshOptions.Count)];
        else if (defaultMesh != null)
            meshFilter.mesh = defaultMesh;

        if (magicCircle != null) magicCircle.SetActive(false);
        if (magicCircle != null && magicAnimator == null)
            magicAnimator = magicCircle.GetComponent<Animator>();

        SetSpeed(0);
        ApplyVelocity(Vector3.forward);
    }

    void Update()
    {
        if (rb != null)
        {
            Vector3 v = rb.linearVelocity; v.y = 0f;
            if (v.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(v.normalized, Vector3.up);
                model.rotation = Quaternion.Slerp(model.rotation, target, Time.deltaTime * faceTurnSpeed);
            }
        }

        transform.rotation = Quaternion.identity;
    }

    public void SetSpeed(int value)
    {
        currentSpeed = value;

        if (speedText != null)
        {
            speedText.text = currentSpeed.ToString();

            if (currentSpeed > 0)
                speedText.color = new Color(1f, 0.5f, 0f);
            else if (currentSpeed < 0)
                speedText.color = new Color(0.6f, 0f, 0.8f);
            else
                speedText.color = Color.white;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Ball ball = collision.gameObject.GetComponentInParent<Ball>()
                    ?? collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            SettingPanel.Instance.PlaySound(9);
            int newSpeed = currentSpeed + ball.power;
            SetSpeed(newSpeed);

            Vector3 hitDir = ball.CurrentDirection;
            hitDir.y = 0f;
            if (hitDir.sqrMagnitude < 1e-6f)
                hitDir = Vector3.forward;
            hitDir.Normalize();

            ApplyVelocity(hitDir);
            
            TriggerMagicCircle();
            Destroy(ball.gameObject);
            return;
        }
    }

    private void ApplyVelocity(Vector3 dir)
    {
        if (rb == null) return;

        if (currentSpeed == 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f)
            dir = Vector3.forward;

        dir.Normalize();

        float zSign = Mathf.Sign(currentSpeed);
        if (zSign == 0) zSign = 1f;
        dir = new Vector3(dir.x, 0f, Mathf.Abs(dir.z) * zSign).normalized;

        rb.linearVelocity = dir * (Mathf.Abs(currentSpeed) * balanceFactor);
        model.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    // ===== Magic Circle helpers =====
    private void TriggerMagicCircle()
    {
        if (magicCircle == null) return;

        magicCircle.SetActive(true);

        if (magicAnimator == null)
            magicAnimator = magicCircle.GetComponent<Animator>();

        if (magicAnimator != null)
        {
            if (!string.IsNullOrEmpty(magicAnimName))
                magicAnimator.Play(magicAnimName, -1, 0f);
            else
                magicAnimator.Play(0, -1, 0f);
        }

        if (magicRoutine != null) StopCoroutine(magicRoutine);
        magicRoutine = StartCoroutine(MagicCircleAutoHide(magicCircleDuration));
    }

    private IEnumerator MagicCircleAutoHide(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (magicCircle != null) magicCircle.SetActive(false);
        magicRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("line"))
        {
            if (other.gameObject.name == "line enemy")
            {
                SettingPanel.Instance.PlaySound(8);
                GameplayView.Instance.IncreasePlayerScore(Mathf.Abs(currentSpeed));
                Debug.Log("[Statue] enemy +point");
                SpawnExplore();
                Destroy(gameObject);
            }
            else if (other.gameObject.name == "line player")
            {
                SettingPanel.Instance.PlaySound(1);
                GameplayView.Instance.IncreaseEnemyScore(Mathf.Abs(currentSpeed));
                Debug.Log("[Statue] player +point");
                SpawnExplore();
                Destroy(gameObject);
            }
        }
    }

    private void SpawnExplore()
    {
        if (exploreStatue != null)
        {
            Instantiate(exploreStatue, transform.position, Quaternion.identity);
        }
    }
}
