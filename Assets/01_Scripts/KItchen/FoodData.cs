using UnityEngine;

[CreateAssetMenu(fileName = "Food_", menuName = "Joe VR/Food Data")]
public class FoodData : ScriptableObject
{
    [Header("Información")]
    public string foodName;
    [Header("Item")]
    public GameObject foodItem;
    [Header("Stats que recupera")]
    [Range(0f, 100f)]
    public float hunger = 20f;

    [Range(0f, 100f)]
    public float health = 0f;

    [Range(0f, 100f)]
    public float energy = 10f;

    [Range(0f, 100f)]
    public float happiness = 5f;

    [Header("Animación")]
    public float eatDuration = 1f;
}