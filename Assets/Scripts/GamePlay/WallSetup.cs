using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class WallSetupTopDown : MonoBehaviour
{
    public Camera cam;

    public Transform topWall;
    public Transform bottomWall;
    public Transform leftWall;
    public Transform rightWall;

    [Header("Config")]
    public float depthFromCamera = 10f;
    public float thickness = 0.5f;
    public float yHeight = 0.2f; // độ cao (chiều dày theo Y)
    public bool updateEveryFrame = true;

    // ---------------- Tombstone Spawn ----------------
    [Header("Tombstone Spawn")]
    public GameObject tombstonePrefab;          // Prefab Tombstone (gồm script Tombstone bạn đã viết)
    public Transform tombParent;                // Parent để chứa các tombstone (optional)

    [Tooltip("Delay cho lần spawn đầu (giây)")]
    public float firstSpawnDelay = 10f;

    [Tooltip("Khoảng thời gian giữa các lần spawn tiếp theo (giây)")]
    public Vector2 respawnIntervalRange = new Vector2(20f, 30f);

    [Header("Spawn Margins")]
    public float marginTop = 0.5f;
    public float marginBottom = 0.5f;
    public float marginLeft = 0.5f;
    public float marginRight = 0.5f;

    private float lastAspect = -1f;
    private float nextSpawnTime = -1f;
    private bool scheduledFirstSpawn = false;

    void Reset() { cam = Camera.main; }
    void OnEnable()
    {
        Apply();

        // Chỉ lên lịch spawn khi đang Play
        if (Application.isPlaying)
        {
            ScheduleFirstSpawn();
        }
    }
    void Start()
    {
        Apply();

        if (Application.isPlaying)
        {
            ScheduleFirstSpawn();
        }
    }

    void Update()
    {
        // Cập nhật size walls nếu cần
        if (updateEveryFrame && cam && (lastAspect != cam.aspect || Application.isEditor))
            Apply();

        // Chỉ xử lý spawn khi đang chạy game
        if (!Application.isPlaying) return;

        // Lên lịch lần đầu nếu chưa (phòng hờ trường hợp Enable không chạy)
        if (!scheduledFirstSpawn)
            ScheduleFirstSpawn();

        // Tick spawn
        if (tombstonePrefab != null && Time.time >= nextSpawnTime)
        {
            TrySpawnTombstone();
            ScheduleNextSpawn();
        }
    }

    // ---------------- Wall placement ----------------
    void Apply()
    {
        if (!cam || !topWall || !bottomWall || !leftWall || !rightWall) return;

        float worldH, worldW;
        if (cam.orthographic)
        {
            worldH = 2f * cam.orthographicSize;
            worldW = worldH * cam.aspect;
        }
        else
        {
            float h = 2f * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * depthFromCamera;
            worldH = h;
            worldW = h * cam.aspect;
        }
        lastAspect = cam.aspect;

        // Root đặt ở mặt phẳng camera
        transform.position = cam.transform.position + cam.transform.forward * depthFromCamera;
        transform.rotation = Quaternion.identity; // vì top-down

        // Vị trí cạnh (top/bottom theo Z, left/right theo X)
        topWall.localPosition = new Vector3(0, 0, worldH / 2f);
        bottomWall.localPosition = new Vector3(0, 0, -worldH / 2f);
        rightWall.localPosition = new Vector3(worldW / 2f, 0, 0);
        leftWall.localPosition = new Vector3(-worldW / 2f, 0, 0);

        // Scale: Y luôn = yHeight, còn lại X/Z
        topWall.localScale = new Vector3(worldW, yHeight, thickness);
        bottomWall.localScale = new Vector3(worldW, yHeight, thickness);
        leftWall.localScale = new Vector3(thickness, yHeight, worldH);
        rightWall.localScale = new Vector3(thickness, yHeight, worldH);
    }

    /// <summary>
    /// Trả về minX, maxX, minZ, maxZ bên trong 4 tường
    /// </summary>
    public List<float> GetLimits()
    {
        List<float> limits = new List<float>();

        // Trái
        float minX = leftWall.localPosition.x + (leftWall.localScale.x * 0.5f);
        // Phải
        float maxX = rightWall.localPosition.x - (rightWall.localScale.x * 0.5f);
        // Dưới
        float minZ = bottomWall.localPosition.z + (bottomWall.localScale.z * 0.5f);
        // Trên
        float maxZ = topWall.localPosition.z - (topWall.localScale.z * 0.5f);

        limits.Add(minX);
        limits.Add(maxX);
        limits.Add(minZ);
        limits.Add(maxZ);

        return limits;
    }

    // ---------------- Spawn helpers ----------------
    private void ScheduleFirstSpawn()
    {
        scheduledFirstSpawn = true;

        float delay = Mathf.Max(0f, firstSpawnDelay);
        nextSpawnTime = Time.time + delay;
    }

    private void ScheduleNextSpawn()
    {
        float min = Mathf.Max(0f, respawnIntervalRange.x);
        float max = Mathf.Max(min, respawnIntervalRange.y);
        nextSpawnTime = Time.time + Random.Range(min, max);
    }

    private void TrySpawnTombstone()
    {
        if (tombstonePrefab == null) return;

        var limits = GetLimits();
        if (limits == null || limits.Count < 4) return;

        float minX = limits[0] + marginLeft;
        float maxX = limits[1] - marginRight;
        float minZ = limits[2] + marginBottom;
        float maxZ = limits[3] - marginTop;

        // Bảo vệ khi margin quá lớn
        if (minX > maxX) { float mid = (limits[0] + limits[1]) * 0.5f; minX = maxX = mid; }
        if (minZ > maxZ) { float mid = (limits[2] + limits[3]) * 0.5f; minZ = maxZ = mid; }

        float x = (minX == maxX) ? minX : Random.Range(minX, maxX);
        float z = (minZ == maxZ) ? minZ : Random.Range(minZ, maxZ);
        Vector3 pos = new Vector3(x, 0f, z);

        Instantiate(tombstonePrefab, pos, Quaternion.identity, tombParent);
    }
}
