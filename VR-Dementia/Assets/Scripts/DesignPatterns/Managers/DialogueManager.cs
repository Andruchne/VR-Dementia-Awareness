using System;
using System.Collections.Generic;
using UnityEngine;

// Todo:
// Play sound from within here, if possible

public class DialogueManager : MonoBehaviour
{
    public event Action<string> OnSubtitleUpdate;
    public event Action OnDialogueFinished;

    public bool IsPlaying { get; private set; }

    private List<string> _dialogueTranscript = new List<string>();

    private int _dialogueEntryCount;
    private int _subtitleProgressionIndex;


    private void SendSubtitlePiece()
    {
        

        _subtitleProgressionIndex++;
    }
}