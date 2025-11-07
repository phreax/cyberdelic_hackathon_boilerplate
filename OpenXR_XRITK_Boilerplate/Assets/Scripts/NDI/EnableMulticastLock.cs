#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using UnityEngine.Android;

public class EnableMulticastLock : MonoBehaviour
{
    AndroidJavaObject _lock;

    void Awake()
    {
        const string perm = "android.permission.CHANGE_WIFI_MULTICAST_STATE";
        if (!Permission.HasUserAuthorizedPermission(perm))
            Permission.RequestUserPermission(perm);

        using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = up.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
        {
            _lock = wifi.Call<AndroidJavaObject>("createMulticastLock", "ndi");
            _lock.Call("acquire");
        }
    }

    void OnDestroy() { try { _lock?.Call("release"); } catch {} }
}
#endif

