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
        instance.start();

        // Check state
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);

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
