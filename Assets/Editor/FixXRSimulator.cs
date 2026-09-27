using UnityEditor;
using UnityEngine;

public static class FixXRSimulator
{
    [MenuItem("JoeVR/Fix XR Simulator")]
    public static void Fix()
    {
        GameObject simulator = GameObject.Find("XR Interaction Simulator");
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");

        if (simulator == null || xrOrigin == null)
        {
            Debug.LogError("No se encontró XR Interaction Simulator o XR Origin.");
            return;
        }

        Transform camera = xrOrigin.transform.Find("Camera Offset/Main Camera");
        Transform left = xrOrigin.transform.Find("Camera Offset/Left Controller");
        Transform right = xrOrigin.transform.Find("Camera Offset/Right Controller");

        if (camera == null || left == null || right == null)
        {
            Debug.LogError("No se encontraron las referencias.");
            return;
        }

        Component simulatorComponent =
            simulator.GetComponent("XRInteractionSimulator");

        if (simulatorComponent == null)
        {
            Debug.LogError("No se encontró XRInteractionSimulator.");
            return;
        }

        SerializedObject so = new SerializedObject(simulatorComponent);

        SetReference(so, "m_CameraTransform", camera);
        SetReference(so, "m_LeftControllerTransform", left);
        SetReference(so, "m_RightControllerTransform", right);

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(simulator);

        Debug.Log("XR Interaction Simulator reparado.");
    }

    static void SetReference(
        SerializedObject so,
        string propertyName,
        Object value)
    {
        SerializedProperty property = so.FindProperty(propertyName);

        if (property != null)
            property.objectReferenceValue = value;
    }
}