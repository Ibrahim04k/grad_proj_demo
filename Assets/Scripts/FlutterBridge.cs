using UnityEngine;
using UnityEngine.SceneManagement;

public class FlutterBridge : MonoBehaviour
{
    // Singleton instance to ensure only one instance of this object exists
    public static FlutterBridge instance;

    void Awake()
    {
        // 1. If instance doesn't exist, assign it and persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the object alive during scene changes
        }
        // 2. If an instance already exists (e.g., returning to menu), destroy the duplicate
        else
        {
            Destroy(gameObject);
        }
    }

    // Method called by Flutter via Unity Message Manager
    public void HandleMessage(string message)
    {
        // Remove any leading or trailing whitespace from the message
        message = message.Trim();

        // Convert the message to all capital letters
        message = message.ToUpperInvariant();


        switch (message)
        {
            case "MENU":
                SceneManager.LoadScene(0);
                break;

            case "EGYPT":
                SceneManager.LoadScene(1);
                break;

            case "ROMA":
                SceneManager.LoadScene(2);
                break;

            case "EXIT":
                Application.Quit();
                // Log for Editor testing, as Application.Quit() only works in builds
                Debug.Log("Game Exiting...");
                break;

            default:
                Debug.LogWarning("Received unknown message: " + message);
                break;
        }
    }
}