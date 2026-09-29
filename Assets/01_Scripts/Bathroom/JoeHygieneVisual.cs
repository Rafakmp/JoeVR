using UnityEngine;

/// <summary>
/// Tints Joe brown when dirty and restores the original color when clean.
/// </summary>
[DisallowMultipleComponent]
public class JoeHygieneVisual : MonoBehaviour
{
    [SerializeField] private PouStats pouStats;
    [SerializeField] private Color dirtyTint = new Color(0.42f, 0.28f, 0.16f, 1f);

    private Renderer[] renderers;
    private Color[] originalColors;

    private void Awake()
    {
        if (pouStats == null)
            pouStats = GetComponent<PouStats>();

        renderers = GetComponentsInChildren<Renderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = ReadColor(renderers[i]);
    }

    private void Update()
    {
        if (pouStats == null || renderers == null)
            return;

        float cleanT = Mathf.Clamp01(pouStats.cleanliness / 100f);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            WriteColor(renderers[i], Color.Lerp(dirtyTint, originalColors[i], cleanT));
        }
    }

    private static Color ReadColor(Renderer renderer)
    {
        if (renderer == null || renderer.sharedMaterial == null)
            return Color.white;

        Material material = renderer.sharedMaterial;
        if (material.HasProperty("_BaseColor"))
            return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color"))
            return material.GetColor("_Color");
        return Color.white;
    }

    private static void WriteColor(Renderer renderer, Color color)
    {
        Material material = renderer.material;
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
    }
}
