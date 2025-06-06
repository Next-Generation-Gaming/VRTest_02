using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

[ExecuteInEditMode]
public class Marker : MonoBehaviour
{
    [Header("Marker Configuration")]
    [Tooltip("Forward direction for alignment checks")]
    public Vector3 forwardDirection = Vector3.forward;

    [Tooltip("Allowed emitter angles (degrees) relative to forwardDirection")]
    public List<float> allowedAngles = new List<float>() { 0f, 45f, 90f, 135f };

    [Tooltip("Sprite or texture to display on successful alignment")]
    public Texture2D correctImage;

    [Tooltip("Gizmo color for forward direction and center sphere")]
    public Color gizmoColor = Color.cyan;

    [Header("Debug")]
    [Tooltip("Show debug lines for each allowed angle in the Scene view")]
    public bool showAngleGizmos = false;

    private void OnValidate()
    {
        if (allowedAngles == null)
            allowedAngles = new List<float>();
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        // Draw a small sphere at the marker center
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(pos, 0.02f);

        // Draw base forward direction arrow
        Vector3 baseDir = transform.TransformDirection(forwardDirection.normalized) * 0.2f;
        Gizmos.DrawLine(pos, pos + baseDir);
        // Arrowhead lines
        Vector3 rightHead = Quaternion.LookRotation(baseDir) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward * 0.03f;
        Vector3 leftHead  = Quaternion.LookRotation(baseDir) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward * 0.03f;
        Gizmos.DrawLine(pos + baseDir, pos + baseDir + rightHead);
        Gizmos.DrawLine(pos + baseDir, pos + baseDir + leftHead);

        // Draw allowed angle lines
        if (showAngleGizmos && allowedAngles.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (float angle in allowedAngles)
            {
                Quaternion rot   = Quaternion.AngleAxis(angle, transform.up);
                Vector3 angleDir = transform.TransformDirection(rot * forwardDirection.normalized) * 0.2f;
                Gizmos.DrawLine(pos, pos + angleDir);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Marker))]
public class MarkerEditor : Editor
{
    SerializedProperty forwardDirectionProp;
    SerializedProperty allowedAnglesProp;
    SerializedProperty correctImageProp;
    SerializedProperty gizmoColorProp;
    SerializedProperty showAngleGizmosProp;

    private void OnEnable()
    {
        forwardDirectionProp = serializedObject.FindProperty("forwardDirection");
        allowedAnglesProp    = serializedObject.FindProperty("allowedAngles");
        correctImageProp     = serializedObject.FindProperty("correctImage");
        gizmoColorProp       = serializedObject.FindProperty("gizmoColor");
        showAngleGizmosProp  = serializedObject.FindProperty("showAngleGizmos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(forwardDirectionProp);
        EditorGUILayout.PropertyField(allowedAnglesProp, true);
        EditorGUILayout.PropertyField(correctImageProp);
        EditorGUILayout.PropertyField(gizmoColorProp);
        EditorGUILayout.PropertyField(showAngleGizmosProp);

        if (GUILayout.Button("Reset to Defaults"))
        {
            Marker m = (Marker)target;
            m.forwardDirection = Vector3.forward;
            m.allowedAngles    = new List<float>() { 0f, 45f, 90f, 135f };
            m.showAngleGizmos  = false;
            serializedObject.Update();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif