using UnityEngine;
using UnityEngine.UI;

public class PouUIBars : MonoBehaviour
{
    [Header("Pou")]
    [SerializeField] private PouStats pouStats;

    [Header("Barra Health")]
    [SerializeField] private Image healthFill;

    [Header("Barra Hunger")]
    [SerializeField] private Image hungerFill;

    [Header("Barra Energy")]
    [SerializeField] private Image energyFill;

    [Header("Barra Happiness")]
    [SerializeField] private Image happinessFill;


    private void Start()
    {
        if (pouStats == null)
        {
            Debug.LogError("PouUIBars: No se asignó PouStats.");
            return;
        }

        UpdateBars();
    }


    private void Update()
    {
        if (pouStats == null)
            return;

        UpdateBars();
    }


    private void UpdateBars()
    {
        // Todas tus estadísticas están entre 0 y 100,
        // por eso simplemente dividimos entre 100.

        if (healthFill != null)
            healthFill.fillAmount = pouStats.health / 100f;

        if (hungerFill != null)
            hungerFill.fillAmount = pouStats.hunger / 100f;

        if (energyFill != null)
            energyFill.fillAmount = pouStats.energy / 100f;

        if (happinessFill != null)
            happinessFill.fillAmount = pouStats.happiness / 100f;
    }
}