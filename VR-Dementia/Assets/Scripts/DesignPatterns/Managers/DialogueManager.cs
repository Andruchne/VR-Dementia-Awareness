using System;
using System.Collections.Generic;
using UnityEditor.Localization;
using UnityEngine;

// Todo:
// Play sound from within here, if possible

public class DialogueManager : MonoBehaviour
{
    public event Action<string> OnSubtitleUpdate;
    public event Action OnDialogueFinished;

    public bool IsPlaying { get; private set; }

    private List<string> _dialogueTranscript = new List<string>();

    private LocalizationTableCollection _currentDialogue;
    private int _dialogueEntryCount;
    private int _subtitleProgressionIndex;

    private void Start()
    {
        _dialogueEntryCount = _currentDialogue.SharedData.Entries.Count;
    }

    public void TriggerDialogue(LocalizationTableCollection dialogue)
    {
        _currentDialogue = dialogue;
        _subtitleProgressionIndex = 0;
        SendSubtitlePiece();

        // Play sound here...

        // 
    }

    private void SendSubtitlePiece()
    {
        

        _subtitleProgressionIndex++;
    }
}