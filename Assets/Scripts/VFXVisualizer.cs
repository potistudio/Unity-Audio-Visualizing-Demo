
using UnityEngine;
using UnityEngine.VFX;

public class VFXVisualizer : MonoBehaviour {
	[SerializeField] private AudioSpectrum m_AudioSpectrum;
	[SerializeField] private AudioSpectrumDrawer m_AudioSpectrumDrawer;

	[Alchemy.Inspector.Title("VFX Settings")]
	[SerializeField] private VisualEffect m_TargetVisualEffect;
	[SerializeField] private string m_TargetMapAttributeName;

	private Texture2D m_AmplitudeMap = default;

	private void Start() {
        m_AmplitudeMap = new(m_AudioSpectrum.OutputResolution, 1, TextureFormat.Alpha8, false) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        m_TargetVisualEffect.SetTexture (m_TargetMapAttributeName, m_AmplitudeMap);
	}

	private void Update() {
		if (m_AudioSpectrum.ProcessedAudioData == null)
			return;

		int textureWidth = m_AudioSpectrum.ProcessedAudioData.Length;

		for (int x = 0; x < textureWidth; x++) {
			m_AmplitudeMap.SetPixel (m_AudioSpectrum.OutputResolution - 1 - x, 0, new Color(0f, 0f, 0f, m_AudioSpectrum.ProcessedAudioData[x] / m_AudioSpectrum.OutputMultiplier));
		}

		m_AmplitudeMap.Apply();
	}
}
