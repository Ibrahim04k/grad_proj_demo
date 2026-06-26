using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class NPCVoiceController : MonoBehaviour
{
    [Header("Server")]
    public string serverUrl = "wss://gizmo-battering-moaning.ngrok-free.dev/ws/npc";

    [Header("World Context")]
    public int yearRangeMin = 1000;
    public int yearRangeMax = 1100;
    public string locationOldName = "Village";
    public string civilization = "Medieval";
    public string roleOrName = "Blacksmith";

    private ClientWebSocket websocket;
    private AudioSource audioSource;
    private string microphoneName;
    private AudioClip recordingClip;
    private bool isSending = false;

    // Internal queue to dispatch Unity API calls back to the main thread
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    // --- Serializable classes for JSON ---

    [System.Serializable]
    private class WorldData
    {
        public int[] year_range;
        public string location_old_name;
        public string civilization;
        public string role_or_name;
    }

    [System.Serializable]
    private class ConfigMessage
    {
        public WorldData world;
    }

    [System.Serializable]
    private class IncomingMetaData
    {
        public string type;
        public string text;
        public string emotion;
        public string reason;
    }

    // -----------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------

    async void Start()
    {
        audioSource = GetComponent<AudioSource>();
        microphoneName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (microphoneName == null)
            Debug.LogWarning("No microphone detected!");

        websocket = new ClientWebSocket();
        await ConnectToServer();
    }

    void Update()
    {
        // Safely execute any queued actions (e.g. PlayAudio) on the main thread
        while (mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }

        // NOTE: Push-to-talk input is no longer handled here.
        // The player's NPCVoiceTrigger script calls StartRecording() /
        // StopRecordingAndSend() on whichever NPC is currently "active"
        // (see NPCInteractionZone.cs).
    }

    void OnApplicationQuit()
    {
        CloseWebSocket();
    }

    void OnDestroy()
    {
        CloseWebSocket();
    }

    // -----------------------------------------------
    // Connection
    // -----------------------------------------------

    async Task ConnectToServer()
    {
        try
        {
            Uri uri = new Uri(serverUrl);
            await websocket.ConnectAsync(uri, CancellationToken.None);
            Debug.Log("🟢 Connected to NPC server!");

            // Start listening for incoming messages in the background
            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ WebSocket Connection Error: {e.Message}");
        }
    }

    void CloseWebSocket()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            // Fire-and-forget close (can't await in OnDestroy/OnApplicationQuit)
            websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            websocket.Dispose();
            websocket = null;
        }
    }

    // -----------------------------------------------
    // Recording
    // -----------------------------------------------

    public void StartRecording()
    {
        if (microphoneName == null || isSending) return;
        Debug.Log("🎙️ Recording started...");
        recordingClip = Microphone.Start(microphoneName, false, 10, 16000);
    }

    public async void StopRecordingAndSend()
    {
        if (microphoneName == null || isSending) return;
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogWarning("⚠️ WebSocket is not connected. Cannot send.");
            return;
        }

        Microphone.End(microphoneName);
        Debug.Log("⏳ Recording ended, sending to server...");

        isSending = true;

        try
        {
            // ── Message 1: Send JSON config (no audio inside) ──────────────
            ConfigMessage config = new ConfigMessage
            {
                world = new WorldData
                {
                    year_range = new int[] { yearRangeMin, yearRangeMax },
                    location_old_name = locationOldName,
                    civilization = civilization,
                    role_or_name = roleOrName
                }
            };

            string configJson = JsonUtility.ToJson(config);
            byte[] configBuffer = Encoding.UTF8.GetBytes(configJson);

            await websocket.SendAsync(
                new ArraySegment<byte>(configBuffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            Debug.Log("📤 Sent config JSON.");

            // ── Message 2: Send raw WAV bytes as binary ─────────────────────
            byte[] wavBytes = WavUtility.FromAudioClip(recordingClip);

            await websocket.SendAsync(
                new ArraySegment<byte>(wavBytes),
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None
            );

            Debug.Log($"📤 Sent audio bytes ({wavBytes.Length} bytes).");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Send Error: {e.Message}");
        }
        finally
        {
            isSending = false;
        }
    }

    // -----------------------------------------------
    // Receiving Messages
    // -----------------------------------------------

    async Task ReceiveMessages()
    {
        var buffer = new byte[1024 * 64]; // 64 KB receive buffer

        while (websocket != null && websocket.State == WebSocketState.Open)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    WebSocketReceiveResult result;

                    // Keep reading chunks until we have the full message
                    do
                    {
                        result = await websocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None
                        );
                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string jsonResponse = Encoding.UTF8.GetString(ms.ToArray());
                        HandleTextResponse(jsonResponse);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Server sends raw WAV audio bytes — play them directly
                        byte[] audioBytes = ms.ToArray();
                        HandleAudioResponse(audioBytes);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("🔴 Server closed the connection.");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (websocket != null && websocket.State == WebSocketState.Open)
                    Debug.LogError($"❌ Receive Error: {e.Message}");
                break;
            }
        }
    }

    // -----------------------------------------------
    // Handlers
    // -----------------------------------------------

    void HandleTextResponse(string json)
    {
        var data = JsonUtility.FromJson<IncomingMetaData>(json);

        if (data.type == "meta")
        {
            Debug.Log($"🧠 NPC: \"{data.text}\" | Emotion: {data.emotion}");
            // TODO: trigger NPC animations using data.emotion here
        }
        else if (data.type == "error")
        {
            Debug.LogWarning($"❌ Server error: {data.reason}");
        }
        else if (data.type == "done")
        {
            Debug.Log("✅ NPC finished responding.");
        }
    }

    void HandleAudioResponse(byte[] wavBytes)
    {
        // Must run on the main thread — queue it for Update()
        mainThreadActions.Enqueue(() =>
        {
            AudioClip clip = WavUtility.ToAudioClip(wavBytes);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("🔊 Playing NPC voice.");
            }
        });
    }
}
