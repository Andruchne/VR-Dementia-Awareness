using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class InworldTTSClient : MonoBehaviour
{
    [Header("Inworld TTS Settings")]
    [SerializeField] string englishVoiceId = "default-att0kitmzafhiciftzuddw__hester_voice_clone_en";
    [SerializeField] string dutchVoiceId = "default-att0kitmzafhiciftzuddw__hester_voice_clone_nl";
    [SerializeField] string modelId = "inworld-tts-1.5-mini";

    private string base64AuthToken;
    private string voiceId;

    private void Awake()
    {
        LoadLocalCredentials();
        voiceId = englishVoiceId;
    }

    private void LoadLocalCredentials()
    {
        string authFilePath = "";

        #if UNITY_EDITOR || UNITY_STANDALONE
        // Running on PC (Editor or Windows Build): Use the local user profile path
        string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        authFilePath = Path.Combine(userProfilePath, ".inworld", "auth.json");
        #elif UNITY_ANDROID
        // Running on Meta Quest (Android): Use the persistent data path
        authFilePath = Path.Combine(Application.persistentDataPath, "inworld_auth.json");
        #endif

        if (File.Exists(authFilePath))
        {
            string jsonContent = File.ReadAllText(authFilePath);
            InworldCredentials creds = JsonUtility.FromJson<InworldCredentials>(jsonContent);

            // Assign API key - already encoded in base64
            base64AuthToken = creds.base64Key;

            Debug.Log($"Inworld Base64 token loaded successfully from: {authFilePath}");
        }
        else
        {
            Debug.LogError($"Auth file missing at: {authFilePath}. Please ensure the file is placed correctly for the current platform.");
        }
    }

    /// <summary>
    /// Generates speech and returns the raw PCM bytes directly, bypassing file saving.
    /// Crucial for parallel processing and stitching.
    /// </summary>
    public async Task<byte[]> GenerateSpeechBytes(string textToSpeak)
    {
        if (string.IsNullOrEmpty(base64AuthToken))
        {
            Debug.LogError("Cannot generate speech. Missing auth token.");
            return null;
        }

        // Build the JSON request
        TTSRequest req = new TTSRequest
        {
            text = textToSpeak,
            voiceId = this.voiceId,
            modelId = this.modelId,
            audioConfig = new AudioConfig
            {
                audioEncoding = "LINEAR16",
                sampleRateHertz = 44100
            }
        };

        string jsonPayload = JsonUtility.ToJson(req);

        // Send the HTTP POST request to Inworld's TTS endpoint
        using (UnityWebRequest www = new UnityWebRequest("https://api.inworld.ai/tts/v1/voice", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Basic {base64AuthToken}");

            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Inworld TTS Error: {www.error} - {www.downloadHandler.text}");
                return null;
            }

            // Parse the response and extract the Base64 audio string to byte array
            TTSResponse response = JsonUtility.FromJson<TTSResponse>(www.downloadHandler.text);
            return Convert.FromBase64String(response.audioContent);
        }
    }
}