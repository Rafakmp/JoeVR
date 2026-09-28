using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class JoeBathroomSampleSceneSetup
{
    private const string ScenePath = "Assets/00_Scenes/SampleScene.unity";
    private const string JoePath = "Assets/02_Sprites/Joe.fbx";

    [MenuItem("JoeVR/Configurar baùo de prueba en SampleScene")]
    public static void SetupSampleScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject oldRoot = GameObject.Find("Joe Bathroom Demo");
        if (oldRoot != null)
            Object.DestroyImmediate(oldRoot);

        GameObject root = new GameObject("Joe Bathroom Demo");
        Vector3 bathPosition = GetBathPosition();
        CreateBath(root.transform, bathPosition);
        PouStats joe = CreateJoe(root.transform, bathPosition);
        CreateShowerHead(root.transform, bathPosition, joe);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (!Application.isBatchMode)
            Selection.activeGameObject = root;

        Debug.Log("JoeVR: baùo de prueba configurado en SampleScene.");
    }

    private static Vector3 GetBathPosition()
    {
        Transform origin = FindXrOrigin();
        if (origin != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(origin.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f)
                forward = Vector3.forward;
            return origin.position + forward * 1.7f;
        }

        Camera camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null)
            return new Vector3(0.4f, 0f, 1.6f);

        Vector3 camForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
        if (camForward.sqrMagnitude < 0.01f)
            camForward = Vector3.forward;
        return camera.transform.position + camForward * 1.7f - Vector3.up * camera.transform.position.y;
    }

    private static Transform FindXrOrigin()
    {
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name.Contains("XR Origin"))
                return transform;
        }

        return null;
    }

    private static void CreateBath(Transform parent, Vector3 position)
    {
        GameObject bath = new GameObject("Bathtub");
        bath.transform.SetParent(parent);
        CreateBlock("Bath Base", bath.transform, position + Vector3.up * 0.12f, new Vector3(1.6f, 0.22f, 1.1f));
        CreateBlock("Bath Left", bath.transform, position + new Vector3(-0.68f, 0.38f, 0f), new Vector3(0.2f, 0.52f, 1.1f));
        CreateBlock("Bath Right", bath.transform, position + new Vector3(0.68f, 0.38f, 0f), new Vector3(0.2f, 0.52f, 1.1f));
        CreateBlock("Bath Back", bath.transform, position + new Vector3(0f, 0.38f, 0.46f), new Vector3(1.6f, 0.52f, 0.18f));
        CreateBlock("Bath Front", bath.transform, position + new Vector3(0f, 0.28f, -0.46f), new Vector3(1.6f, 0.32f, 0.18f));
    }

    private static PouStats CreateJoe(Transform parent, Vector3 bathPosition)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(JoePath);
        GameObject joeObject = prefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
            : GameObject.CreatePrimitive(PrimitiveType.Capsule);

        joeObject.name = "Joe";
        joeObject.transform.SetParent(parent);
        joeObject.transform.SetPositionAndRotation(
            bathPosition + new Vector3(0f, 0.48f, 0f),
            Quaternion.Euler(-100f, 0f, 0f));
        joeObject.transform.localScale = Vector3.one * 0.1f;

        Rigidbody body = joeObject.GetComponent<Rigidbody>();
        if (body == null)
            body = joeObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        BoxCollider hitCollider = joeObject.GetComponent<BoxCollider>();
        if (hitCollider == null)
            hitCollider = joeObject.AddComponent<BoxCollider>();
        hitCollider.center = new Vector3(0.055f, 0.53f, 0.69f);
        hitCollider.size = new Vector3(7.45f, 4.7f, 6.56f);
        hitCollider.isTrigger = false;

        PouStats stats = joeObject.GetComponent<PouStats>();
        if (stats == null)
            stats = joeObject.AddComponent<PouStats>();

        SerializedObject statsSo = new SerializedObject(stats);
        statsSo.FindProperty("cleanliness").floatValue = 0f;
        statsSo.FindProperty("dirtyAfterSeconds").floatValue = 10f;
        statsSo.FindProperty("cleanRate").floatValue = 12f;
        statsSo.ApplyModifiedPropertiesWithoutUndo();

        JoeHygieneVisual visual = joeObject.GetComponent<JoeHygieneVisual>();
        if (visual == null)
            visual = joeObject.AddComponent<JoeHygieneVisual>();
        SerializedObject visualSo = new SerializedObject(visual);
        SerializedProperty statsProp = visualSo.FindProperty("pouStats");
        if (statsProp != null)
            statsProp.objectReferenceValue = stats;
        visualSo.ApplyModifiedPropertiesWithoutUndo();

        return stats;
    }

    private static void CreateShowerHead(Transform parent, Vector3 bathPosition, PouStats joe)
    {
        GameObject shower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shower.name = "Showerhead (Grab + Trigger)";
        shower.transform.SetParent(parent);

        Vector3 showerPosition = bathPosition + new Vector3(0.35f, 1.15f, -0.55f);
        Vector3 joeTarget = bathPosition + new Vector3(0f, 0.7f, 0f);
        Vector3 sprayDirection = (joeTarget - showerPosition).normalized;
        shower.transform.SetPositionAndRotation(
            showerPosition,
            Quaternion.FromToRotation(Vector3.up, sprayDirection));
        shower.transform.localScale = new Vector3(0.08f, 0.18f, 0.08f);

        SphereCollider extraSphere = shower.GetComponent<SphereCollider>();
        if (extraSphere != null)
            Object.DestroyImmediate(extraSphere);

        GameObject nozzle = new GameObject("Nozzle");
        nozzle.transform.SetParent(shower.transform, false);
        nozzle.transform.localPosition = Vector3.up;
        nozzle.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        Rigidbody body = shower.GetComponent<Rigidbody>();
        if (body == null)
            body = shower.AddComponent<Rigidbody>();
        body.mass = 0.35f;
        body.useGravity = true;
        body.isKinematic = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        XRGrabInteractable grab = shower.GetComponent<XRGrabInteractable>();
        if (grab == null)
            grab = shower.AddComponent<XRGrabInteractable>();
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        grab.interactionLayers = 1;

        ShowerHeadController controller = shower.GetComponent<ShowerHeadController>();
        if (controller == null)
            controller = shower.AddComponent<ShowerHeadController>();
        controller.Configure(nozzle.transform, joe);

        SerializedObject controllerSo = new SerializedObject(controller);
        SerializedProperty nozzleProp = controllerSo.FindProperty("nozzle");
        SerializedProperty joeProp = controllerSo.FindProperty("joe");
        if (nozzleProp != null)
            nozzleProp.objectReferenceValue = nozzle.transform;
        if (joeProp != null)
            joeProp.objectReferenceValue = joe;
        controllerSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateBlock(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = scale;
        return block;
    }
}
