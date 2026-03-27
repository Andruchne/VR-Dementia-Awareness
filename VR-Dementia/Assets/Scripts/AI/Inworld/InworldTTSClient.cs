using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class InworldTTSClient : MonoBehaviour
{
    [Header("Inworld TTS Settings")]
    public string voiceId = "Ashley"; // Options: Ashley, Dennis, Alex, Craig, etc.
    public string modelId = "inworld-tts-1.5-mini";

    private string base64AuthToken;

    private void Awake()
    {
        LoadLocalCredentials();
    }

    private void LoadLocalCredentials()
    {
        string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string authFilePath = Path.Combine(userProfilePath, ".inworld", "auth.json");

        if (File.Exists(authFilePath))
        {
            string jsonContent = File.ReadAllText(authFilePath);
            InworldCredentials creds = JsonUtility.FromJson<InworldCredentials>(jsonContent);

            // Assign API key - already encoded in base64
            base64AuthToken = creds.base64Key;

            Debug.Log("Inworld Base64 token loaded successfully!");
        }
        else
        {
            Debug.LogError($"Auth file missing at: {authFilePath}");
        }
    }

    public async Task<string> GenerateSpeech(string textToSpeak)
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

            // Parse the response and extract the Base64 audio string
            TTSResponse response = JsonUtility.FromJson<TTSResponse>(www.downloadHandler.text);
            byte[] audioBytes = Convert.FromBase64String(response.audioContent);

            // Save bytes to a temp file and return the file path directly for FMOD
            string tempPath = Path.Combine(Application.temporaryCachePath, "character_voice_temp.wav");
            File.WriteAllBytes(tempPath, audioBytes);

            return tempPath;
        }
    }
}