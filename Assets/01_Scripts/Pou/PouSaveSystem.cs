using System;
using System.IO;
using UnityEngine;

public class PouSaveSystem : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField]
    private PouStats pouStats;

    private const int MIN_SLOT = 1;
    private const int MAX_SLOT = 3;

    [Serializable]
    private class PouSaveData
    {
        public float hunger;
        public float health;
        public float energy;
        public float happiness;
        public float cleanliness;

        public float age;
        public float ageProgress;

        public string saveDate;
    }

    private void Awake()
    {
        if (pouStats == null)
        {
            pouStats = FindFirstObjectByType<PouStats>(
                FindObjectsInactive.Include
            );
        }
    }

    // =========================================================
    // RUTA
    // =========================================================

    private string GetSavePath(int slot)
    {
        return Path.Combine(
            Application.persistentDataPath,
            $"PouSaveSlot_{slot}.json"
        );
    }

    // =========================================================
    // GUARDAR
    // =========================================================

    public bool SaveSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError(
                $"Slot inválido: {slot}"
            );

            return false;
        }

        if (pouStats == null)
        {
            Debug.LogError(
                "PouSaveSystem: No se encontró PouStats."
            );

            return false;
        }

        PouSaveData data = new PouSaveData();

        data.hunger = pouStats.hunger;
        data.health = pouStats.health;
        data.energy = pouStats.energy;
        data.happiness = pouStats.happiness;
        data.cleanliness = pouStats.cleanliness;

        data.age = pouStats.Age;
        data.ageProgress = pouStats.AgeProgress;

        data.saveDate =
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss"
            );

        string json =
            JsonUtility.ToJson(
                data,
                true
            );

        string path =
            GetSavePath(slot);

        try
        {
            File.WriteAllText(
                path,
                json
            );

            Debug.Log(
                $"Partida guardada correctamente en Slot {slot}\n" +
                path
            );

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Error al guardar Slot {slot}: {ex.Message}"
            );

            return false;
        }
    }

    // =========================================================
    // CARGAR
    // =========================================================

    public bool LoadSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError(
                $"Slot inválido: {slot}"
            );

            return false;
        }

        if (pouStats == null)
        {
            Debug.LogError(
                "PouSaveSystem: No se encontró PouStats."
            );

            return false;
        }

        string path =
            GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning(
                $"No existe ninguna partida en Slot {slot}."
            );

            return false;
        }

        try
        {
            string json =
                File.ReadAllText(path);

            PouSaveData data =
                JsonUtility.FromJson<PouSaveData>(
                    json
                );

            if (data == null)
            {
                Debug.LogError(
                    $"El archivo del Slot {slot} está vacío o corrupto."
                );

                return false;
            }

            pouStats.ApplyLoadedData(
                data.hunger,
                data.health,
                data.energy,
                data.happiness,
                data.cleanliness,
                data.age,
                data.ageProgress
            );

            Debug.Log(
                $"Partida cargada correctamente desde Slot {slot}."
            );

            Debug.Log(
                $"Fecha de guardado: {data.saveDate}"
            );

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Error al cargar Slot {slot}: {ex.Message}"
            );

            return false;
        }
    }

    // =========================================================
    // EXISTE
    // =========================================================

    public bool HasSave(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return false;
        }

        return File.Exists(
            GetSavePath(slot)
        );
    }

    // =========================================================
    // FECHA DE GUARDADO
    // =========================================================

    public string GetSaveDate(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return "FECHA DESCONOCIDA";
        }

        string path =
            GetSavePath(slot);

        if (!File.Exists(path))
        {
            return "SIN DATOS";
        }

        try
        {
            string json =
                File.ReadAllText(path);

            PouSaveData data =
                JsonUtility.FromJson<PouSaveData>(
                    json
                );

            if (data == null ||
                string.IsNullOrEmpty(data.saveDate))
            {
                return "FECHA DESCONOCIDA";
            }

            return data.saveDate;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Error al leer la fecha del Slot {slot}: {ex.Message}"
            );

            return "FECHA DESCONOCIDA";
        }
    }

    // =========================================================
    // ELIMINAR
    // =========================================================

    public void DeleteSlot(int slot)
    {
        if (!IsValidSlot(slot))
        {
            Debug.LogError(
                $"Slot inválido: {slot}"
            );

            return;
        }

        string path =
            GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning(
                $"No existe guardado en Slot {slot}."
            );

            return;
        }

        try
        {
            File.Delete(path);

            Debug.Log(
                $"Slot {slot} eliminado."
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Error al eliminar Slot {slot}: {ex.Message}"
            );
        }
    }

    // =========================================================
    // BOTONES DEL SLOT 1
    // =========================================================

    public void SaveSlot1()
    {
        SaveSlot(1);
    }

    public void LoadSlot1()
    {
        LoadSlot(1);
    }

    public void DeleteSlot1()
    {
        DeleteSlot(1);
    }

    // =========================================================
    // BOTONES DEL SLOT 2
    // =========================================================

    public void SaveSlot2()
    {
        SaveSlot(2);
    }

    public void LoadSlot2()
    {
        LoadSlot(2);
    }

    public void DeleteSlot2()
    {
        DeleteSlot(2);
    }

    // =========================================================
    // BOTONES DEL SLOT 3
    // =========================================================

    public void SaveSlot3()
    {
        SaveSlot(3);
    }

    public void LoadSlot3()
    {
        LoadSlot(3);
    }

    public void DeleteSlot3()
    {
        DeleteSlot(3);
    }

    // =========================================================
    // VALIDACIÓN
    // =========================================================

    private bool IsValidSlot(int slot)
    {
        return slot >= MIN_SLOT &&
               slot <= MAX_SLOT;
    }
}