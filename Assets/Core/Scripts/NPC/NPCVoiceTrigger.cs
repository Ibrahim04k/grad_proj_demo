using UnityEngine;
using UnityEngine.InputSystem;

// Attach this to the PLAYER (not the NPC).
// It is the single source of push-to-talk input in the scene.
// It always talks to whichever NPC is currently "active" (see NPCInteractionZone.cs),
// so only one NPC responds at a time even if several exist in the scene.
public class NPCVoiceTrigger : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Hold this key to record. Release to send to the active NPC.")]
    public Key pushToTalkKey = Key.M;

    // The NPC the player is currently close enough to / interacting with.
    // Set by NPCInteractionZone when the player enters/exits range.
    private NPCVoiceController activeNpc;

    private bool isRecording = false;

    void Update()
    {
        if (Keyboard.current == null) return;

        // Keyboard support
        if (Keyboard.current[pushToTalkKey].wasPressedThisFrame)
        {
            StartVoiceRecording();
        }

        if (Keyboard.current[pushToTalkKey].wasReleasedThisFrame)
        {
            StopVoiceRecording();
        }
    }

    // Called by NPCInteractionZone when the player enters an NPC's range.
    public void SetActiveNpc(NPCVoiceController npc)
    {
        // Don't switch targets mid-recording (e.g. walking past one NPC into another's zone)
        if (isRecording) return;
        activeNpc = npc;
        Debug.Log($"<color=cyan>🗣️ Now talking to: {npc.name}</color>");
    }

    // Called by NPCInteractionZone when the player exits an NPC's range.
    public void ClearActiveNpc(NPCVoiceController npc)
    {
        // Only clear if this NPC was actually the active one
        if (activeNpc == npc)
        {
            activeNpc = null;
            Debug.Log($"<color=cyan>👋 No longer near: {npc.name}</color>");
        }
    }

    // --- UI BUTTON SUPPORT ---

    // Link this to your UI Button's Event Trigger -> PointerDown
    public void OnPointerDown() 
    {
        StartVoiceRecording();
    }

    // Link this to your UI Button's Event Trigger -> PointerUp
    public void OnPointerUp() 
    {
        StopVoiceRecording();
    }

    // --- SHARED RECORDING LOGIC ---

    private void StartVoiceRecording()
    {
        if (activeNpc == null)
        {
            Debug.Log("<color=orange>🤷 No NPC nearby to talk to.</color>");
            return;
        }

        if (!isRecording)
        {
            isRecording = true;
            activeNpc.StartRecording();
            Debug.Log($"<color=green>🎙️ Mic Recording Started... Speak now!</color>");
        }
    }

    private void StopVoiceRecording()
    {
        if (isRecording && activeNpc != null)
        {
            isRecording = false;
            activeNpc.StopRecordingAndSend();
            Debug.Log("<color=yellow>⏳ Processing voice... Sending to server.</color>");
        }
    }
}