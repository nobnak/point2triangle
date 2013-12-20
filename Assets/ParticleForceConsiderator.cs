using UnityEngine;
using System.Collections;
using System.Text;

public class ParticleForceConsiderator : MonoBehaviour {
	public DistanceField distField;
	public float coeffForceDist = 1f;
	
	private ParticleSystem.Particle[] _particles;
	private ParticleSystem _system;
	private float _log2Inv;
	
	// Use this for initialization
	void Start () {
		this._particles = new ParticleSystem.Particle[0];
		this._system = particleSystem;
		this._log2Inv = 1f / Mathf.Log10(2);
	}
	
	// Update is called once per frame
	void Update () {
		if (_system.particleCount > _particles.Length) {
			int powerOfTwo = (int)Mathf.Ceil(Mathf.Log10(_system.particleCount) * _log2Inv);
			int n = (int)Mathf.Pow(2, powerOfTwo);
			_particles = new ParticleSystem.Particle[n];
		}
		int nParticles = _system.GetParticles(_particles);
		float dt = Time.deltaTime;

		for (int i = 0; i < nParticles; i++) {
			ParticleSystem.Particle p = _particles[i];
			Vector4 dist = distField.distance(p.position);
			Vector3 forceDist = (dist.w > 0 ? coeffForceDist * (Vector3)dist : Vector3.zero);
			Vector3 force = forceDist;
			p.velocity += dt * force;
			p.size = Mathf.Abs(dist.w);
			_particles[i] = p;
		}
		
		_system.SetParticles(_particles, nParticles);
	}
}
