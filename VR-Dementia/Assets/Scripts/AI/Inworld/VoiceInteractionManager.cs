using FMODUnity;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class VoiceInteractionManager : MonoBehaviour
{
    [Header("OpenAI Setup")]
    private OpenAIApi openAI;
    private List<ChatMessage> messages = new List<ChatMessage>();

    [TextArea(5, 10)]
    [SerializeField] private string systemPrompt = "You are roleplaying Juliette, a 68-year-old Dutch woman living in Zutphen.\r\nJuliette used to be a geography teacher and loved hiking and traveling across Europe. She was known as a kind and enthusiastic teacher who cared deeply about her students.\r\nJuliette has early to mid stage dementia. She is aware that she has dementia and is still mostly independent.\r\nHer symptoms include:\r\n- short term memory loss\r\n- occasional repetition\r\n- mild confusion\r\n- occasional word-finding difficulty\r\n- emotional sensitivity when small mistakes happen\r\nJuliette recognizes the player. The player is her grandchild.\r\nShe does NOT forget who the player is. However she may:\r\n- occasionally mix up names\r\n- pause while searching for words\r\n- repeat questions occasionally\r\n- slightly forget recent parts of the conversation\r\nJuliette is generally present and aware of her surroundings and the current moment. She understands she is at home and that the player is visiting her.\r\nShe does NOT focus only on distant past memories. She talks naturally about both present and past, with a preference for what is currently happening.\r\nJuliette is warm, calm, and affectionate.\r\nShe enjoys talking about:\r\n- what is happening around her\r\n- simple daily activities (tea, house, weather)\r\n- her students and teaching (occasionally)\r\n- small memories from her life (not overly dominant)\r\nHer speech style should feel natural:\r\n- occasional pauses\r\n- short sentences\r\n- sometimes unfinished thoughts\r\n- mild topic switching\r\nShe may sometimes repeat a question or slightly forget what was just discussed.\r\nJuliette occasionally pauses mid sentence when she cannot remember a word.\r\nShe sometimes asks the player questions to keep the conversation going.\r\nShe never becomes aggressive.\r\nAll responses must be in English.\r\nKeep responses short (2-5 sentences) so the dialogue feels natural in a VR experience.";

    // Stores the current language code for Whisper STT
    private string sttLanguage = "en";

    [Header("Inworld Setup")]
    [SerializeField] private InworldTTSClient inworldTTS;

    [Header("FMOD Mic Recording Setup")]
    private FMOD.Sound micSound;
    private int nativeRate;
    private int nativeChannels;
    private int recordDeviceId = 0; // Default microphone
    private const int MAX_RECORDING_SECONDS = 10;
    private bool isRecording = false;

    [Header("FMOD Playback Setup")]
    [SerializeField] private EventReference fmodDialogueEvent;
    private FMOD.Studio.EventInstance dialogueInstance;
    private GCHandle stringHandle;

    // For showing off progress during processing
    public event Action OnProcessingStarted;
    public event Action OnProcessingFinished;

    public bool IsRecording => isRecording;

    public bool IsSpeaking
    {
        get
        {
            if (!dialogueInstance.isValid()) return false;
            dialogueInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
            return state == FMOD.Studio.PLAYBACK_STATE.PLAYING || state == FMOD.Studio.PLAYBACK_STATE.STARTING;
        }
    }

    private IEnumerator Start()
    {
        openAI = new OpenAIApi();

        #if UNITY_ANDROID
        // Request microphone permission on Android (Meta Quest) devices
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
        }
        #endif

        // Wait for the Unity Localization system to finish initializing
        yield return LocalizationSettings.InitializationOperation;

        // Subscribe to the built-in Unity Localization change event
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;

        // Force an initial sync with the currently active language at startup
        if (LocalizationSettings.SelectedLocale != null)
        {
            HandleLocalizationChanged(LocalizationSettings.SelectedLocale);
        }
    }

    private void Update()
    {
        // Guard clause to ensure a keyboard is connected
        if (Keyboard.current == null) return;

        // Toggle recording state when the spacebar is pressed
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

    public void DiscardRecording()
    {
        if (!isRecording) return;
        isRecording = false;

        // Stop FMOD without processing the data
        RuntimeManager.CoreSystem.recordStop(recordDeviceId);
        Debug.Log("Recording discarded.");
    }

    public void StartRecording()
    {
        // Delaying initialization ensures the user had time to accept Android Mic permissions
        if (!micSound.hasHandle())
        {
            InitFMODMicrophone();
            if (!micSound.hasHandle()) { return; } // Abort if initialization failed
        }

        isRecording = true;

        // Start recording into our custom micSound object (false = no looping)
        RuntimeManager.CoreSystem.recordStart(recordDeviceId, micSound, false);
        Debug.Log("FMOD Recording started...");
    }

    public async void StopRecordingAndProcess()
    {
        if (!isRecording) return;
        isRecording = false;

        OnProcessingStarted?.Invoke();

        // Retrieve the current recording position before stopping
        RuntimeManager.CoreSystem.getRecordPosition(recordDeviceId, out uint recordPosition);

        // Stop the FMOD recording process
        RuntimeManager.CoreSystem.recordStop(recordDeviceId);
        Debug.Log($"FMOD Recording stopped. Recorded {recordPosition} samples. Processing STT...");

        // Extract the raw PCM bytes
        byte[] pcmData = GetRecordedPCMData(recordPosition);

        if (pcmData == null || pcmData.Length == 0)
        {
            Debug.LogError("Failed to extract audio data from FMOD. Position was likely 0.");
            OnProcessingFinished?.Invoke(); // Fehler, also beenden wir das "Processing"
            return;
        }

        // Process Speech-To-Text (OpenAI Whisper)
        string userText = await TranscribeAudio(pcmData);
        Debug.Log($"User said: {userText}");

        // Generate AI Text Response (OpenAI GPT)
        string aiResponseText = await GetAIResponse(userText);
        Debug.Log($"AI Response: {aiResponseText}");

        // Process Text-To-Speech (Inworld)
        await PlayInworldTTS(aiResponseText);

        OnProcessingFinished?.Invoke();
    }

    private void HandleLocalizationChanged(Locale newLocale)
    {
        // We use StartsWith to catch all variations of a language
        if (newLocale.Identifier.Code.StartsWith("en"))
        {
            sttLanguage = "en";
            // Specifically replace the response rule to keep her a "Dutch woman" in the prompt
            systemPrompt = systemPrompt.Replace("All responses must be in Dutch.", "All responses must be in English.");
            Debug.Log($"Language switched to English (Code: {newLocale.Identifier.Code}).");
        }
        else if (newLocale.Identifier.Code.StartsWith("nl"))
        {
            sttLanguage = "nl";
            systemPrompt = systemPrompt.Replace("All responses must be in English.", "All responses must be in Dutch.");
            Debug.Log($"Language switched to Dutch (Code: {newLocale.Identifier.Code}).");
        }
    }

    private void InitFMODMicrophone()
    {
        // Check if FMOD detects any input drivers
        RuntimeManager.CoreSystem.getRecordNumDrivers(out int numDrivers, out _);
        if (numDrivers == 0)
        {
            Debug.LogError("No recording devices found by FMOD! Check your Microphone permissions.");
            return;
        }

        // Get hardware details from the default microphone
        RuntimeManager.CoreSystem.getRecordDriverInfo(recordDeviceId, out string name, 128, out _, out nativeRate, out _, out nativeChannels, out _);
        Debug.Log($"Initialized FMOD Mic: {name} | Rate: {nativeRate} | Channels: {nativeChannels}");

        // Prepare an FMOD struct to allocate memory for the recording
        FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        exinfo.numchannels = nativeChannels;
        exinfo.format = FMOD.SOUND_FORMAT.PCM16;
        exinfo.defaultfrequency = nativeRate;
        exinfo.length = (uint)(nativeRate * sizeof(short) * nativeChannels * MAX_RECORDING_SECONDS);

        // Create the custom sound object in memory
        RuntimeManager.CoreSystem.createSound("", FMOD.MODE.DEFAULT | FMOD.MODE.OPENUSER, ref exinfo, out micSound);
    }

    private byte[] GetRecordedPCMData(uint recordPosition)
    {
        // Abort if no data was recorded
        if (recordPosition == 0) return null;

        // Calculate the file size in bytes (Samples * 2 bytes per short * number of channels)
        uint lengthInBytes = recordPosition * sizeof(short) * (uint)nativeChannels;

        // Lock the FMOD memory area to safely read the data
        FMOD.RESULT result = micSound.@lock(0, lengthInBytes, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2);

        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"Failed to lock FMOD sound buffer: {result}");
            return null;
        }

        byte[] audioBytes = new byte[len1 + len2];

        // Copy the data from the unmanaged FMOD memory to our managed C# byte array
        if (len1 > 0) Marshal.Copy(ptr1, audioBytes, 0, (int)len1);
        if (len2 > 0) Marshal.Copy(ptr2, audioBytes, (int)len1, (int)len2);

        // Unlock the memory area
        micSound.unlock(ptr1, ptr2, len1, len2);

        return audioBytes;
    }

    private async Task<string> TranscribeAudio(byte[] pcmData)
    {
        // Convert the raw PCM data into a valid WAV format
        byte[] wavData = SaveWav.SaveFromPCM16(pcmData, nativeRate, nativeChannels);

        CreateAudioTranscriptionsRequest req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = wavData, Name = "audio.wav" },
            Model = "whisper-1",
            Language = sttLanguage
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

        // Inject the system prompt if the conversation history is empty
        if (messages.Count == 0)
        {
            messages.Add(new ChatMessage() { Role = "system", Content = systemPrompt });
        }
        else
        {
            // Update the system prompt in the history dynamically if language changed mid-conversation
            if (messages[0].Role == "system")
            {
                var updatedSystemMessage = messages[0];
                updatedSystemMessage.Content = systemPrompt;
                messages[0] = updatedSystemMessage;
            }
        }

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

            // Append the AI response to the history to maintain context
            messages.Add(responseMessage);

            return responseMessage.Content;
        }

        return "Error: No response generated.";
    }

    private async Task PlayInworldTTS(string textToSpeak)
    {
        Debug.Log("Sending text to Inworld TTS...");

        // Retrieve the local file path of the generated audio
        string audioFilePath = await inworldTTS.GenerateSpeech(textToSpeak);

        if (!string.IsNullOrEmpty(audioFilePath))
        {
            PlayFMODProgrammerSound(audioFilePath);
        }
        else
        {
            Debug.LogError("Failed to generate TTS audio.");
        }
    }

    private void PlayFMODProgrammerSound(string filePath)
    {
        // Stop and clean up any currently playing dialogue instance
        if (dialogueInstance.isValid())
        {
            dialogueInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            dialogueInstance.release();
        }

        dialogueInstance = RuntimeManager.CreateInstance(fmodDialogueEvent);

        // Pin the file path string in memory to prevent the garbage collector from moving it
        stringHandle = GCHandle.Alloc(filePath, GCHandleType.Pinned);
        dialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

        // Assign callbacks to handle the creation and destruction of the programmer sound
        dialogueInstance.setCallback(ProgrammerSoundCallback,
            FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND |
            FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND);

        dialogueInstance.start();
        dialogueInstance.release(); // Safe to release here, FMOD manages its lifetime
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT ProgrammerSoundCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        // Retrieve the pinned string pointer from the event instance
        instance.getUserData(out IntPtr stringPtr);

        GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
        string audioFilePath = stringHandle.Target as string;

        switch (type)
        {
            case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    // Load the audio file directly into an FMOD Sound object
                    FMOD.RESULT result = RuntimeManager.CoreSystem.createSound(audioFilePath, FMOD.MODE.DEFAULT | FMOD.MODE.CREATESTREAM, out FMOD.Sound dialogueSound);

                    if (result == FMOD.RESULT.OK)
                    {
                        // Assign the sound handle to the programmer instrument parameter
                        parameter.sound = dialogueSound.handle;
                        parameter.subsoundIndex = -1;
                        Marshal.StructureToPtr(parameter, parameterPtr, false);
                    }
                    break;
                }
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                {
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    // Reconstruct the FMOD Sound object and release it from memory
                    var sound = new FMOD.Sound(parameter.sound);
                    sound.release();

                    // Free the pinned string handle
                    stringHandle.Free();
                    break;
                }
        }
        return FMOD.RESULT.OK;
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks when the object is destroyed
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;

        // Ensure the FMOD recording sound buffer is released when the object is destroyed
        if (micSound.hasHandle())
        {
            micSound.release();
        }
    }
}