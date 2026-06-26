using UnityEngine;

// Attach this to each NPC alongside NPCVoiceController.
// Requires a Collider on this object (or a child) with "Is Trigger" checked,
// and the player object must have a Collider + Rigidbody (or be tagged "Player"
// with a Collider) for OnTriggerEnter/Exit to fire.
[RequireComponent(typeof(NPCVoiceController))]
public class NPCInteractionZone : MonoBehaviour
{
    [Tooltip("Tag used to identify the player object.")]
    public string playerTag = "Player";

    private NPCVoiceController npcController;

    void Awake()
    {
        npcController = GetComponent<NPCVoiceController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var trigger = other.GetComponent<NPCVoiceTrigger>();
        if (trigger != null)
        {
            trigger.SetActiveNpc(npcController);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var trigger = other.GetComponent<NPCVoiceTrigger>();
        if (trigger != null)
        {
            trigger.ClearActiveNpc(npcController);
        }
    }
}
