using UnityEngine;
using System.Collections;

public class ParticleDeployment : MonoBehaviour {
	public float radius = 1f;

	private ParticleSystem _shuriken;
	private DistanceField _distanceField;

	// Use this for initialization
	void Start () {
		_shuriken = GetComponent<ParticleSystem>();
		_distanceField = GetComponent<DistanceField>();
	}
	
	// Update is called once per frame
	void Update () {
		if (_shuriken.maxParticles <= _shuriken.particleCount)
			return;

		var pos = radius * Random.insideUnitSphere;
		var dist = _distanceField.distance(pos);
		if (dist.w > 0)
			return;

		_shuriken.Emit(pos, Vector3.zero, 2.0f * Mathf.Abs(dist.w), _shuriken.startLifetime, Color.white);
	}
}
