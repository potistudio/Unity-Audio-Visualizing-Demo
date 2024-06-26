
using Unity.Jobs;

//* Use Burst + Job System -> 3.5ms (resolution:200)
[System.Serializable]
public sealed class GoertzelBurstProxy : ISpectrumMethod {
    public int OutputResolution { get; set; }
    public int SampleRate { get; set; }
    public float OutputMultiplier { get; set; }
    public int MinFrequency { get; set; }
    public int MaxFrequency { get; set; }
    public int AudioDuration { get; set; }
    public float SmoothingTimeConstant { get; set; }
    public float WindowSkew { get; set; }

    public void Prepare() {	}

	public float[] Process (float[] _waveform) {
		// Prepare Output Buffer
		float[] processedSpectrum = new float[OutputResolution];
		Unity.Collections.NativeArray<float> processedSpectrumBuffer = new (OutputResolution, Unity.Collections.Allocator.TempJob);

		// Prepare Waveform Data as NativeArray
		Unity.Collections.NativeArray<float> source = new (8196, Unity.Collections.Allocator.TempJob);
		source.CopyFrom (_waveform);

		// Create Job
		GoertzelSpectrumJob job = new() {
			m_WaveformInput = source,
			m_SpectrumOutput = processedSpectrumBuffer,
			m_SampleRate = SampleRate,
			m_SamplesOut = OutputResolution,
			m_OutputMultiplier = OutputMultiplier,
			m_FreqMin = -MinFrequency,
			m_FreqMax = MaxFrequency,
			m_AudioDuration = AudioDuration,
			m_SmoothingTimeConstant = SmoothingTimeConstant,
			m_WindowSkew = WindowSkew
		};

		// Execute Job
		JobHandle jobHandle = job.Schedule();
		jobHandle.Complete();

		// Copy Processed Job Buffer to Managed Array
		processedSpectrumBuffer.CopyTo (processedSpectrum);

		// Dispose NativeArray
		source.Dispose();
		processedSpectrumBuffer.Dispose();

		return processedSpectrum;
	}
}
