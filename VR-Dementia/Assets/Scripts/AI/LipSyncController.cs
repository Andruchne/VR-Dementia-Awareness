using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Offline processes raw PCM audio data into OVR Lipsync Visemes,
/// and synchronizes the blendshapes with an active FMOD EventInstance.
/// </summary>
public class LipSyncController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer targetMesh;

    [Header("Blendshape Mapping")]
    [Tooltip("IDs of the blendshapes. Order: sil, PP, FF, TH, DD, kk, CH, SS, nn, RR, aa, E, ih, oh, ou")]
    [SerializeField] private int[] visemeToBlendshape = new int[15];

    [Header("Settings")]
    [Range(1f, 30f)]
    [Tooltip("Lerp Speed of Lips")]
    [SerializeField] private float lerpSpeed = 20;

    private uint lipSyncContext = 0;
    private List<OVRLipSync.Frame> cachedFrames = new List<OVRLipSync.Frame>();
    private FMOD.Studio.EventInstance currentAudioInstance;

    private bool isPlaying = false;
    private int sampleRate = 44100;
    private const int FRAME_SIZE = 1024;

    private void Start()
    {
        // FMOD completely disables Unity's built-in audio system, causing OVRLipSync to fail
        // when it automatically queries AudioSettings. We initialize it manually here to bypass this.
        if (OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
        {
            // Manually feed standard fallback values (48000 Hz Sample Rate, 1024 Buffer Size)
            OVRLipSync.Initialize(48000, 1024);
        }

        // Initialize native OVR Lipsync Engine
        if (OVRLipSync.CreateContext(ref lipSyncContext, OVRLipSync.ContextProviders.Enhanced) != OVRLipSync.Result.Success)
        {
            Debug.LogError("Failed to create OVR Lipsync Context.");
        }
    }

    /// <summary>
    /// Converts raw PCM data into OVR Frames for later playback synchronization.
    /// Call this right before playing the FMOD audio.
    /// </summary>
    public void PrepareAndPlayVisemes(byte[] rawPcmData, int audioSampleRate, FMOD.Studio.EventInstance fmodInstance)
    {
        sampleRate = audioSampleRate;
        currentAudioInstance = fmodInstance;
        cachedFrames.Clear();

        // Convert byte[] (16-bit PCM) to short[]
        int shortLength = rawPcmData.Length / 2;
        short[] shortData = new short[shortLength];
        Buffer.BlockCopy(rawPcmData, 0, shortData, 0, rawPcmData.Length);

        // Process data in standard chunk sizes
        for (int i = 0; i < shortData.Length; i += FRAME_SIZE)
        {
            int remaining = shortData.Length - i;
            int currentFrameSize = Mathf.Min(FRAME_SIZE, remaining);

            // Pad the last frame with zeros if it's smaller than FRAME_SIZE to avoid internal OVR errors
            short[] frameData = new short[FRAME_SIZE];
            Array.Copy(shortData, i, frameData, 0, currentFrameSize);

            OVRLipSync.Frame frame = new OVRLipSync.Frame();
            OVRLipSync.ProcessFrame(lipSyncContext, frameData, frame);

            // Clone frame to prevent OVR from recycling and overwriting the reference
            OVRLipSync.Frame clonedFrame = new OVRLipSync.Frame();
            clonedFrame.CopyInput(frame);
            cachedFrames.Add(clonedFrame);
        }

        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying || !currentAudioInstance.isValid() || targetMesh == null) { return; }

        currentAudioInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);

        if (state == FMOD.Studio.PLAYBACK_STATE.STOPPING || state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            isPlaying = false;
            ResetBlendshapes();
            return;
        }

        // Retrieve current timeline position in milliseconds from FMOD
        currentAudioInstance.getTimelinePosition(out int timelinePosMs);

        // Calculate the corresponding cached frame index
        float timeInSeconds = timelinePosMs / 1000f;
        int currentFrameIndex = Mathf.FloorToInt((timeInSeconds * sampleRate) / FRAME_SIZE);

        if (currentFrameIndex >= 0 && currentFrameIndex < cachedFrames.Count)
        {
            UpdateBlendshapes(cachedFrames[currentFrameIndex]);
        }
    }

    private void UpdateBlendshapes(OVRLipSync.Frame frame)
    {
        for (int i = 0; i < visemeToBlendshape.Length; i++)
        {
            int blendshapeIndex = visemeToBlendshape[i];

            // Skip unassigned or invalid blendshapes (-1 handles your 'sil' setup gracefully)
            if (blendshapeIndex >= 0 && blendshapeIndex < targetMesh.sharedMesh.blendShapeCount)
            {
                // Convert OVR weights (0.0 - 1.0) to Unity weights (0 - 100)
                float targetWeight = frame.Visemes[i] * 100f;
                float currentWeight = targetMesh.GetBlendShapeWeight(blendshapeIndex);

                targetMesh.SetBlendShapeWeight(blendshapeIndex, Mathf.Lerp(currentWeight, targetWeight, lerpSpeed * Time.deltaTime));
            }
        }
    }

    private void ResetBlendshapes()
    {
        foreach (int index in visemeToBlendshape)
        {
            if (index >= 0 && index < targetMesh.sharedMesh.blendShapeCount)
            {
                targetMesh.SetBlendShapeWeight(index, 0f);
            }
        }
    }

    private void OnDestroy()
    {
        // Memory cleanup for native Meta code
        if (lipSyncContext != 0)
        {
            OVRLipSync.DestroyContext(lipSyncContext);
        }
    }
}