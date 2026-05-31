using UnityEngine;

/// <summary>
/// Sits on the Player. When the player touches a GameObject tagged "Food",
/// it eats it: the food disappears, the player grows a little, and the score goes up.
/// </summary>
public class FoodCollector : MonoBehaviour
{
    [Tooltip("How much the player grows (added to each axis of its scale) per food eaten.")]
    public float growthPerFood = 0.2f;

    [Tooltip("Read-only: how many foods have been eaten so far.")]
    public int score = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Food"))
        {
            return;
        }

        Destroy(other.gameObject);
        transform.localScale += Vector3.one * growthPerFood;
        score++;
        Debug.Log("Yum! Score: " + score);
    }
}
