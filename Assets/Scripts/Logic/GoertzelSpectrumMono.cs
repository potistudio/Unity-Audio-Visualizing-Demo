
using System.Linq;

public class GoertzelSpectrumMono {
	private int m_SamplesOut;
	private int m_SampleRate;
	private int m_FreqMin;
	private int m_FreqMax;
	private int m_AudioDuration;
	private float m_SmoothingTimeConstant;
	private float m_WindowSkew;
	private float m_OutputMultiplier;

	public GoertzelSpectrumMono (int _samplesOut, int _sampleRate, int _freqMin, int _freqMax, float _smoothingTimeConstant, float _windowSkew, float _outputMultiplier, int _audioDuration) {
		m_SamplesOut = _samplesOut;
		m_SampleRate = _sampleRate;
		m_FreqMin = _freqMin;
		m_FreqMax = _freqMax;
		m_SmoothingTimeConstant = _smoothingTimeConstant;
		m_WindowSkew = _windowSkew;
		m_OutputMultiplier = _outputMultiplier;
		m_AudioDuration = _audioDuration;
	}

	private float Remap (float _x, float _inMin, float _inMax, float _outMin, float _outMax) {
		return (_x - _inMax) / (_inMax - _inMin) * (_outMax - _outMin) + _outMin;
	}

	private float ApplyWindow (float _posX, bool _truncate, float _skew) {
		float x = _skew > 0 ? (_posX / 2f - 0.5f) / (1f - (_posX / 2f - 0.5f) * 10f * (float)(float)System.Math.Pow((float)_skew, (float)2f)) / (1f / (1f + 10f * (float)(float)System.Math.Pow((float)_skew, (float)2f))) * 2f + 1 : (_posX / 2f + 0.5f) / (1f + (_posX / 2f + 0.5f) * 10f * (float)(float)System.Math.Pow((float)_skew, (float)2f)) / (1f / (1f + 10f * (float)(float)System.Math.Pow((float)_skew, (float)2f))) * 2f - 1f;

		if (_truncate && (float)System.Math.Abs(x) > 1)
			return 0f;

		return 0.54f + 0.46f * (float)(float)System.Math.Cos (x * (float)System.Math.PI);
	}

	private float ApplyWeight (float _freq, float _amount) {
		float f2 = (float)System.Math.Pow (_freq, 2);

		return (float)System.Math.Pow (1.2588966f * 148840000f * (float)System.Math.Pow(f2, 2) / ((f2 + 424.36f) * (float)System.Math.Sqrt((f2 + 11599.29f) * (f2 + 544496.41f)) * (f2 + 148840000f)), _amount);
	}

	private float Ascale (float _x, float _nthRoot, bool _logarithmic, float _dbRange, bool _useAbsoluteValue) {
		return Remap ((float)System.Math.Pow(_x, 1f / _nthRoot), _useAbsoluteValue ? 0f : (float)System.Math.Pow(DBToLinear(-_dbRange), 1f / _nthRoot), 1f, 0f, 1f);
	}

	private float DBToLinear (float _dB) {
		return (float)System.Math.Pow (10f, _dB / 20f);
	}

	private float CalcFreqTilt (float _freq, float _centerFreq = 440f, float _amount = 3f) {
		return (float)System.Math.Abs (_amount) > 0f ? (float)System.Math.Pow (10f, (float)System.Math.Log(_freq / _centerFreq, 2) * _amount / 20f) : 1f;
	}

	private Freq[] GenerateFreqBands (int _samples, int _min, int _max) {
		Freq[] freqArray = new Freq[_samples];

		for (int i = 0; i < _samples; i++) {
			freqArray[i] = new Freq (
				Remap (i - 0.5f, 0f, _samples - 1f, _min, _max),
				Remap (i, 0f, _samples - 1f, _min, _max),
				Remap (i + 0.5f, 0f, _samples - 1f, _min, _max)
			);
		}

		return freqArray;
	}

	private float CalcGoertzel (float[] _waveform, float _coeff) {
		float f1 = 0f, f2 = 0f, sine;

		foreach (float x in _waveform) {
			sine = x + _coeff * f1 - f2;
			f2 = f1;
			f1 = sine;
		}

		return (float)System.Math.Sqrt ((float)System.Math.Pow(f1, 2) + (float)System.Math.Pow(f2, 2) - _coeff * f1 * f2) / _waveform.Length;
	}

	private float[] CalcGoertzelSpectrum (float[] _waveform) {
		return GenerateFreqBands (m_SamplesOut, m_FreqMin, m_FreqMax).Select (x => {
			float coeff = 2f * (float)System.Math.Cos (2f * (float)System.Math.PI * x.Mid / m_SampleRate);
			return CalcGoertzel (_waveform, coeff);
		}).ToArray();
	}

	private void ApplySmoothingTimeConstant (ref float[] _targetArray, in float[] _sourceArray, float _factor = 0.5f) {
		for (int i = 0; i < _targetArray.Length; i++) {
			_targetArray[i] = (float.NaN == _targetArray[i] ? 0f : _targetArray[i]) * _factor + (float.NaN == _sourceArray[i] ? 0f : _sourceArray[i]) * (1f - _factor);
		}
	}

	public float[] Execute (float[] _waveform) {
		int FFT_SIZE = (int)System.Math.Round (m_AudioDuration * (m_SampleRate * 0.001f));

		float[] audioBuffer = new float[FFT_SIZE];
		float normalized = 0f;

		for (int i = 0; i < FFT_SIZE; i++) {
			float x = i * 2f / (FFT_SIZE - 1) - 1;
			float w = ApplyWindow (x, true, m_WindowSkew);

			audioBuffer[i] = _waveform[i + (8196 - FFT_SIZE)] * w;
			normalized += w;
		}

		audioBuffer = audioBuffer.Select (x => x * (audioBuffer.Length / normalized)).ToArray();

		float[] resultBuffer = CalcGoertzelSpectrum (audioBuffer);

		float[] dataArray = new float[resultBuffer.Length];
		ApplySmoothingTimeConstant (ref dataArray, resultBuffer.Select((x, i) => {
			Freq[] freqBands = GenerateFreqBands (m_SamplesOut, m_FreqMin, m_FreqMax);
			return x * DBToLinear (m_OutputMultiplier) * CalcFreqTilt (freqBands[i].Mid, 440f, 0f) * ApplyWeight (freqBands[i].Mid, 0f);
		}).ToArray(), m_SmoothingTimeConstant);
		float[] processedResult = dataArray.Select((x, i) => (float)System.Math.Max(Ascale(x, 1f, false, 70f, true), 0f)).ToArray();

		return processedResult;
	}
}
