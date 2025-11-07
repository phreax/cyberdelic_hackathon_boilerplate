using System.Collections.Generic;
using UnityEngine;

public class AudioBeatController : MonoBehaviour
{
    [Header("Input")]
    public AudioSource source;
    [Range(128, 4096)] public int sampleSize = 4096;
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    public float gain  = 1.0f;

    [Header("Bands (Hz)")]
    public Vector2 bassRange = new Vector2(20, 150);
    public Vector2 midRange  = new Vector2(150, 2000);
    public Vector2 highRange = new Vector2(2000, 8000);

    [Header("Smoothing")]
    [Range(0f, 1f)] public float riseLerp = 0.6f;
    [Range(0f, 1f)] public float fallLerp = 0.2f;

    [Header("Beat Detection")]
    public float sensitivity = 1.6f;
    public float minInterval = 0.2f, maxInterval = 2.0f;
    public int historyCount = 8;
    public float leadTime = 0.08f; // anticipation for predicted hit

    [Header("BeatTime")]
    public float baseSpeed = 1.0f;
    public float beatIntensity = 2.0f;
    [HideInInspector]
    public float beatTime;
    [HideInInspector]
    public float currentSpeed;

    [Header("Hit Envelopes")]
    public float attack = 0.02f;   // seconds to rise to 1
    public float hold   = 0.04f;   // seconds to stay at 1
    public float decay  = 0.20f;   // seconds to fall back to 0
    [Tooltip("If true, pre-ramp envelope ahead of predicted beat by leadTime.")]
    public bool anticipatoryRamp = true;

    [Header("Global Shader Exports")]
    public bool setGlobalShaderParameters = true;

    [HideInInspector]
    public float bass, mid, high;
    
    [HideInInspector]
    public bool bassHit, midHit, highHit;
    [HideInInspector]
    public float nextBassTime, nextMidTime, nextHighTime;

    [HideInInspector]
    public float bassEnv, midEnv, highEnv;

    // ---- internals
    float[] _spectrum;
    float _sampleRate;

    BandState _bassState, _midState, _highState;
    Env _bassEnv, _midEnv, _highEnv;

    struct BandState
    {
        public float smoothed, movingAvg, lastLevel;
        public bool armed;
        public float lastBeatTime;
        public Queue<float> intervals;
        public float avgInterval;
        public bool isBeat;
        public float predictedTime;

        public void Init(float defaultInterval)
        {
            smoothed = movingAvg = lastLevel = 0f;
            armed = true;
            lastBeatTime = 0f;
            intervals = new Queue<float>();
            avgInterval = defaultInterval;
            isBeat = false;
            predictedTime = 0f;
        }
    }

    enum EnvPhase { Idle, Attack, Hold, Decay }
    struct Env
    {
        public EnvPhase phase;
        public float t;     // time within current phase
        public float value; // 0..1
        public void Reset() { phase = EnvPhase.Idle; t = 0f; value = 0f; }
    }

    void Awake()
    {
        _spectrum = new float[sampleSize];
        _sampleRate = AudioSettings.outputSampleRate;

        _bassState.Init(0.5f);
        _midState.Init(0.5f);
        _highState.Init(0.5f);

        _bassEnv.Reset(); _midEnv.Reset(); _highEnv.Reset();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- spectrum
        if (source) source.GetSpectrumData(_spectrum, 0, fftWindow);
        else AudioListener.GetSpectrumData(_spectrum, 0, fftWindow);

        // --- band energies (simple avg, normalized a bit)
        float bassRaw = Mathf.Clamp01(BandAverage(bassRange.x, bassRange.y) * 100f);
        float midRaw  = Mathf.Clamp01(BandAverage(midRange.x,  midRange.y)  * 100f);
        float highRaw = Mathf.Clamp01(BandAverage(highRange.x, highRange.y) * 100f);
        // After you compute bassRaw, midRaw, highRaw  (before smoothing)

        // --- adaptive normalization ---
        float combined = (bassRaw + midRaw + highRaw) / 3f;
        const float targetLevel = 0.25f;            // desired average energy
        float autoGain = 1f;

        // simple adaptive gain: slowly increases if average < target, decreases if too loud
        float rmsAvg = 0f;
        rmsAvg = Mathf.Lerp(rmsAvg, combined, 0.01f);
        autoGain = Mathf.Clamp(targetLevel / Mathf.Max(0.0001f, rmsAvg), 0.5f, 8f);

        // apply gain before clamping
        bassRaw  = Mathf.Clamp01(bassRaw  * gain);
        midRaw   = Mathf.Clamp01(midRaw   * gain);
        highRaw  = Mathf.Clamp01(highRaw  * gain);

        // --- smooth
        bass = Smooth(_bassState.smoothed, bassRaw, riseLerp, fallLerp, out _bassState.smoothed);
        mid  = Smooth(_midState.smoothed,  midRaw,  riseLerp, fallLerp, out _midState.smoothed);
        high = Smooth(_highState.smoothed, highRaw, riseLerp, fallLerp, out _highState.smoothed);

        // moving averages (for dynamic thresholds)
        _bassState.movingAvg = Mathf.Lerp(_bassState.movingAvg, bass, 0.02f);
        _midState.movingAvg  = Mathf.Lerp(_midState.movingAvg,  mid,  0.02f);
        _highState.movingAvg = Mathf.Lerp(_highState.movingAvg, high, 0.02f);

        // --- beats & predictions
        StepBeat(ref _bassState, bass, out bassHit, out nextBassTime);
        StepBeat(ref _midState,  mid,  out midHit,  out nextMidTime);
        StepBeat(ref _highState, high, out highHit, out nextHighTime);

        // --- envelopes (trigger on beats, optionally pre-ramp)
        bassEnv = StepEnvelope(ref _bassEnv, bassHit, _bassState.predictedTime, dt);
        midEnv  = StepEnvelope(ref _midEnv,  midHit,  _midState.predictedTime, dt);
        highEnv = StepEnvelope(ref _highEnv, highHit, _highState.predictedTime, dt);

        // --- BeatTime integration (weighted energy)
        float energy = Mathf.Clamp01(0.6f * bass + 0.3f * mid + 0.1f * high);
        float targetSpeed = baseSpeed * (1f + bass * beatIntensity);

        //float rise = 0.25f, fall = 0.08f;
        currentSpeed = (targetSpeed > currentSpeed)
            ? Mathf.Lerp(currentSpeed, targetSpeed, riseLerp)
            : Mathf.Lerp(currentSpeed, targetSpeed, fallLerp);

        beatTime += currentSpeed * dt;

        // Debug.Log("bassRaw: " + bassRaw);
        // Debug.Log("bass: " + bass);
        // Debug.Log("mid: " + mid);
        // Debug.Log("bassHit: " + bassEnv);
        // Debug.Log("bassTime: " + beatTime);
        // Debug.Log("gain: " + gain);

        // --- shader globals
        if (setGlobalShaderParameters)
        {
            Shader.SetGlobalFloat("_BeatTime", beatTime);
            Shader.SetGlobalVector("_AudioBands",   new Vector4(bass, mid, high, 0));
            Shader.SetGlobalVector("_AudioHits", new Vector4(bassEnv, midEnv, highEnv, 0));
        }
    }

