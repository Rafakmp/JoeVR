using TMPro;
using UnityEngine;

public class PouSaveMenu : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Referencias")]

    [SerializeField]
    private PouSaveSystem saveSystem;

    [SerializeField]
    private PouStats pouStats;

    [Tooltip("GameObject raíz de Pou (el que se activa/desactiva).")]
    [SerializeField]
    private GameObject pouRoot;

    [Tooltip("GameObject con el Locomotion del XR Origin.")]
    [SerializeField]
    private GameObject locomotionRoot;

    // ---------------------------------------------------------
    // START
    // ---------------------------------------------------------

    [Header("Menú de inicio")]

    [SerializeField]
    private GameObject startPanel;

    [SerializeField]
    private TMP_Text[] startSlotLabels;

    [SerializeField]
    private TMP_Text startStatusText;

    // ---------------------------------------------------------
    // PAUSA
    // ---------------------------------------------------------

    [Header("Menú de pausa")]

    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private TMP_Text[] pauseSlotLabels;

    [SerializeField]
    private TMP_Text pauseStatusText;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (saveSystem == null)
        {
            saveSystem = FindFirstObjectByType<PouSaveSystem>(
                FindObjectsInactive.Include
            );
        }

        if (pouStats == null)
        {
            pouStats = FindFirstObjectByType<PouStats>(
                FindObjectsInactive.Include
            );
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pouRoot != null)
            pouRoot.SetActive(false);

        OpenStartMenu();
    }

    // =========================================================
    // MENÚ DE INICIO
    // =========================================================

    public void OpenStartMenu()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        RefreshStartSlots();

        SetStartStatus("Selecciona una opción.");

        BlockLocomotion(true);
    }

    public void NewGame()
    {
        if (pouRoot != null)
            pouRoot.SetActive(true);

        if (pouStats != null)
        {
            pouStats.ApplyLoadedData(
                100f, 100f, 100f, 100f, 100f, 1f, 0f
            );
        }

        if (startPanel != null)
            startPanel.SetActive(false);

        SetStartStatus("Nueva partida iniciada.");

        BlockLocomotion(false);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // =========================================================
    // MENÚ DE PAUSA
    // =========================================================

    public void OpenMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        
        if (pouRoot != null)
            pouRoot.SetActive(false);

        RefreshPauseSlots();

        SetPauseStatus("Selecciona una ranura.");

        BlockLocomotion(true);
    }

    public void CloseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

      
        if (pouRoot != null)
            pouRoot.SetActive(true);

        BlockLocomotion(false);
    }

    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshStartSlots()
    {
        RefreshSlotsInArray(startSlotLabels);
    }

    public void RefreshPauseSlots()
    {
        RefreshSlotsInArray(pauseSlotLabels);
    }

    private void RefreshSlotsInArray(TMP_Text[] labels)
    {
        if (saveSystem == null || labels == null) return;

        for (int i = 0; i < labels.Length && i < 3; i++)
        {
            if (labels[i] == null) continue;

            int slot = i + 1;

            if (saveSystem.HasSave(slot))
            {
                labels[i].text =
                    $"RANURA {slot}\n" + saveSystem.GetSaveDate(slot);
            }
            else
            {
                labels[i].text =
                    $"RANURA {slot}\nVACÍA";
            }
        }
    }

    // =========================================================
    // SAVE SLOTS
    // =========================================================

    public void SaveSlot1() { SaveSlot(1); }
    public void SaveSlot2() { SaveSlot(2); }
    public void SaveSlot3() { SaveSlot(3); }

    private void SaveSlot(int slot)
    {
        if (saveSystem == null)
        {
            SetPauseStatus("Sistema de guardado no encontrado.");
            return;
        }

        bool success = saveSystem.SaveSlot(slot);

        SetPauseStatus(success
            ? $"Partida guardada en ranura {slot}."
            : "No se pudo guardar.");

        RefreshPauseSlots();
    }

    // =========================================================
    // LOAD SLOTS
    // =========================================================

    public void LoadSlot1() { LoadSlot(1); }
    public void LoadSlot2() { LoadSlot(2); }
    public void LoadSlot3() { LoadSlot(3); }

    private void LoadSlot(int slot)
    {
        if (saveSystem == null)
        {
            SetPauseStatus("Sistema de guardado no encontrado.");
            return;
        }

        if (!saveSystem.HasSave(slot))
        {
            // Avisa en el panel que esté abierto
            if (startPanel != null && startPanel.activeSelf)
                SetStartStatus($"La ranura {slot} está vacía.");
            else
                SetPauseStatus($"La ranura {slot} está vacía.");

            return;
        }

        if (pouRoot != null)
            pouRoot.SetActive(true);

        bool success = saveSystem.LoadSlot(slot);

        if (success)
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);

            BlockLocomotion(false);
        }
        else
        {
            SetPauseStatus("No se pudo cargar.");
        }
    }

    // =========================================================
    // DELETE
    // =========================================================

    public void DeleteSlot1() { DeleteSlot(1); }
    public void DeleteSlot2() { DeleteSlot(2); }
    public void DeleteSlot3() { DeleteSlot(3); }

    private void DeleteSlot(int slot)
    {
        if (saveSystem == null) return;

        saveSystem.DeleteSlot(slot);
        SetPauseStatus($"Ranura {slot} eliminada.");
        RefreshPauseSlots();
    }

    // =========================================================
    // LOCOMOTION
    // =========================================================

    private void BlockLocomotion(bool blocked)
    {
        if (locomotionRoot != null)
            locomotionRoot.SetActive(!blocked);
    }

    // =========================================================
    // STATUS
    // =========================================================

    private void SetStartStatus(string message)
    {
        if (startStatusText != null)
            startStatusText.text = message;

        Debug.Log($"PouSaveMenu (Start): {message}");
    }

    private void SetPauseStatus(string message)
    {
        if (pauseStatusText != null)
            pauseStatusText.text = message;

        Debug.Log($"PouSaveMenu (Pause): {message}");
    }
}