using UnityEngine;
using UnityEngine.UI;

public class AudioUIBinder : MonoBehaviour
{
    public AudioBeatController driver;
    public Slider gain, sensitivity, baseSpeed, beatIntensity, riseLerp, fallLerp;//, attack, hold, decay, baseSpeed, beatIntensity;

    void Start()
    {
        gain.onValueChanged.AddListener(v => driver.gain  = v);
        sensitivity.onValueChanged.AddListener(v => driver.sensitivity  = v);
        baseSpeed.onValueChanged.AddListener(v => driver.baseSpeed  = v);
        beatIntensity.onValueChanged.AddListener(v => driver.beatIntensity  = v);
        riseLerp.onValueChanged     .AddListener(v => driver.riseLerp       = v);
        fallLerp.onValueChanged     .AddListener(v => driver.fallLerp       = v);
        // attack.onValueChanged     .AddListener(v => driver.attack       = v);
        // hold.onValueChanged       .AddListener(v => driver.hold         = v);
        // decay.onValueChanged      .AddListener(v => driver.decay        = v);
        // baseSpeed.onValueChanged  .AddListener(v => driver.baseSpeed    = v);
        // beatIntensity.onValueChanged.AddListener(v => driver.beatIntensity = v);
    }
}
