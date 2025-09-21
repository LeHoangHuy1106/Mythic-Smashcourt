using UnityEngine;

public class AutoDestroy : MonoBehaviour
{

    [SerializeField] private float lifeTime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
