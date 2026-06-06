using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayVoice : MonoBehaviour
{
    private static PlayVoice _activeInstance;

    private LipSyncController lipSyncController;
    private FMOD.DSP_READ_CALLBACK dspCallback;
    private FMOD.Studio.EventInstance currentInstance;

    void Start()
    {
        EventBus<OnJulietteTalk>.OnEvent += PlayVoiceSound;
        lipSyncController = FindFirstObjectByType<LipSyncController>();
    }

    private void OnDestroy()
    {
        EventBus<OnJulietteTalk>.OnEvent -= PlayVoiceSound;
    }

    private void PlayVoiceSound(OnJulietteTalk evt)
    {
        if (!evt.phrase.IsNull)
        {
            // Stop current voice if it is already playing
            StopVoiceSound();

            StartCoroutine(PlayAudioRoutine(evt.phrase));
        }
    }

    public void StopVoiceSound()
    {
        // Stop the instance immediately if valid
        if (currentInstance.isValid())
        {
            currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    private IEnumerator PlayAudioRoutine(EventReference eventRef)
    {
        // Create instance and play audio
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);

        // Check validity
        if (!instance.isValid())
        {
            EventBus<OnJulietteFinishedTalk>.Publish(new OnJulietteFinishedTalk());
            yield break;
        }

        // Set 3D attributes
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));

        currentInstance = instance;
        _activeInstance = this;

        // Setup DSP Description
        FMOD.DSP dsp = new FMOD.DSP();
        FMOD.DSP_DESCRIPTION dspDesc = new FMOD.DSP_DESCRIPTION();
        dspDesc.name = new byte[32];
        System.Text.Encoding.UTF8.GetBytes("LipSyncDSP", 0, "LipSyncDSP".Length, dspDesc.name, 0);

        // Critical Fix: Define inputs/outputs so FMOD doesn't bypass the DSP
        dspDesc.numinputbuffers = 1;
        dspDesc.numoutputbuffers = 1;

        dspCallback = new FMOD.DSP_READ_CALLBACK(CaptureDSPRead);
        dspDesc.read = dspCallback;

        FMOD.ChannelGroup coreChannelGroup = new FMOD.ChannelGroup();
        bool dspAttached = false;

        instance.start();

        // Check state
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);

        // Wait for FMOD to start the event
        while (state == PLAYBACK_STATE.STOPPED)
        {
            yield return null;
            instance.getPlaybackState(out state);
        }

        // Wait for channel group registration
        while (instance.getChannelGroup(out coreChannelGroup) != FMOD.RESULT.OK)
        {
            yield return null;
        }

        // Attach DSP to channel group
        if (RuntimeManager.CoreSystem.createDSP(ref dspDesc, out dsp) == FMOD.RESULT.OK)
        {
            coreChannelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, dsp);
            dspAttached = true;
        }

        while (state != PLAYBACK_STATE.STOPPED && state != PLAYBACK_STATE.STOPPING)
        {
            yield return null;
            instance.getPlaybackState(out state);
        }

        // Clean up DSP
        if (dspAttached && dsp.hasHandle())
        {
            coreChannelGroup.removeDSP(dsp);
            dsp.release();
        }

        _activeInstance = null;

        // Remove instance from memory as soon as finished
        instance.release();
        EventBus<OnJulietteFinishedTalk>.Publish(new OnJulietteFinishedTalk());
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.DSP_READ_CALLBACK))]
    private static FMOD.RESULT CaptureDSPRead(ref FMOD.DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
    {
        int samples = (int)(length * inchannels);
        float[] buffer = new float[samples];

        // Copy audio data from FMOD memory
        Marshal.Copy(inbuffer, buffer, 0, samples);

        // Pass it through to the output so the audio is actually audible
        Marshal.Copy(buffer, 0, outbuffer, samples);

        if (_activeInstance != null && _activeInstance.lipSyncController != null)
        {
            // NEW: Wir übergeben direkt das float Array. Das spart Rechenleistung 
            // und vermeidet die unsaubere short-Konvertierung.
            _activeInstance.lipSyncController.EnqueueLiveAudio(buffer, inchannels == 2, _activeInstance.currentInstance);
        }

        return FMOD.RESULT.OK;
    }
}