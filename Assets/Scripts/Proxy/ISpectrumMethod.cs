
public interface ISpectrumMethod {
	int SampleRate { get; set; }
	int OutputResolution { get; set; }
	float OutputMultiplier { get; set; }
	int MinFrequency { get; set; }
	int MaxFrequency { get; set; }
	int AudioDuration { get; set; }
	float SmoothingTimeConstant { get; set; }
	float WindowSkew { get; set; }

	void Prepare();
	float[] Process (float[] _waveform);
}
