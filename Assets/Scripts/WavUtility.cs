using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // Converts a Unity AudioClip into raw WAV file bytes to send to Python
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            Int16[] intData = new Int16[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * 32767f);
            }

            int sampleRate = clip.frequency;
            int channels = clip.channels;
            int bitsPerSample = 16;
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            int blockAlign = channels * (bitsPerSample / 8);

            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + intData.Length * 2);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);
            writer.Write("data".ToCharArray());
            writer.Write(intData.Length * 2);

            foreach (short sample in intData)
            {
                writer.Write(sample);
            }

            return stream.ToArray();
        }
    }

    // Converts incoming WAV bytes from Python back into a Unity AudioClip
    public static AudioClip ToAudioClip(byte[] wavBytes)
    {
        using (MemoryStream stream = new MemoryStream(wavBytes))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.ReadBytes(4); // RIFF
            reader.ReadInt32(); // chunkSize
            reader.ReadBytes(4); // WAVE

            int channels = 1;
            int sampleRate = 16000;
            int bitsPerSample = 16;

            // Look for the "fmt " chunk
            while (stream.Position < stream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();
                if (chunkId == "fmt ")
                {
                    reader.ReadInt16(); // audioFormat
                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32(); // byteRate
                    reader.ReadInt16(); // blockAlign
                    bitsPerSample = reader.ReadInt16();
                    if (chunkSize > 16) reader.ReadBytes(chunkSize - 16); // skip extra 
                    break;
                }
                stream.Position += chunkSize;
            }

            stream.Position = 12; // Reset back and look for "data" chunk
            int dataSize = 0;
            while (stream.Position < stream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();
                if (chunkId == "data")
                {
                    dataSize = chunkSize;
                    break;
                }
                stream.Position += chunkSize;
            }

            int sampleCount = dataSize / (bitsPerSample / 8);
            float[] audioData = new float[sampleCount];

            if (bitsPerSample == 16)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = reader.ReadInt16();
                    audioData[i] = sample / 32768f;
                }
            }

            AudioClip clip = AudioClip.Create("NPCVoice", sampleCount / channels, channels, sampleRate, false);
            clip.SetData(audioData, 0);
            return clip;
        }
    }
}
