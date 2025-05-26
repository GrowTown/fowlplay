using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativeShare : MonoBehaviour
{

    public string roomID = "ROOM123";

    public void ShareRoomID()
    {
        StartCoroutine(ShareRoomIDCoroutine());
    }

    private IEnumerator ShareRoomIDCoroutine()
    {
        string shareText = $"Join my game! Room ID: {roomID}";
        string shareSubject = "Game Room Invitation";

        // For Android
#if UNITY_ANDROID
        // Create intent
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        // Set action and type
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        intentObject.Call<AndroidJavaObject>("setType", "text/plain");

        // Add content
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), shareSubject);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);

        // Get current activity and show chooser
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser",
            intentObject, "Share Room ID Via");
        currentActivity.Call("startActivity", chooser);

        // Clean up
        intentClass.Dispose();
        intentObject.Dispose();
        unity.Dispose();
        currentActivity.Dispose();
        chooser.Dispose();
#endif

        // For iOS (if needed)
#if UNITY_IOS
        // iOS sharing code would go here
#endif

        yield return null;
    }
}

