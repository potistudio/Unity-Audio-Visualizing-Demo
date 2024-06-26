
using System.Linq;
using UnityEngine;
using Alchemy.Inspector;

public class AudioSpectrum : MonoBehaviour {
	[SerializeField] private AudioSource m_AudioSource;
	[SerializeReference] private ISpectrumMethod m_SpectrumMethod;

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

		m_SpectrumMethod.OutputResolution = m_OutputResolution;
		m_SpectrumMethod.OutputMultiplier = m_OutputMultiplier;
		m_SpectrumMethod.MinFrequency = m_MinFrequency;
		m_SpectrumMethod.MaxFrequency = m_MaxFrequency;
		m_SpectrumMethod.AudioDuration = m_AudioDuration;
		m_SpectrumMethod.SmoothingTimeConstant = m_SmoothingTimeConstant;
		m_SpectrumMethod.WindowSkew = m_WindowSkew;
	}

	private void Start() {
		m_SampleRate = AudioSettings.outputSampleRate;
		m_SpectrumMethod.SampleRate = m_SampleRate;

		m_SpectrumMethod.Prepare();
	}

	private void Update() {
		// Get Output Waveform
		m_AudioSource.GetOutputData (m_OutputAudioData, 0);

		// Calc DFT
		ProcessedAudioData = m_SpectrumMethod.Process (m_OutputAudioData);
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
