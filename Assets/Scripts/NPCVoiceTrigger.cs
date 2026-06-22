using UnityEngine;
using UnityEngine.InputSystem; // Import the New Input System namespace

public class NPCVoiceTrigger : MonoBehaviour
{
    [Header("References")]
    public NPCVoiceController voiceController;

    private bool isRecording = false;

    void Update()
    {
        if (voiceController == null) return;
        
        // Safety check to ensure keyboard is connected/detected
        if (Keyboard.current == null) return; 

        // 1. Detect when the 'V' key is pressed down this frame
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            isRecording = true;
            voiceController.StartRecording();
            Debug.Log("<color=green>🎙️ Mic Recording Started... Speak now (V held)!</color>");
        }

        // 2. Detect when the 'V' key is released this frame
        if (Keyboard.current.vKey.wasReleasedThisFrame && isRecording)
        {
            isRecording = false;
            voiceController.StopRecordingAndSend();
            Debug.Log("<color=yellow>⏳ Processing voice... Sending to server.</color>");
        }
    }
}