using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

/// <summary>
/// Processes raw PCM audio data into OVR Lipsync Visemes,
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

    [Range(1, 100)]
    [Tooltip("Internal neural network smoothing. 1 = jittery/raw, 100 = extremely smooth (Matches Meta's default 70)")]
    [SerializeField] private int smoothAmount = 70;

    [Header("Live FMOD Audio Tuning")]
    [Range(1f, 50f)]
    [Tooltip("Pusht die FMOD Audio-Amplitude künstlich nach oben, damit die Lippen stark ausschlagen. Standard: 10")]
    [SerializeField] private float liveAudioGain = 10f;

    private uint lipSyncContext = 0;
    private List<OVRLipSync.Frame> cachedFrames = new List<OVRLipSync.Frame>();
    private FMOD.Studio.EventInstance currentAudioInstance;

    private bool isPlaying = false;
    private int sampleRate = 44100;
    private const int FRAME_SIZE = 1024;

    private ConcurrentQueue<float[]> liveAudioQueue = new ConcurrentQueue<float[]>();
    private List<float> liveSampleBuffer = new List<float>();

    private bool isLiveMode = false;
    private bool isLiveStereo = false;
    private OVRLipSync.Frame liveFrame = new OVRLipSync.Frame();

    private void Start()
    {
        if (OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
        {
            OVRLipSync.Initialize(44100, 1024);
        }

        if (OVRLipSync.CreateContext(ref lipSyncContext, OVRLipSync.ContextProviders.Enhanced) != OVRLipSync.Result.Success)
        {
            Debug.LogError("Failed to create OVR Lipsync Context.");
        }
        else
        {
            OVRLipSync.SendSignal(lipSyncContext, OVRLipSync.Signals.VisemeSmoothing, smoothAmount, 0);
        }
    }

    public void PrepareAndPlayVisemes(byte[] rawPcmData, int audioSampleRate, FMOD.Studio.EventInstance fmodInstance)
    {
        isLiveMode = false;
        sampleRate = audioSampleRate;
        currentAudioInstance = fmodInstance;
        cachedFrames.Clear();
        liveSampleBuffer.Clear();

        int shortLength = rawPcmData.Length / 2;
        short[] shortData = new short[shortLength];
        Buffer.BlockCopy(rawPcmData, 0, shortData, 0, rawPcmData.Length);

        for (int i = 0; i < shortData.Length; i += FRAME_SIZE)
        {
            int remaining = shortData.Length - i;
            int currentFrameSize = Mathf.Min(FRAME_SIZE, remaining);

            short[] frameData = new short[FRAME_SIZE];
            Array.Copy(shortData, i, frameData, 0, currentFrameSize);

            OVRLipSync.Frame frame = new OVRLipSync.Frame();
            OVRLipSync.ProcessFrame(lipSyncContext, frameData, frame);

            OVRLipSync.Frame clonedFrame = new OVRLipSync.Frame();
            clonedFrame.CopyInput(frame);
            cachedFrames.Add(clonedFrame);
        }

        isPlaying = true;
    }

    public void EnqueueLiveAudio(float[] pcmData, bool stereo, FMOD.Studio.EventInstance fmodInstance)
    {
        currentAudioInstance = fmodInstance;
        isLiveStereo = stereo;
        isLiveMode = true;
        isPlaying = false;
        liveAudioQueue.Enqueue(pcmData);
    }

    private void Update()
    {
        if (isLiveMode && currentAudioInstance.isValid())
        {
            currentAudioInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);

            if (state == FMOD.Studio.PLAYBACK_STATE.STOPPING || state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                isLiveMode = false;
                ResetBlendshapes();
                liveSampleBuffer.Clear();

                while (liveAudioQueue.TryDequeue(out _)) { }
                return;
            }

            while (liveAudioQueue.TryDequeue(out float[] incomingData))
            {
                liveSampleBuffer.AddRange(incomingData);
            }

            int channels = isLiveStereo ? 2 : 1;
            int requiredSamples = FRAME_SIZE * channels;

            while (liveSampleBuffer.Count >= requiredSamples)
            {
                float[] chunk = new float[requiredSamples];
                liveSampleBuffer.CopyTo(0, chunk, 0, requiredSamples);
                liveSampleBuffer.RemoveRange(0, requiredSamples);

                if (liveAudioGain != 1f)
                {
                    for (int i = 0; i < chunk.Length; i++)
                    {
                        // Fix the audio, so it's not too quiet for lip evaluation
                        chunk[i] = Mathf.Clamp(chunk[i] * liveAudioGain, -1f, 1f);
                    }
                }

                OVRLipSync.ProcessFrame(lipSyncContext, chunk, liveFrame, isLiveStereo);
            }

            UpdateBlendshapes(liveFrame);
            return;
        }

        if (!isPlaying || !currentAudioInstance.isValid() || targetMesh == null) { return; }

        currentAudioInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE stateOffline);

        if (stateOffline == FMOD.Studio.PLAYBACK_STATE.STOPPING || stateOffline == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            isPlaying = false;
            ResetBlendshapes();
            return;
        }

        currentAudioInstance.getTimelinePosition(out int timelinePosMs);

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

            if (blendshapeIndex >= 0 && blendshapeIndex < targetMesh.sharedMesh.blendShapeCount)
            {
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
        if (lipSyncContext != 0)
        {
            OVRLipSync.DestroyContext(lipSyncContext);
        }
    }
}