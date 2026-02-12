using UnityEngine;
using UnityEngine.SceneManagement;

public class FlutterBridge : MonoBehaviour
{
    // متغير ثابت عشان نتأكد إن مفيش غير نسخة واحدة بس من الأوبجكت ده
    public static FlutterBridge instance;

    void Awake()
    {
        // 1. لو النسخة دي لسه مش موجودة، احجزها وامنع تدميرها
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // الأمر ده هو اللي بيخليه يكمل معاك في كل السينز
        }
        // 2. لو في نسخة موجودة أصلاً (مثلاً لما رجعت للمنيو تاني)، دمر النسخة الجديدة دي فوراً
        else
        {
            Destroy(gameObject);
        }
    }

    // دي الدالة اللي فلاتر هينده عليها
    public void HandleMessage(string message)
    {
        // نتأكد إن الرسالة مفيش فيها مسافات زيادة
        message = message.Trim();

        switch (message)
        {
            case "MENU":
                SceneManager.LoadScene(0);
                break;

            case "ROMA":
                SceneManager.LoadScene(1);
                break;

            case "EGYPT":
                SceneManager.LoadScene(2);
                break;

            case "EXIT":
                Application.Quit();
                // السطر ده عشان تعرف إن الأمر وصل وأنت بتجرب في الـ Editor لأن Quit مش بتشتغل غير في الـ Build
                Debug.Log("Game Exiting..."); 
                break;

            default:
                Debug.LogWarning("Received unknown message: " + message);
                break;
        }
    }
}