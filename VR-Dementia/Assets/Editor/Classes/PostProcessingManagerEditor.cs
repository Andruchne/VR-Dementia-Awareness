using UnityEngine;
using UnityEditor;

// 1. Inherit from Editor, not MonoBehaviour
[CustomEditor(typeof(PostProcessingManager))]
public class PostProcessingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 2. Draw the standard inspector (Fields like volumeCollection, transitionTime)
        DrawDefaultInspector();

        // 3. Get reference to the target script
        PostProcessingManager manager = (PostProcessingManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        // 4. Check if we have a collection to read from
        if (manager != null)
        {
            // 5. Disable buttons if not in Play Mode (optional, but recommended)
            if (Application.isPlaying)
            {
                // Loop through every entry in the ScriptableObject
                foreach (var entry in manager.volumeCollection.entries)
                {
                    // Create a button with the Mood's name
                    if (GUILayout.Button($"Switch to {entry.mood}"))
                    {
                        // Call the method on the manager
                        manager.SwitchMood(entry.mood);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test Mood Transitions.", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please assign a Volume Collection.", MessageType.Warning);
        }
    }
}