using UnityEngine;
using System.Threading.Tasks;
using OpenAI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class VoiceInteractionManager : MonoBehaviour
{
    [Header("OpenAI Setup")]
    private OpenAIApi openAI;

    [Header("Inworld Setup")]
    public InworldTTSClient inworldTTS;

    [Header("Audio Setup")]
    public AudioSource audioSource;
    private string microphoneName;
    private AudioClip recordedClip;
    private bool isRecording = false;

    private void Start()
    {
        openAI = new OpenAIApi();
        audioSource = GetComponent<AudioSource>();

        if (Microphone.devices.Length > 0)
        {
            microphoneName = Microphone.devices[0];
            Debug.Log($"Using Microphone: {microphoneName}");
        }
        else { Debug.LogError("No microphone found!"); }
    }

    private void Update()
    {
        // Make sure a keyboard is actually connected/detected first
        if (Keyboard.current == null) return;

        // Use the New Input System to check if the Spacebar was pressed this frame
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecordingAndProcess();
            }
        }
    }

    public void StartRecording()
    {
        if (microphoneName == null) return;

        isRecording = true;

        // Record up to 10 seconds of audio at 44100 Hz
        recordedClip = Microphone.Start(microphoneName, false, 10, 44100);
        Debug.Log("Recording started...");
    }

    public async void StopRecordingAndProcess()
    {
        if (!isRecording) return;

        Microphone.End(microphoneName);
        isRecording = false;
        Debug.Log("Recording stopped. Processing STT...");

        // Process STT (OpenAI Whisper)
        string userText = await TranscribeAudio(recordedClip);
        Debug.Log($"User said: {userText}");

        // Generate AI Text Response (OpenAI GPT)
        string aiResponseText = await GetAIResponse(userText);
        Debug.Log($"AI Response: {aiResponseText}");

        // Process TTS (Inworld)
        await PlayInworldTTS(aiResponseText);
    }

    private async Task<string> TranscribeAudio(AudioClip clip)
    {
        // OpenAI Whisper requires a byte array of the audio
        CreateAudioTranscriptionsRequest req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = SaveWav.Save(clip), Name = "audio.wav" },
            Model = "whisper-1",
            Language = "en"
        };

        CreateAudioResponse res = await openAI.CreateAudioTranscription(req);
        return res.Text;
    }

    private async Task<string> GetAIResponse(string prompt)
    {
        CreateChatCompletionRequest req = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = new System.Collections.Generic.List<ChatMessage>()
            {
                new ChatMessage() { Role = "user", Content = prompt }
            }
        };

        CreateChatCompletionResponse res = await openAI.CreateChatCompletion(req);
        return res.Choices[0].Message.Content;
    }

    private async Task PlayInworldTTS(string textToSpeak)
    {
        Debug.Log("Sending text to Inworld TTS...");

        AudioClip speechClip = await inworldTTS.GenerateSpeech(textToSpeak);

        if (speechClip != null)
        {
            audioSource.clip = speechClip;
            audioSource.Play();
            Debug.Log("Inworld TTS Audio Playing!");
        }
        else
        {
            Debug.LogError("Failed to generate or play TTS audio.");
        }
    }
}