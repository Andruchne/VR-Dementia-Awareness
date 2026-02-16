using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This script enables user-friendly management of volumes connected to moods
/// It features adaptive enum selection, removing entries that are already contained, and warning for missing references
/// The UI is also made adaptive, to match any screen size
/// </summary>

[CustomEditor(typeof(VolumeCollection))]
public class VolumeCollectionEditor : Editor
{
    private VolumeCollection targetScript;

    private void OnEnable()
    {
        targetScript = (VolumeCollection)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Title
        GUILayout.Space(10);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("Mood Volume Configuration", headerStyle);
        GUILayout.Space(10);

        // Get all moods of enum
        List<Mood> allMoods = Enum.GetValues(typeof(Mood)).Cast<Mood>().ToList();
        // Get all moods currently used in entries
        List<Mood> usedMoods = targetScript.entries.Select(x => x.mood).ToList();

        // Draw Entries
        for (int i = 0; i < targetScript.entries.Count; i++)
        {
            DrawRow(i, allMoods, usedMoods);
        }

        GUILayout.Space(10);

        // Warning for empty references
        DrawWarnings();

        GUILayout.Space(5);

        // Add Button
        if (targetScript.entries.Count < allMoods.Count)
        {
            DrawAddButton(allMoods, usedMoods);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetScript);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRow(int index, List<Mood> allMoods, List<Mood> usedMoods)
    {
        VolumeEntry entry = targetScript.entries[index];

        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

        float moodWidth = rect.width * 0.4f;
        float objWidth = rect.width * 0.4f;
        float btnWidth = rect.width * 0.2f;

        Rect moodRect = new Rect(rect.x, rect.y, moodWidth - 5, rect.height);
        Rect objRect = new Rect(rect.x + moodWidth, rect.y, objWidth - 5, rect.height);
        Rect btnRect = new Rect(rect.x + moodWidth + objWidth, rect.y, btnWidth, rect.height);

        // Adaptive enum selection
        // If the mood is not used in any entry or it's being used for the one selected, add them to the available mood list
        List<Mood> availableForThisRow = allMoods.Where(m => !usedMoods.Contains(m) || m == entry.mood).ToList();
        // UI only takes arrays, so convert the list previously made 
        string[] options = availableForThisRow.Select(x => x.ToString()).ToArray();
        // Get the index of the current selected mood (keep it sorted)
        int currentIndex = availableForThisRow.IndexOf(entry.mood);

        // New mood selected
        int newIndex = EditorGUI.Popup(moodRect, currentIndex, options);

        // Apply changes
        if (newIndex >= 0 && newIndex < availableForThisRow.Count)
        {
            Mood selectedMood = availableForThisRow[newIndex];
            if (selectedMood != entry.mood)
            {
                Undo.RecordObject(targetScript, "Change Mood");
                entry.mood = selectedMood;
            }
        }

        // Volume Field
        GameObject newVol = (GameObject)EditorGUI.ObjectField(objRect, entry.volume, typeof(GameObject), false);
        if (newVol != entry.volume)
        {
            Undo.RecordObject(targetScript, "Change Volume");
            entry.volume = newVol;
        }

        // Remove Button
        if (GUI.Button(btnRect, "Remove"))
        {
            Undo.RecordObject(targetScript, "Remove Entry");
            targetScript.entries.RemoveAt(index);
            GUIUtility.ExitGUI();
        }
    }

    private void DrawWarnings()
    {
        bool hasMissingAssignments = false;

        for (int i = 0; i < targetScript.entries.Count; i++)
        {
            if (targetScript.entries[i].volume == null)
            {
                string moodName = targetScript.entries[i].mood.ToString();
                EditorGUILayout.HelpBox($"Missing Volume for mood: {moodName}", MessageType.Error);
                hasMissingAssignments = true;
            }
        }

        if (hasMissingAssignments)
        {
            GUILayout.Space(5);
        }
    }

    private void DrawAddButton(List<Mood> allMoods, List<Mood> usedMoods)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add New Volume", GUILayout.Width(Screen.width * 0.5f)))
        {
            Mood nextAvailable = allMoods.FirstOrDefault(m => !usedMoods.Contains(m));

            Undo.RecordObject(targetScript, "Add Entry");

            // Create new instance of the class
            VolumeEntry newEntry = new VolumeEntry();
            newEntry.mood = nextAvailable;
            newEntry.volume = null;

            targetScript.entries.Add(newEntry);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}