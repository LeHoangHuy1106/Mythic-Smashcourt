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
    public Transform model;

    [Header("Meshes")]
    public Mesh defaultMesh;
    public List<Mesh> meshOptions = new List<Mesh>();

    [Header("UI")]
    public TextMeshProUGUI speedText;

    [Header("Facing Config")]
    public float faceTurnSpeed = 12f;

    [Header("Magic Circle")]
    public GameObject magicCircle;
    public Animator magicAnimator;
    public string magicAnimName = "Play";
    public float magicCircleDuration = 1f;
    private Coroutine magicRoutine;

    [Header("Explosion Prefabs")]
    public GameObject exploreStatue;

    [Header("Motion Lock")]
    [Tooltip("Giữ nguyên tọa độ X (tránh bị đẩy lệch làn).")]
    public bool lockX = true;

    // ===== Runtime =====
    private int currentSpeed = 0;            // tổng lực
    private float balanceFactor = 0.01f;     // hệ số
    private Vector3 desiredDir = Vector3.forward; // hướng mong muốn (XZ, chuẩn hóa)
    private float desiredSpeedMag = 0f;           // |currentSpeed| * balanceFactor
    private float laneX;                          // X ban đầu (khi lockX = true)

    public void SetBalanceFactor(float value) => balanceFactor = Mathf.Max(0.0001f, value);

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

        // chọn mesh
        if (meshOptions != null && meshOptions.Count > 0) meshFilter.mesh = meshOptions[Random.Range(0, meshOptions.Count)];
        else if (defaultMesh) meshFilter.mesh = defaultMesh;

        if (magicCircle) magicCircle.SetActive(false);
        if (magicCircle && magicAnimator == null) magicAnimator = magicCircle.GetComponent<Animator>();

        // lane X để khóa (nếu bật lockX)
        laneX = transform.position.x;

        SetSpeed(0);
        // thiết lập hướng/ vận tốc mong muốn lúc đầu
        SetDesiredMotion(Vector3.forward);
        ApplyDesiredMotionInstant();
    }

    void Update()
    {
        // Xoay model theo velocity hiển thị cho đẹp
        if (rb != null)
        {
            Vector3 v = rb.linearVelocity; v.y = 0f;
            if (v.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(v.normalized, Vector3.up);
                model.rotation = Quaternion.Slerp(model.rotation, target, Time.deltaTime * faceTurnSpeed);
            }
        }

        // Root không xoay
        transform.rotation = Quaternion.identity;
    }

    // >>> Đây là chỗ quan trọng để giữ vận tốc sau mỗi bước vật lý <<<
    void FixedUpdate()
    {
        // 1) Khoá X (tránh bị húc lệch làn)
        if (lockX)
        {
            var p = rb.position;
            p.x = laneX;
            rb.position = p;
        }

        // 2) Áp lại vận tốc mong muốn (bất chấp xô đẩy)
        ApplyDesiredMotionInstant();

        // 3) Không cho xoay do va chạm
        rb.angularVelocity = Vector3.zero;
    }

    private void SetDesiredMotion(Vector3 inDir)
    {
        // chuẩn hóa & ép dấu Z theo currentSpeed
        inDir.y = 0f;
        if (inDir.sqrMagnitude < 1e-6f) inDir = Vector3.forward;
        inDir.Normalize();

        float zSign = Mathf.Sign(currentSpeed);
        if (zSign == 0) zSign = 1f;

        desiredDir = new Vector3(inDir.x, 0f, Mathf.Abs(inDir.z) * zSign).normalized;
        desiredSpeedMag = Mathf.Abs(currentSpeed) * balanceFactor;
    }

    private void ApplyDesiredMotionInstant()
    {
        if (rb == null) return;

        if (currentSpeed == 0 || desiredSpeedMag <= 0f)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = desiredDir * desiredSpeedMag;
        }
    }

    public void SetSpeed(int value)
    {
        currentSpeed = value;

        if (speedText)
        {
            speedText.text = currentSpeed.ToString();
            speedText.color =
                currentSpeed > 0 ? new Color(1f, 0.5f, 0f) :
                currentSpeed < 0 ? new Color(0.6f, 0f, 0.8f) :
                Color.white;
        }

        // Cập nhật biên độ mục tiêu khi speed đổi
        desiredSpeedMag = Mathf.Abs(currentSpeed) * balanceFactor;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ăn bóng → đổi tổng lực + đổi hướng theo góc bóng, nhưng magnitude do mình khống chế
        Ball ball = collision.gameObject.GetComponentInParent<Ball>()
                    ?? collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            SettingPanel.Instance.PlaySound(9);
            SetSpeed(currentSpeed + ball.power);

            Vector3 hitDir = ball.CurrentDirection;
            if (hitDir.sqrMagnitude < 1e-6f) hitDir = Vector3.forward;

            // Update hướng mong muốn
            SetDesiredMotion(hitDir);

            TriggerMagicCircle();
            Destroy(ball.gameObject);
            return;
        }

        // Nếu chạm Tombstone/ vật khác → KHÔNG đổi desiredDir/desiredSpeedMag
        // Vận tốc sẽ bị áp lại trong FixedUpdate, nên không bị “đẩy”.
    }

    // ===== Magic Circle =====
    private void TriggerMagicCircle()
    {
        if (!magicCircle) return;

        magicCircle.SetActive(true);
        if (!magicAnimator) magicAnimator = magicCircle.GetComponent<Animator>();
        if (magicAnimator)
        {
            if (!string.IsNullOrEmpty(magicAnimName)) magicAnimator.Play(magicAnimName, -1, 0f);
            else magicAnimator.Play(0, -1, 0f);
        }
        if (magicRoutine != null) StopCoroutine(magicRoutine);
        magicRoutine = StartCoroutine(MagicCircleAutoHide(magicCircleDuration));
    }

    private IEnumerator MagicCircleAutoHide(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (magicCircle) magicCircle.SetActive(false);
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
                SpawnExplore();
                Destroy(gameObject);
            }
            else if (other.gameObject.name == "line player")
            {
                SettingPanel.Instance.PlaySound(1);
                GameplayView.Instance.IncreaseEnemyScore(Mathf.Abs(currentSpeed));
                SpawnExplore();
                Destroy(gameObject);
            }
        }
    }

    private void SpawnExplore()
    {
        if (exploreStatue) Instantiate(exploreStatue, transform.position, Quaternion.identity);
    }
}
 