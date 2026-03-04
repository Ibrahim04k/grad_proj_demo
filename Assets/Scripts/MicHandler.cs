using UnityEngine;
using UnityEngine.UI; // مهم عشان الـ Button

public class MicHandler : MonoBehaviour
{
    private AudioSource audioSource;
    private string deviceName;
    private bool isRecording = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
        }
    }

    // الدالة اللي هنربطها بالزرار
    public void ToggleRecording()
    {
        if (!isRecording)
        {
            // يبدأ التسجيل (مدة 10 ثواني مثلاً)
            audioSource.clip = Microphone.Start(deviceName, true, 10, 44100);
            isRecording = true;
            Debug.Log("Recording...");
        }
        else
        {
            // يوقف التسجيل ويشغل الصوت فوراً للتجربة
            Microphone.End(deviceName);
            isRecording = false;
            audioSource.Play();
            Debug.Log("Stopped & Playing back.");
        }
    }
}