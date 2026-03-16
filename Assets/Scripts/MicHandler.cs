using UnityEngine;
using UnityEngine.UI; // ضروري للتحكم في الألوان والزرار

public class MicHandler : MonoBehaviour
{
    private AudioSource audioSource;
    private string deviceName;
    private bool isRecording = false;

    [Header("UI Settings")]
    public Button recordButton; // اسحب الزرار هنا من الـ Inspector
    public Color recordingColor = Color.green;
    private Color originalColor;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // حفظ اللون الأصلي للزرار
        if (recordButton != null)
        {
            originalColor = recordButton.image.color;
        }

        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
        }
    }

    public void ToggleRecording()
    {
        if (!isRecording)
        {
            // بدء التسجيل
            audioSource.clip = Microphone.Start(deviceName, true, 10, 44100);
            isRecording = true;
            
            // تغيير اللون للأخضر
            recordButton.image.color = recordingColor;
            Debug.Log("Recording...");
        }
        else
        {
            // إيقاف التسجيل
            Microphone.End(deviceName);
            isRecording = false;
            
            // رجوع اللون للأصلي
            recordButton.image.color = originalColor;
            
            audioSource.Play();
            Debug.Log("Stopped & Playing back.");
        }
    }
}