using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FoodItem : MonoBehaviour
{
    [Header("Food Data")]
    [SerializeField] private FoodData foodData;

    [Header("Joe")]
    [SerializeField] private PouStats joeStats;

    [Header("Eating")]
    [SerializeField] private float eatDistance = 0.8f;

    private bool eaten;

    private void Update()
    {
        if (eaten)
            return;

        if (joeStats == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            joeStats.transform.position
        );

        if (distance <= eatDistance)
        {
            Eat();
        }
    }

    private void Eat()
    {
        if (eaten)
            return;

        eaten = true;

        if (foodData != null)
        {
            joeStats.Feed(foodData.hunger);
            joeStats.RestoreHealth(foodData.health);
            joeStats.RestoreEnergy(foodData.energy);
            joeStats.AddHappiness(foodData.happiness);
        }

        PouExpressionController expression =
            joeStats.GetComponent<PouExpressionController>();

        if (expression != null)
        {
            float duration = 1f;

            if (foodData != null)
                duration = foodData.eatDuration;

            expression.PlayEatAnimation(duration);
        }

        Destroy(gameObject);
    }
}