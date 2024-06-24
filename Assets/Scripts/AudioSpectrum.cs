
using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Alchemy.Inspector;

public class AudioSpectrum : MonoBehaviour {
	[SerializeField] private AudioSource m_AudioSource;

	[Title("Audio Settings")]
	[SerializeField] private int m_AudioDuration;
	[SerializeField] private int m_MinFrequency;
	[SerializeField] private int m_MaxFrequency;

	[Title("Output Settings")]
	[SerializeField, LabelText("Resolution")] private int m_OutputResolution;
	[SerializeField, LabelText("Multiplier")] private float m_OutputMultiplier;

	[Title("Legacy Settings")]
	[SerializeField] private float m_WindowSkew;
	[SerializeField, Range(0f, 1f)] private float m_SmoothingTimeConstant;

	public int AudioDuration { get { return m_AudioDuration; } set { m_AudioDuration = value; }}
	public int SampleRate { get { return m_SampleRate; } set { m_SampleRate = value; }}
	public int OutputResolution { get { return m_OutputResolution; } set { m_OutputResolution = value; }}
	public float OutputMultiplier { get { return m_OutputMultiplier; } set { m_OutputMultiplier = value; }}
	public float[] ProcessedAudioData { get; private set; }

	private GoertzelSpectrumMono m_GoertzelSpectrumMono;
	private readonly float[] m_OutputAudioData = new float[8196];
	private int m_SampleRate = 48000;

	private float Remap (float _x, float _inMin, float _inMax, float _outMin, float _outMax) {
		return (_x - _inMax) / (_inMax - _inMin) * (_outMax - _outMin) + _outMin;
	}

	private void OnValidate() {
		int maxAudioDuration = Mathf.FloorToInt (8196 / (m_SampleRate * 0.001f));

		if (m_AudioDuration < 0) m_AudioDuration = 0;
		if (m_AudioDuration > maxAudioDuration) m_AudioDuration = maxAudioDuration;
		if (m_OutputResolution < 0) m_OutputResolution = 0;
		if (m_MinFrequency < 0) m_MinFrequency = 0;
		if (m_MaxFrequency < 0) m_MaxFrequency = 0;
		if (m_WindowSkew < 0f) m_WindowSkew = 0f;
		if (m_SmoothingTimeConstant < 0f) m_SmoothingTimeConstant = 0f;
		if (m_SmoothingTimeConstant > 1f) m_SmoothingTimeConstant = 1f;
	}

	private void Start() {
		m_SampleRate = AudioSettings.outputSampleRate;
		m_GoertzelSpectrumMono = new (m_OutputResolution, m_SampleRate, m_MinFrequency, m_MaxFrequency, m_SmoothingTimeConstant, m_WindowSkew, m_OutputMultiplier, m_AudioDuration);
	}

	private void Update() {
		// Get Output Waveform
		m_AudioSource.GetOutputData (m_OutputAudioData, 0);

		#region Use Mono
			//* 11.6ms (resolution:200)
			// ProcessedAudioData = m_GoertzelSpectrumMono.Execute (m_OutputAudioData);
		#endregion

		#region Use Job System
			//* 2.2ms (resolution:200)
			// Prepare Output Buffer
			float[] processedSpectrum = new float[m_OutputResolution];
			Unity.Collections.NativeArray<float> processedSpectrumBuffer = new (m_OutputResolution, Unity.Collections.Allocator.TempJob);

			// Prepare Waveform Data as NativeArray
			Unity.Collections.NativeArray<float> source = new (8196, Unity.Collections.Allocator.TempJob);
			source.CopyFrom (m_OutputAudioData);

			// Create Job
			GoertzelSpectrumJob job = new() {
				m_WaveformInput = source,
				m_SpectrumOutput = processedSpectrumBuffer,
				m_SampleRate = m_SampleRate,
				m_SamplesOut = m_OutputResolution,
				m_OutputMultiplier = m_OutputMultiplier,
				m_FreqMin = -m_MinFrequency,
				m_FreqMax = m_MaxFrequency,
				m_AudioDuration = m_AudioDuration,
				m_SmoothingTimeConstant = m_SmoothingTimeConstant,
				m_WindowSkew = m_WindowSkew
			};

			// Execute Job
			JobHandle jobHandle = job.Schedule();
			jobHandle.Complete();

			// Copy Processed Job Buffer to Managed Array
			processedSpectrumBuffer.CopyTo (processedSpectrum);
			ProcessedAudioData = processedSpectrum;

			// Dispose NativeArray
			source.Dispose();
			processedSpectrumBuffer.Dispose();
		#endregion
	}
}

public struct Freq {
	public Freq (float _low, float _mid, float _high) {
		Low = _low;
		Mid = _mid;
		High = _high;
	}

	public float Low { get; private set; }
	public float Mid { get; private set; }
	public float High { get; private set; }
}
