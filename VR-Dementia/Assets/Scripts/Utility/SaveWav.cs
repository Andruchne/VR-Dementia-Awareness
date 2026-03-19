using System;
using System.IO;
using UnityEngine;

public static class SaveWav
{
    /// <summary>
    /// Converts a Unity AudioClip into a standard WAV byte array.
    /// </summary>
    public static byte[] Save(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("SaveWav: AudioClip is null!");
            return null;
        }

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // 1. Fetch audio data from the clip
            int hz = clip.frequency;
            int channels = clip.channels;
            int samples = clip.samples;

            // 2. Write the standard WAV Header
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(36 + samples * channels * 2); // File size
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size (16 for PCM)
            writer.Write((short)1); // AudioFormat (1 for PCM)
            writer.Write((short)channels);
            writer.Write(hz); // SampleRate
            writer.Write(hz * channels * 2); // ByteRate
            writer.Write((short)(channels * 2)); // BlockAlign
            writer.Write((short)16); // BitsPerSample
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(samples * channels * 2); // Subchunk2Size

            // 3. Extract float audio data from the clip
            float[] data = new float[samples * channels];
            clip.GetData(data, 0);

            // 4. Convert float data (-1.0 to 1.0) to 16-bit PCM (short)
            int rescaleFactor = 32767; // Maximum value for a 16-bit integer
            for (int i = 0; i < data.Length; i++)
            {
                writer.Write((short)(data[i] * rescaleFactor));
            }

            // 5. Return the memory stream as a byte array
            return stream.ToArray();
        }
    }
}