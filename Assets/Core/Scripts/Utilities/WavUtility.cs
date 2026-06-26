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
            // --- Read the RIFF/WAVE file header (12 bytes) ---
            string riff = new string(reader.ReadChars(4));  // "RIFF"
            reader.ReadInt32();                              // chunkSize (ignored)
            string wave = new string(reader.ReadChars(4));  // "WAVE"

            if (riff != "RIFF" || wave != "WAVE")
            {
                Debug.LogError("WavUtility: Not a valid WAV file.");
                return null;
            }

            int channels      = 1;
            int sampleRate    = 16000;
            int bitsPerSample = 16;
            float[] audioData = null;

            // --- Single forward pass: iterate every chunk until EOF ---
            while (stream.Position + 8 <= stream.Length) // need at least 4-char ID + int32 size
            {
                string chunkId   = new string(reader.ReadChars(4));
                int    chunkSize = reader.ReadInt32();

                if (chunkId == "fmt ")
                {
                    // Minimum fmt chunk is 16 bytes (PCM)
                    reader.ReadInt16();               // audioFormat (1 = PCM)
                    channels      = reader.ReadInt16();
                    sampleRate    = reader.ReadInt32();
                    reader.ReadInt32();               // byteRate
                    reader.ReadInt16();               // blockAlign
                    bitsPerSample = reader.ReadInt16();

                    // Skip any extra fmt bytes (e.g. extensible format)
                    int extraBytes = chunkSize - 16;
                    if (extraBytes > 0)
                        reader.ReadBytes(extraBytes);
                }
                else if (chunkId == "data")
                {
                    int sampleCount = chunkSize / (bitsPerSample / 8);
                    audioData = new float[sampleCount];

                    if (bitsPerSample == 16)
                    {
                        for (int i = 0; i < sampleCount; i++)
                        {
                            // Guard against truncated files
                            if (stream.Position + 2 > stream.Length) break;
                            short sample = reader.ReadInt16();
                            audioData[i] = sample / 32768f;
                        }
                    }
                    else if (bitsPerSample == 8)
                    {
                        for (int i = 0; i < sampleCount; i++)
                        {
                            if (stream.Position + 1 > stream.Length) break;
                            audioData[i] = (reader.ReadByte() - 128) / 128f;
                        }
                    }

                    break; // "data" is always the last chunk we need
                }
                else
                {
                    // Unknown/metadata chunk (e.g. "LIST", "ID3 ", "bext") — skip it.
                    // Chunks must be word-aligned; odd sizes are padded by 1 byte.
                    long skipAmount = chunkSize + (chunkSize % 2);
                    stream.Position = Math.Min(stream.Position + skipAmount, stream.Length);
                }
            }

            if (audioData == null)
            {
                Debug.LogError("WavUtility: No 'data' chunk found in WAV bytes.");
                return null;
            }

            int totalSamples = audioData.Length;
            AudioClip clip = AudioClip.Create(
                "NPCVoice",
                totalSamples / channels,
                channels,
                sampleRate,
                false
            );
            clip.SetData(audioData, 0);
            return clip;
        }
    }
}
