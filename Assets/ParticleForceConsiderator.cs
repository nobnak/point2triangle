using UnityEngine;
using System.Collections;

public class ParticleForceConsiderator : MonoBehaviour {
	public int initNumParticles = 1000;
	public DistanceField distField;
	public float coeffForceDist = 1f;
	public float coeffForceViscous = 0.5f;
	public float mass = 1f;
	
	private ParticleSystem.Particle[] _particles;
	private ParticleSystem _system;
	private float _massInv;
	private float _log2Inv;
	
	// Use this for initialization
	void Start () {
		this._particles = new ParticleSystem.Particle[initNumParticles];
		this._system = particleSystem;
		this._massInv = 1f / mass;
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
			Vector4 dist = coeffForceDist * distField.distance(p.position);
			Vector3 forceDist = (dist.w > 0 ? (Vector3)dist : Vector3.zero);
			Vector3 force = forceDist;
			Vector3 accel = _massInv * force;
			_particles[i].velocity += dt * accel;
		}
		
		_system.SetParticles(_particles, nParticles);
	}
}
