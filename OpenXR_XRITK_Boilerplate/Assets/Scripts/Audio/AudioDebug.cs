using UnityEngine;
using UnityEngine.UI;

public class VRDebugText : MonoBehaviour
{
    public AudioBeatController driver;
    public Transform xrCamera;
    public float distance = 1.2f;
    Text text;

    void Start()
    {
        text = GetComponentInChildren<Text>();
    }

    void LateUpdate()
    {
        if (!driver || !xrCamera) return;

        // Follow camera
        transform.position = xrCamera.position + xrCamera.forward * distance;
        transform.rotation = Quaternion.LookRotation(xrCamera.forward);

        // Show values
        text.text =
            $"Bass: {driver.bass:F3}\n" +
            $"Mid: {driver.mid:F3}\n" +
            $"High: {driver.high:F3}\n" +
            $"BassEnv: {driver.bassEnv:F3}\n" +
            $"Gain: {(driver.sensitivity):F2}";
    }
}
