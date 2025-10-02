using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Ball : MonoBehaviour
{
    public enum Owner { Player, Enemy }

    [Header("Config")]
    public Owner owner = Owner.Player;
    public string wallTag = "wall";           // tag tường
    public string tombstoneTag = "tombstone"; // tag của tombstone
    public int power = 1;                     // chỉ số power (gán từ Character)

    [Header("Explosion")]
    public GameObject explosionPrefab;        // Prefab hiệu ứng nổ

    [Header("Runtime")]
    public float speed = 10f;
    private Vector3 moveDir = Vector3.zero;

    private void Update()
    {
        if (moveDir != Vector3.zero)
        {
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }

    public void Shoot(Vector3 dir, float newSpeed)
    {
        if (SettingPanel.Instance != null) SettingPanel.Instance.PlaySound(4);

        moveDir = new Vector3(dir.x, 0f, dir.z).normalized;
        speed = newSpeed;
    }

    public Vector3 CurrentDirection => moveDir;

    // ==== Trigger cho line ====
    private void OnTriggerEnter(Collider other)
    {
        string objName = other.gameObject.name;

        if (owner == Owner.Player && objName == "line enemy")
        {
            SpawnExplosion(transform.position, transform.rotation);
            Destroy(gameObject);
            return;
        }
        if (owner == Owner.Enemy && objName == "line player")
        {
            SpawnExplosion(transform.position, transform.rotation);
            Destroy(gameObject);
            return;
        }
    }

    // ==== Collision cho wall + ball + tombstone ====
    private void OnCollisionEnter(Collision collision)
    {
        // Wall → nổ chính nó luôn
        if (collision.gameObject.CompareTag(wallTag))
        {
            SpawnExplosion(transform.position, transform.rotation);
            Destroy(gameObject);
            return;
        }

        // Tombstone
        if (collision.gameObject.CompareTag(tombstoneTag))
        {
            SpawnExplosion(transform.position, transform.rotation);

            var tomb = collision.gameObject.GetComponentInParent<Tombstone>();
            if (tomb != null) Destroy(tomb.gameObject);
            else Destroy(collision.gameObject);

            Destroy(gameObject);
            return;
        }

        // Ball khác
        Ball otherBall = collision.gameObject.GetComponentInParent<Ball>()
                         ?? collision.gameObject.GetComponent<Ball>();

        if (otherBall != null && otherBall != this)
        {
            int myPower = Mathf.Abs(this.power);
            int otherPower = Mathf.Abs(otherBall.power);

            if (myPower > otherPower)
            {
                SpawnExplosion(otherBall.transform.position, otherBall.transform.rotation);
                Destroy(otherBall.gameObject);
            }
            else if (myPower < otherPower)
            {
                SpawnExplosion(transform.position, transform.rotation);
                Destroy(this.gameObject);
            }
            else
            {
                SpawnExplosion(otherBall.transform.position, otherBall.transform.rotation);
                SpawnExplosion(transform.position, transform.rotation);
                Destroy(otherBall.gameObject);
                Destroy(this.gameObject);
            }
        }
    }

    private void SpawnExplosion(Vector3 pos, Quaternion rot)
    {
        if (explosionPrefab != null)
        {
            if (SettingPanel.Instance != null) SettingPanel.Instance.PlaySound(7);
            Instantiate(explosionPrefab, pos, rot);
        }
    }
}
