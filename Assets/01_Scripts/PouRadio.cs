using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PouRadio : MonoBehaviour
{
    // =========================================================
    // REFERENCIAS
    // =========================================================

    [Header("Referencias")]

    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private SkinnedMeshRenderer radioRenderer;

    [SerializeField]
    private GameObject visualOnWhenPlaying;

    // =========================================================
    // ZONA DE BAILE
    // =========================================================

    [Header("Zona de baile")]

    [SerializeField]
    private BoxCollider danceZone;

    [SerializeField]
    private float checkInterval = 0.2f;

    [SerializeField]
    private bool requireDancerComponent = true;

    // =========================================================
    // PLAYLIST
    // =========================================================

    [Header("Playlist")]

    [SerializeField]
    private AudioClip[] playlist;

    [SerializeField]
    private bool avoidRepeat = true;

    // =========================================================
    // BLENDSHAPES
    // =========================================================

    [Header("Blendshapes de la radio")]

    [SerializeField]
    private string basisShapeName = "Basis";

    [SerializeField]
    private string sizeShapeName = "tamaño";

    [SerializeField]
    private string speakerShapeName = "parlantes";

    // =========================================================
    // REACCIÓN A LA MÚSICA
    // =========================================================

    [Header("Reacción a la música")]

    [Tooltip("Cuánto se amplifica la señal antes de normalizar.")]
    [SerializeField]
    private float amplification = 3f;

    [Tooltip("Velocidad con la que baja el pico de referencia. " +
             "0.99 = lento, 0.9 = rápido.")]
    [Range(0.9f, 0.999f)]
    [SerializeField]
    private float peakDecay = 0.995f;

    [Tooltip("Suavizado del ataque (subida).")]
    [SerializeField]
    private float attackSpeed = 40f;

    [Tooltip("Suavizado de la caída.")]
    [SerializeField]
    private float releaseSpeed = 8f;

    [Tooltip("Valor mínimo del parlante cuando hay música.")]
    [SerializeField]
    private float speakerMinimum = 20f;

    [Tooltip("Multiplicador extra cuando detecta pico de bajos.")]
    [SerializeField]
    private float punchMultiplier = 1.6f;

    [Tooltip("Valor máximo de los blendshapes.")]
    [SerializeField]
    private float maxBlendValue = 100f;

    [Tooltip("Muestra los valores en consola (solo para depurar).")]
    [SerializeField]
    private bool debugValues = false;

    // ---------------------------------------------------------

    private bool isPlaying = false;
    private PouDanceController currentDancer;

    public bool IsPlaying => isPlaying;

    private int basisIndex = -1;
    private int sizeIndex = -1;
    private int speakerIndex = -1;

    private const int SAMPLE_COUNT = 512;
    private float[] spectrumData = new float[SAMPLE_COUNT];

    private float currentSizeBlend = 0f;
    private float currentSpeakerBlend = 0f;

    // Peak tracking para auto-normalización
    private float peakBass = 0.0001f;
    private float peakMids = 0.0001f;

    private int lastPlayedIndex = -1;
    private float nextCheckTime = 0f;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (visualOnWhenPlaying != null)
            visualOnWhenPlaying.SetActive(false);

        if (radioRenderer != null && radioRenderer.sharedMesh != null)
        {
            Mesh mesh = radioRenderer.sharedMesh;

            basisIndex = mesh.GetBlendShapeIndex(basisShapeName);
            sizeIndex = mesh.GetBlendShapeIndex(sizeShapeName);
            speakerIndex = mesh.GetBlendShapeIndex(speakerShapeName);

            if (basisIndex == -1)
                Debug.LogWarning(
                    $"PouRadio: no se encontró '{basisShapeName}'.");

            if (sizeIndex == -1)
                Debug.LogWarning(
                    $"PouRadio: no se encontró '{sizeShapeName}'.");

            if (speakerIndex == -1)
                Debug.LogWarning(
                    $"PouRadio: no se encontró '{speakerShapeName}'.");

            if (basisIndex != -1)
                radioRenderer.SetBlendShapeWeight(basisIndex, 0f);
        }
        else if (radioRenderer == null)
        {
            Debug.LogWarning(
                "PouRadio: radioRenderer no asignado.");
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateRadioVisuals();
        UpdateDanceZone();
    }

    // =========================================================
    // VISUALES CON MÚSICA
    // =========================================================

    private void UpdateRadioVisuals()
    {
        if (musicSource == null || radioRenderer == null)
            return;

        if (!isPlaying || !musicSource.isPlaying)
        {
            currentSizeBlend = Mathf.Lerp(
                currentSizeBlend, 0f, Time.deltaTime * releaseSpeed);

            currentSpeakerBlend = Mathf.Lerp(
                currentSpeakerBlend, 0f, Time.deltaTime * releaseSpeed);

            ApplyBlendshapes(currentSizeBlend, currentSpeakerBlend);
            return;
        }

        // -----------------------------------------------
        // Leer espectro
        // -----------------------------------------------

        musicSource.GetSpectrumData(
            spectrumData, 0, FFTWindow.BlackmanHarris);

        float bass = 0f;
        for (int i = 0; i < 10; i++)
            bass += spectrumData[i];

        float mids = 0f;
        for (int i = 10; i < 40; i++)
            mids += spectrumData[i];

        float highs = 0f;
        for (int i = 40; i < 80; i++)
            highs += spectrumData[i];

        float speakerEnergy = mids + highs * 0.6f;

        // Amplificar
        bass *= amplification;
        speakerEnergy *= amplification;

        // -----------------------------------------------
        // Peak tracking (auto-normalización)
        // -----------------------------------------------

        peakBass = Mathf.Max(bass, peakBass * peakDecay);
        peakMids = Mathf.Max(speakerEnergy, peakMids * peakDecay);

        // Evitar división por cero
        float safePeakBass = Mathf.Max(peakBass, 0.0001f);
        float safePeakMids = Mathf.Max(peakMids, 0.0001f);

        // Normalizar 0-1
        float normBass = Mathf.Clamp01(bass / safePeakBass);
        float normSpeaker = Mathf.Clamp01(speakerEnergy / safePeakMids);

        // -----------------------------------------------
        // Punch cuando hay pico de bajos
        // -----------------------------------------------

        bool bassHit = normBass > 0.85f;

        float targetSize = normBass * maxBlendValue;
        float targetSpeaker = normSpeaker * maxBlendValue;

        if (bassHit)
        {
            targetSize *= punchMultiplier;
            targetSpeaker *= punchMultiplier;
        }

        targetSpeaker = Mathf.Max(targetSpeaker, speakerMinimum);

        targetSize = Mathf.Clamp(targetSize, 0f, maxBlendValue);
        targetSpeaker = Mathf.Clamp(targetSpeaker, 0f, maxBlendValue);

        // -----------------------------------------------
        // Suavizado asimétrico
        // -----------------------------------------------

        float sizeSpeed = targetSize > currentSizeBlend
            ? attackSpeed : releaseSpeed;

        float speakerSpeed = targetSpeaker > currentSpeakerBlend
            ? attackSpeed : releaseSpeed;

        currentSizeBlend = Mathf.Lerp(
            currentSizeBlend, targetSize, Time.deltaTime * sizeSpeed);

        currentSpeakerBlend = Mathf.Lerp(
            currentSpeakerBlend, targetSpeaker, Time.deltaTime * speakerSpeed);

        ApplyBlendshapes(currentSizeBlend, currentSpeakerBlend);

        if (debugValues)
        {
            Debug.Log(
                $"Bass raw: {bass:F5}, " +
                $"peak: {peakBass:F5}, " +
                $"norm: {normBass:F2} | " +
                $"Speaker: {currentSpeakerBlend:F1} | " +
                $"Size: {currentSizeBlend:F1}");
        }
    }

    private void ApplyBlendshapes(float sizeVal, float speakerVal)
    {
        if (basisIndex != -1)
            radioRenderer.SetBlendShapeWeight(basisIndex, 0f);

        if (sizeIndex != -1)
            radioRenderer.SetBlendShapeWeight(sizeIndex, sizeVal);

        if (speakerIndex != -1)
            radioRenderer.SetBlendShapeWeight(speakerIndex, speakerVal);
    }

    // =========================================================
    // ZONA DE BAILE
    // =========================================================

    private void UpdateDanceZone()
    {
        if (danceZone == null) return;

        if (!isPlaying)
        {
            if (currentDancer != null)
            {
                currentDancer.StopDancing();
                currentDancer = null;
            }
            return;
        }

        if (Time.time < nextCheckTime) return;

        nextCheckTime = Time.time + checkInterval;

        Vector3 worldCenter =
            danceZone.transform.TransformPoint(danceZone.center);

        Vector3 halfExtents = Vector3.Scale(
            danceZone.size * 0.5f,
            danceZone.transform.lossyScale
        );

        Collider[] hits = Physics.OverlapBox(
            worldCenter,
            halfExtents,
            danceZone.transform.rotation,
            ~0,
            QueryTriggerInteraction.Collide
        );

        PouDanceController found = null;

        foreach (Collider hit in hits)
        {
            PouDanceController dancer =
                hit.GetComponentInParent<PouDanceController>();

            if (dancer == null) continue;

            found = dancer;
            break;
        }

        if (found != null)
        {
            if (currentDancer != found)
            {
                if (currentDancer != null)
                    currentDancer.StopDancing();

                currentDancer = found;
                currentDancer.StartDancing();
            }
        }
        else
        {
            if (currentDancer != null)
            {
                currentDancer.StopDancing();
                currentDancer = null;
            }
        }
    }

    // =========================================================
    // GIZMO
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (danceZone == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);

        Vector3 worldCenter =
            danceZone.transform.TransformPoint(danceZone.center);

        Vector3 halfExtents = Vector3.Scale(
            danceZone.size * 0.5f,
            danceZone.transform.lossyScale
        );

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            worldCenter,
            danceZone.transform.rotation,
            Vector3.one
        );

        Gizmos.DrawCube(Vector3.zero, halfExtents * 2f);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);

        Gizmos.matrix = oldMatrix;
    }

    // =========================================================
    // API
    // =========================================================

    public void ToggleMusic()
    {
        if (isPlaying) StopMusic();
        else StartMusic();
    }

    public void StartMusic()
    {
        if (isPlaying) return;

        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("PouRadio: la playlist está vacía.");
            return;
        }

        isPlaying = true;

        // Resetear peaks para empezar de cero
        peakBass = 0.0001f;
        peakMids = 0.0001f;

        PlayNextRandom();

        if (visualOnWhenPlaying != null)
            visualOnWhenPlaying.SetActive(true);
    }

    public void StopMusic()
    {
        if (!isPlaying) return;

        isPlaying = false;

        CancelInvoke(nameof(PlayNextRandom));

        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        if (visualOnWhenPlaying != null)
            visualOnWhenPlaying.SetActive(false);

        if (currentDancer != null)
        {
            currentDancer.StopDancing();
            currentDancer = null;
        }
    }

    private void PlayNextRandom()
    {
        int index = PickRandomIndex();

        if (index < 0) return;

        lastPlayedIndex = index;

        musicSource.clip = playlist[index];
        musicSource.loop = false;
        musicSource.Play();

        CancelInvoke(nameof(PlayNextRandom));
        Invoke(nameof(PlayNextRandom), playlist[index].length + 0.05f);

        Debug.Log($"PouRadio: suena '{playlist[index].name}'.");
    }

    private int PickRandomIndex()
    {
        if (playlist.Length == 1) return 0;

        int index = Random.Range(0, playlist.Length);

        if (avoidRepeat && index == lastPlayedIndex)
            index = (index + 1) % playlist.Length;

        return index;
    }
}