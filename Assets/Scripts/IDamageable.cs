// Anything an arrow can hit. Zombies take damage directly; the boss only takes damage
// through its weak point, and only while exposed.
public interface IDamageable
{
    void TakeHit(int dmg);
}
