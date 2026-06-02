using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class PlayVoice : MonoBehaviour
{
    void Start()
    {
        EventBus<OnJulietteTalk>.OnEvent += PlayVoiceSound;
    }

    private void OnDestroy()
    {
        EventBus<OnJulietteTalk>.OnEvent -= PlayVoiceSound;
    }

    private void PlayVoiceSound(OnJulietteTalk evt)
    {
        if (!evt.phrase.IsNull) { StartCoroutine(PlayAudioRoutine(evt.phrase)); }
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

        while (state != PLAYBACK_STATE.STOPPED && state != PLAYBACK_STATE.STOPPING)
        {
            yield return null;
            instance.getPlaybackState(out state);
        }

        // Remove instance from memory as soon as finished
        instance.release();
        EventBus<OnJulietteFinishedTalk>.Publish(new OnJulietteFinishedTalk());
    }
}