using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AudioMaterialBinder : MonoBehaviour
{
    public AudioBeatController driver;

    Renderer rend;
    MaterialPropertyBlock mpb;

    static readonly int BeatTimeID   = Shader.PropertyToID("_BeatTime");
    static readonly int AudioBandsID = Shader.PropertyToID("_AudioBands");
    static readonly int AudioHitsID  = Shader.PropertyToID("_AudioHits");

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (!driver || !rend) return;

        // Update MaterialPropertyBlock values
        mpb.SetFloat(BeatTimeID, driver.beatTime);
        mpb.SetVector(AudioBandsID, new Vector4(driver.bass, driver.mid, driver.high, 0));
        mpb.SetVector(AudioHitsID, new Vector4(driver.bassEnv, driver.midEnv, driver.highEnv, 0));

        rend.SetPropertyBlock(mpb);
    }
}
