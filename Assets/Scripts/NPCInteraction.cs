using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public float interactionDistance = 3.0f; // المسافة المطلوبة
    private AudioSource audioSource;
    private string deviceName;
    private bool isRecording = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (Microphone.devices.Length > 0) deviceName = Microphone.devices[0];
    }

    // الدالة اللي هتربطها بالزرار في الـ UI
    public void OnMicButtonClick()
    {
        GameObject npc = GameObject.FindWithTag("NPC");
        
        if (npc != null)
        {
            float distance = Vector3.Distance(transform.position, npc.transform.position);
            
            if (distance <= interactionDistance)
            {
                ToggleMic();
            }
            else
            {
                Debug.Log("أنت بعيد جداً عن الـ NPC!");
            }
        }
    }

    void ToggleMic()
    {
        if (!isRecording)
        {
            audioSource.clip = Microphone.Start(deviceName, true, 10, 44100);
            isRecording = true;
            Debug.Log("بدأ التسجيل...");
        }
        else
        {
            Microphone.End(deviceName);
            isRecording = false;
            audioSource.Play(); // للتجربة سماع الصوت
            Debug.Log("توقف التسجيل.");
        }
    }
}