    // ------------------ helpers ------------------

    float Smooth(float prev, float raw, float rise, float fall, out float smoothed)
    {
        smoothed = (raw > prev) ? Mathf.Lerp(prev, raw, rise) : Mathf.Lerp(prev, raw, fall);
        return smoothed;
    }

    void StepBeat(ref BandState b, float level, out bool isBeat, out float nextTime)
    {
        float thresh = Mathf.Max(0.01f, b.movingAvg * sensitivity);
        isBeat = false;
        float t = Time.time;

        if (b.armed && level > thresh && level > b.lastLevel)
        {
            float dt = t - b.lastBeatTime;
            if (dt > minInterval && dt < maxInterval)
            {
                b.intervals.Enqueue(dt);
                while (b.intervals.Count > historyCount) b.intervals.Dequeue();
                float sum = 0f; foreach (var v in b.intervals) sum += v;
                b.avgInterval = (b.intervals.Count > 0) ? sum / b.intervals.Count : b.avgInterval;

                b.lastBeatTime = t;
                isBeat = true;
            }
            b.armed = false;
        }
        if (level < thresh * 0.6f) b.armed = true;

        b.lastLevel = level;
        b.predictedTime = b.lastBeatTime + b.avgInterval - leadTime;
        nextTime = b.predictedTime;
    }

    float StepEnvelope(ref Env env, bool trigger, float predictedTime, float dt)
    {
        float now = Time.time;

        // Anticipatory pre-ramp: start Attack so it peaks at predictedTime
        if (anticipatoryRamp && predictedTime > now)
        {
            float timeToPeak = predictedTime - now;
            // if we're within (attack) of the predicted hit and not already peaked, force Attack
            if (timeToPeak <= Mathf.Max(attack, 0.0001f) && env.phase != EnvPhase.Hold)
            {
                env.phase = EnvPhase.Attack;
                env.t = Mathf.Clamp01(attack - timeToPeak); // start part-way so we peak exactly on time
            }
        }

        // On real beat, (re)trigger full A-H-D cycle
        if (trigger)
        {
            env.phase = EnvPhase.Attack;
            env.t = 0f;
        }

        switch (env.phase)
        {
            case EnvPhase.Idle:
                env.value = Mathf.Max(0f, env.value - dt / Mathf.Max(decay, 0.0001f)); // gentle fall to 0
                break;

            case EnvPhase.Attack:
                env.t += dt;
                env.value = (attack <= 0f) ? 1f : Mathf.Clamp01(env.t / attack);
                if (env.value >= 1f)
                {
                    env.phase = (hold > 0f) ? EnvPhase.Hold : EnvPhase.Decay;
                    env.t = 0f;
                }
                break;

            case EnvPhase.Hold:
                env.t += dt;
                env.value = 1f;
                if (env.t >= hold)
                {
                    env.phase = EnvPhase.Decay;
                    env.t = 0f;
                }
                break;

            case EnvPhase.Decay:
                env.t += dt;
                env.value = (decay <= 0f) ? 0f : Mathf.Clamp01(1f - env.t / decay);
                if (env.value <= 0f)
                {
                    env.phase = EnvPhase.Idle;
                    env.t = 0f;
                    env.value = 0f;
                }
                break;
        }

        return env.value;
    }

    float BandAverage(float hzMin, float hzMax)
    {
        int i0 = HzToBin(hzMin);
        int i1 = HzToBin(hzMax);
        if (i1 < i0) { var tmp = i0; i0 = i1; i1 = tmp; }
        i0 = Mathf.Clamp(i0, 0, _spectrum.Length - 1);
        i1 = Mathf.Clamp(i1, 0, _spectrum.Length - 1);

        float sum = 0f; int count = 0;
        for (int i = i0; i <= i1; i++) { sum += _spectrum[i]; count++; }
        return (count > 0) ? sum / count : 0f;
    }

    int HzToBin(float hz)
    {
        float nyquist = _sampleRate * 0.5f;
        float t = Mathf.Clamp01(hz / Mathf.Max(1f, nyquist));
        return Mathf.RoundToInt(t * (sampleSize - 1));
    }
}
