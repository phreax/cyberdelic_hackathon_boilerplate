using UnityEngine;

public class BeatTransform : MonoBehaviour
{
    public AudioBeatController driver;
    public Vector3 baseScale = Vector3.one;
    public float bassAmount = 0.5f;
    public bool move = false;
    public bool circular = false;
    public bool rotate = false;
    public bool useOsc = false;
    public float moveAmplitude = 0.1f;
    public float speed = 1.0f;
    
    private float oscBass = 0.0f;
    private Vector3 p0; // original position

   void Start()
    {
        // Store the starting local position once
        p0 = transform.localPosition;
    }
    public void SetOSCBass(float v)
    {
        Debug.Log("bass osc received: " + v);
        oscBass = v;
    }
    void Update()
    {
        if (!driver) return;

        if (move)
        {
            float beatTime = driver.beatTime;

            // Copy the position, modify, then assign back
            Vector3 pos = p0;
            pos.z += Mathf.Sin(beatTime * speed) * moveAmplitude; // animated vertical motion
            if(circular)
                pos.x += Mathf.Cos(beatTime * speed) * moveAmplitude; // animated vertical motion
            transform.localPosition = pos;
        }
        else
        {
            Debug.Log("osc bass: " + oscBass);
            float bass = useOsc ? oscBass : driver.bass;
            float s = 1f + bass * bassAmount;
            transform.localScale = baseScale * s;
        }
        if(rotate) {
            Vector3 rotationSpeed = new Vector3(45f, 90f, 0f);
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}

