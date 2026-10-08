using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float activeTime = 0.15f;

    void Start()
    {
        Destroy(gameObject, activeTime);
    }
}