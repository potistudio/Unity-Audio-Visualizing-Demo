
using UnityEngine;

public class AudioSpectrumDrawer : UnityEngine.UI.Graphic {
	[SerializeField] private AudioSpectrum m_AudioSpectrum;

	[SerializeField] private float m_LeftOffset;
	[SerializeField] private float m_RightOffset;

	private float Remap (float _x, float _inMin, float _inMax, float _outMin, float _outMax) {
		return (_x - _inMax) / (_inMax - _inMin) * (_outMax - _outMin) + _outMin;
	}

	private void AddVerticalLine (UnityEngine.UI.VertexHelper vh, Vector3 _position, float _height, float _width = 2f, Color _color = default) {
		UIVertex v0 = new() { position = new Vector3 (_position.x - _width / 2f, _position.y + _height, 0f), color = _color }; // Top left
		UIVertex v1 = new() { position = new Vector3 (_position.x + _width / 2f, _position.y + _height, 0f), color = _color }; // Top right
		UIVertex v2 = new() { position = new Vector3 (_position.x + _width / 2f, _position.y, 0f), color = _color }; // Bottom right
		UIVertex v3 = new() { position = new Vector3 (_position.x - _width / 2f, _position.y, 0f), color = _color }; // Bottom left

		vh.AddUIVertexQuad (new UIVertex[]{ v0, v1, v2, v3 });
	}

	protected override void OnPopulateMesh (UnityEngine.UI.VertexHelper vh) {
		vh.Clear();

		if (m_AudioSpectrum.ProcessedAudioData == null)
			return;

		for (int i = 0; i < m_AudioSpectrum.ProcessedAudioData.Length; i++) {
			float remappedPosX = Remap (i / (m_AudioSpectrum.ProcessedAudioData.Length - 1f), 0f, 1f, m_LeftOffset, -Mathf.Abs(m_RightOffset - m_LeftOffset) + m_LeftOffset);
			AddVerticalLine (vh, new Vector3(remappedPosX, 0f, 0f), m_AudioSpectrum.ProcessedAudioData[i], 4f, Color.white);
		}
	}

	private void Update() {
		SetAllDirty();
	}
}
