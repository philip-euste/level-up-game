using UnityEngine;

[System.Serializable]
public class AttackData
{
    public string attackName;
    public float damage = 10f;
    public float knockbackForce = 5f;
    public float upwardForce = 1.5f;
    public float hitstunDuration = 0.3f;
    public float cooldown = 0.4f;

    public float startupTime = 0.1f;
    public float activeTime = 0.15f;
    public float recoveryTime = 0.2f;
}