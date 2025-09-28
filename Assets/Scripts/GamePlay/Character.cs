using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    [Header("References")]
    public GameObject model;
    public GameObject ballPrefab;
    public Transform parentBall;

    [Header("Rotation Config (xoay qua lại quanh Y)")]
    public float angleA = -30f;
    public float angleB = 30f;

    // --- Runtime config (set từ GamePlayController/SubLevels) ---
    [Header("Debug/Runtime Values (read-only at runtime)")]
    [SerializeField] private float ballSpeed;
    [SerializeField] private float spawnCooldown;
    [SerializeField] private float respawnDelay;
    [SerializeField] private float rotateTime;
    [SerializeField] private int ballPower;

    // Enemy only
    [SerializeField] private float enemyShootIntervalMin;
    [SerializeField] private float enemyShootIntervalMax;

    // Freeze spawn config (KHÔNG ảnh hưởng xoay/AI)
    [SerializeField] private float spawnFreezeTime = 0.2f; // thời gian tạm ngưng spawn sau khi bắn

    // --- State ---
    [SerializeField] private bool isEnemy = false;
    [SerializeField] private bool isPlaying = false;
    [SerializeField] private bool ballReady = false;
    [SerializeField] private bool isPreparing = false;

    private float timer = 0f;
    private bool forward = true;

    private GameObject currentBall;
    private float lastSpawnTime = -999f;
    private float nextEnemyShootTime = Mathf.Infinity;

    // ==================== CONFIG APIs ====================

    /// <summary>Set thời gian tạm ngưng spawn bóng mới sau khi bắn.</summary>
    public void SetFreezeTime(float time)
    {
        this.spawnFreezeTime = Mathf.Max(0f, time);
    }

    public void ConfigurePlayer(float ballSpeed, float spawnCooldown, float respawnDelay, float rotateTime, int ballPower)
    {
        this.ballSpeed = ballSpeed;
        this.spawnCooldown = spawnCooldown;
        this.respawnDelay = respawnDelay;
        this.rotateTime = rotateTime;
        this.ballPower = ballPower;

        isEnemy = false;
    }

    public void ConfigureEnemy(float ballSpeed, float spawnCooldown, float respawnDelay, float rotateTime, int ballPower,
                               float shootIntervalMin, float shootIntervalMax)
    {
        this.ballSpeed = ballSpeed;
        this.spawnCooldown = spawnCooldown;
        this.respawnDelay = respawnDelay;
        this.rotateTime = rotateTime;
        this.ballPower = ballPower;

        this.enemyShootIntervalMin = shootIntervalMin;
        this.enemyShootIntervalMax = shootIntervalMax;

        isEnemy = true;
    }

    // ==================== LIFECYCLE ====================

    public void StartGame()
    {
        timer = 0f;
        forward = true;
        isPlaying = true;

        if (isEnemy) ScheduleNextEnemyShot();

        Debug.Log($"[{name}] START | Enemy={isEnemy} | " +
                  $"ballSpeed={ballSpeed}, spawnCooldown={spawnCooldown}, respawnDelay={respawnDelay}, rotateTime={rotateTime}, ballPower={ballPower}, " +
                  $"enemyShoot=({enemyShootIntervalMin}-{enemyShootIntervalMax}), spawnFreezeTime={spawnFreezeTime}");
    }

    void Update()
    {
        if (!isPlaying || model == null) return;

        // Quay qua lại model (không bị ảnh hưởng bởi freeze spawn)
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(rotateTime > 0f ? (timer / rotateTime) : 1f);
        float yRot = forward ? Mathf.Lerp(angleA, angleB, t) : Mathf.Lerp(angleB, angleA, t);

        var rot = model.transform.localEulerAngles;
        rot.y = yRot;
        model.transform.localEulerAngles = rot;

        if (timer >= rotateTime)
        {
            timer = 0f;
            forward = !forward;
        }

        // Enemy auto shoot: chỉ bắn khi đang có bóng sẵn
        if (isEnemy && ballReady && Time.time >= nextEnemyShootTime)
        {
            ShootBall();
            ScheduleNextEnemyShot();
        }
    }

    // ==================== BALL FLOW ====================

    public void PrepareBall() => PrepareBallInternal(false);

    private void PrepareBallInternal(bool ignoreCooldown)
    {
        if (ballPrefab == null || parentBall == null) return;
        if (isPreparing) return;

        // Tôn trọng cooldown khi được gọi thủ công
        if (!ignoreCooldown && Time.time - lastSpawnTime < spawnCooldown) return;

        lastSpawnTime = Time.time;

        if (currentBall != null) Destroy(currentBall);

        isPreparing = true;
        ballReady = false;

        currentBall = Instantiate(ballPrefab, parentBall);
        currentBall.transform.localPosition = Vector3.zero;
        currentBall.transform.localRotation = Quaternion.identity;
        currentBall.transform.localScale = Vector3.zero;

        // Tắt collider khi nạp để không va chạm sớm
        var col = currentBall.GetComponent<Collider>();
        if (col) col.enabled = false;

        var b = currentBall.GetComponent<Ball>();
        if (b != null)
        {
            b.power = ballPower;
            b.owner = isEnemy ? Ball.Owner.Enemy : Ball.Owner.Player;
        }

        StartCoroutine(ScaleInBall(currentBall, 0.2f));
    }

    private IEnumerator ScaleInBall(GameObject ballObj, float duration)
    {
        float time = 0f;
        while (time < duration && ballObj != null)
        {
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            ballObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, k);
            yield return null;
        }

        if (ballObj != null) ballObj.transform.localScale = Vector3.one;
        ballReady = (ballObj != null);
        isPreparing = false;
    }

    public void ShootBall()
    {
        if (currentBall == null || !ballReady) return;
        if (currentBall.transform.localScale.x < 0.95f) return;

        var shot = currentBall;
        Vector3 worldPos = shot.transform.position;
        Quaternion worldRot = shot.transform.rotation;

        currentBall = null;
        ballReady = false;

        // Giữ nguyên world transform khi tách
        shot.transform.SetParent(null, true);
        shot.transform.position = worldPos;
        shot.transform.rotation = worldRot;

        // Bật collider + tạm bỏ qua va chạm với chủ
        var shotCol = shot.GetComponent<Collider>();
        if (shotCol)
        {
            shotCol.enabled = true;
            StartCoroutine(TempIgnoreOwnerCollisions(shotCol, 0.25f));
        }

        // Hướng bắn theo world forward của model
        Vector3 dir = new Vector3(model.transform.forward.x, 0f, model.transform.forward.z).normalized;
        if (dir.sqrMagnitude > 0f)
            shot.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        Ball b = shot.GetComponent<Ball>();
        if (b != null)
        {
            b.power = ballPower;
            b.Shoot(dir, ballSpeed);
        }

        // Respawn: chỉ chặn nạp bóng mới bằng spawnFreezeTime + respawnDelay (+ cooldown còn thiếu)
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        // 1) Đợi freeze spawn + respawnDelay (đều là cấu hình gameplay)
        float waitA = Mathf.Max(0f, spawnFreezeTime) + Mathf.Max(0f, respawnDelay);
        if (waitA > 0f) yield return new WaitForSeconds(waitA);

        // 2) Tính phần cooldown còn thiếu kể từ lần gọi PrepareBallInternal gần nhất
        float sinceLast = Time.time - lastSpawnTime;
        float remainingCooldown = Mathf.Max(0f, spawnCooldown - sinceLast);
        if (remainingCooldown > 0f) yield return new WaitForSeconds(remainingCooldown);

        // 3) Nạp bóng mới, bỏ qua kiểm tra cooldown vì ta đã tự chờ đủ
        PrepareBallInternal(true);
    }

    private void ScheduleNextEnemyShot()
    {
        float minT = Mathf.Max(0.05f, enemyShootIntervalMin);
        float maxT = Mathf.Max(minT, enemyShootIntervalMax);
        nextEnemyShootTime = Time.time + Random.Range(minT, maxT);
    }

    private IEnumerator TempIgnoreOwnerCollisions(Collider ballCol, float duration)
    {
        if (ballCol == null) yield break;
        var myCols = GetComponentsInChildren<Collider>(true);

        foreach (var c in myCols)
        {
            if (c != null && c.enabled)
                Physics.IgnoreCollision(ballCol, c, true);
        }

        yield return new WaitForSeconds(duration);

        foreach (var c in myCols)
        {
            if (c != null)
                Physics.IgnoreCollision(ballCol, c, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (model != null)
        {
            Gizmos.color = (isEnemy ? Color.red : Color.green);
            Gizmos.DrawRay(model.transform.position, model.transform.forward * 2f);
        }
    }
}
