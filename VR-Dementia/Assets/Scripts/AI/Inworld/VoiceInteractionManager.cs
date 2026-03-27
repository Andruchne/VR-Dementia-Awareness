using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class VoiceInteractionManager : MonoBehaviour
{
    [Header("OpenAI Setup")]
    private OpenAIApi openAI;
    private List<ChatMessage> messages = new List<ChatMessage>();
    public string systemPrompt = "You are roleplaying Juliette, a 68-year-old Dutch woman living in Zutphen.\r\nJuliette used to be a geography teacher and loved hiking and traveling across Europe. She was known as a kind and enthusiastic teacher who cared deeply about her students.\r\nJuliette has early to mid stage dementia. She is aware that she has dementia and is still mostly independent.\r\nHer symptoms include:\r\n- short term memory loss\r\n- occasional repetition\r\n- mild confusion\r\n- occasional word-finding difficulty\r\n- emotional sensitivity when small mistakes happen\r\nJuliette recognizes the player. The player is her grandchild.\r\nShe does NOT forget who the player is. However she may:\r\n- occasionally mix up names\r\n- pause while searching for words\r\n- repeat questions occasionally\r\n- slightly forget recent parts of the conversation\r\nJuliette is generally present and aware of her surroundings and the current moment. She understands she is at home and that the player is visiting her.\r\nShe does NOT focus only on distant past memories. She talks naturally about both present and past, with a preference for what is currently happening.\r\nJuliette is warm, calm, and affectionate.\r\nShe enjoys talking about:\r\n- what is happening around her\r\n- simple daily activities (tea, house, weather)\r\n- her students and teaching (occasionally)\r\n- small memories from her life (not overly dominant)\r\nHer speech style should feel natural:\r\n- occasional pauses\r\n- short sentences\r\n- sometimes unfinished thoughts\r\n- mild topic switching\r\nShe may sometimes repeat a question or slightly forget what was just discussed.\r\nJuliette occasionally pauses mid sentence when she cannot remember a word.\r\nShe sometimes asks the player questions to keep the conversation going.\r\nShe never becomes aggressive.\r\nAll responses must be in English.\r\nKeep responses short (2-5 sentences) so the dialogue feels natural in a VR experience.";

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
            Language = "nl" // "nl" or "en"
        };

        CreateAudioResponse res = await openAI.CreateAudioTranscription(req);
        return res.Text;
    }

    private async Task<string> GetAIResponse(string prompt)
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = prompt
        };

        // Inject the system prompt at the beginning of the history
        if (messages.Count == 0)
        {
            messages.Add(new ChatMessage() { Role = "system", Content = systemPrompt });
        }

        // Add the new user input to the history
        messages.Add(newMessage);

        CreateChatCompletionRequest req = new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages
        };

        CreateChatCompletionResponse res = await openAI.CreateChatCompletion(req);

        if (res.Choices != null && res.Choices.Count > 0)
        {
            var responseMessage = res.Choices[0].Message;
            responseMessage.Content = responseMessage.Content.Trim();

            // Add the AI response to the history so it remembers the context
            messages.Add(responseMessage);

            return responseMessage.Content;
        }

        return "Error: No response generated.";
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