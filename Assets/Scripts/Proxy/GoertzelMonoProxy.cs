
//* Use Mono -> 11.6ms (resolution:200)
[System.Serializable]
public sealed class GoertzelMonoProxy : ISpectrumMethod {
    public int SampleRate { get; set; }
    public int OutputResolution { get; set; }
    public float OutputMultiplier { get; set; }
    public int MinFrequency { get; set; }
    public int MaxFrequency { get; set; }
    public int AudioDuration { get; set; }
    public float SmoothingTimeConstant { get; set; }
    public float WindowSkew { get; set; }

	private GoertzelSpectrumMono m_GoertzelSpectrumMono;

    public void Prepare() {
		m_GoertzelSpectrumMono = new (OutputResolution, SampleRate, MinFrequency, MaxFrequency, SmoothingTimeConstant, WindowSkew, OutputMultiplier, AudioDuration);
	}

    public float[] Process (float[] _waveform) {
		return m_GoertzelSpectrumMono.Execute (_waveform);
	}
}
