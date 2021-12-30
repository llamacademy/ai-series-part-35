using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SmoothAgentMovement))]
public class SmoothAgentMovementEditor : Editor
{
    public void OnSceneGUI()
    {
        SmoothAgentMovement smoothAgentMovement = (SmoothAgentMovement)target;

        if (smoothAgentMovement == null || smoothAgentMovement.PathLocations == null || smoothAgentMovement.PathLocations.Length == 0)
        {
            return;
        }

        Handles.color = Color.white;
        for (int i = 0; i < smoothAgentMovement.PathLocations.Length - 1; i++)
        {
            Handles.DrawLine(smoothAgentMovement.PathLocations[i], smoothAgentMovement.PathLocations[i+1]);
            Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), smoothAgentMovement.PathLocations[i], Quaternion.identity, 0.05f, EventType.Repaint);
            Handles.Label(smoothAgentMovement.PathLocations[i], $"{i}");
        }

        Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), smoothAgentMovement.PathLocations[smoothAgentMovement.PathLocations.Length - 1], Quaternion.identity, 0.05f, EventType.Repaint);
    }
}
