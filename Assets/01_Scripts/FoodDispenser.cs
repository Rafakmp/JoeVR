using UnityEngine;

public class FoodDispenser : MonoBehaviour
{
    [Header("Referencias")]

    [Tooltip("Punto donde aparece la comida. Si está vacío, usa este transform.")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("Comidas")]

    [Tooltip("Lista de FoodData que puede soltar.")]
    [SerializeField]
    private FoodData[] foodPool;

    [Tooltip("Fuerza con la que sale la comida (0 = sin impulso).")]
    [SerializeField]
    private float launchForce = 0f;

    [Tooltip("Cooldown entre dispensaciones en segundos.")]
    [SerializeField]
    private float cooldown = 0.4f;

    [Tooltip("Si está activo, no repite la misma comida dos veces seguidas.")]
    [SerializeField]
    private bool avoidRepeat = true;

    // ---------------------------------------------------------

    private float nextDispenseTime = 0f;
    private int lastSpawnedIndex = -1;

    // =========================================================
    // API — llamada desde el botón XR
    // =========================================================

    public void Dispense()
    {
        if (Time.time < nextDispenseTime)
            return;

        if (foodPool == null || foodPool.Length == 0)
        {
            Debug.LogWarning("FoodDispenser: no hay comidas en el pool.");
            return;
        }

        int index = PickRandomIndex();
        FoodData data = foodPool[index];

        if (data == null || data.foodItem == null)
        {
            Debug.LogWarning(
                $"FoodDispenser: FoodData en índice {index} está vacío.");
            return;
        }

        lastSpawnedIndex = index;

        Transform origin = spawnPoint != null ? spawnPoint : transform;

        GameObject item = Instantiate(
            data.foodItem,
            origin.position,
            Random.rotation
        );

        FoodItem foodItem = item.GetComponent<FoodItem>();

        if (foodItem != null)
            foodItem.Setup(data);
        else
            Debug.LogWarning(
                $"FoodDispenser: el prefab '{data.foodItem.name}' " +
                $"no tiene el script FoodItem.");

        if (launchForce > 0f)
        {
            Rigidbody rb = item.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddForce(origin.up * launchForce, ForceMode.Impulse);
        }

        nextDispenseTime = Time.time + cooldown;

        Debug.Log($"FoodDispenser: dispensado '{data.foodName}'.");
    }

    // =========================================================
    // INTERNO
    // =========================================================

    private int PickRandomIndex()
    {
        if (foodPool.Length == 1)
            return 0;

        int index = Random.Range(0, foodPool.Length);

        if (avoidRepeat && index == lastSpawnedIndex)
            index = (index + 1) % foodPool.Length;

        return index;
    }
}