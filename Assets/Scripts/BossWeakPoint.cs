using UnityEngine;

// Sits on the boss's glowing core. Arrows that hit this relay the damage to the boss,
// which only counts while the boss is exposed.
public class BossWeakPoint : MonoBehaviour, IDamageable
{
    public Boss boss;
    public void TakeHit(int dmg) { if (boss != null) boss.HitWeakPoint(dmg); }
}
