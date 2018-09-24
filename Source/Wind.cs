using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;
using LowQualityParticles;

namespace NinaBirthday
{
	public class Wind : Script
	{
		public float FireDeathLength = 80;
		public int DeadFires = 0;

		public event Action<Wind, int> FireDeath;

		public UIControl MouseOverride;
		private Image _image;

		private void Start()
		{
			_image = MouseOverride?.Get<Image>();
		}

		private void Update()
		{
			Ray windRay = Camera.MainCamera.ConvertMouseToRay(Input.MousePosition);
			var hits = Physics.RayCastAll(windRay.Position, windRay.Direction);

			// Here you can add code that needs to be called every frame
			bool hasEmitter = false;
			foreach (var hit in hits)
			{
				var fireEmitter = hit.Collider.Parent.GetScript<Emitter>();
				if (fireEmitter && fireEmitter.ParticleCount > 0)
				{
					hasEmitter = true;
					if (Input.GetMouseButton(MouseButton.Left))
					{
						Blow(windRay, fireEmitter);
					}
				}
			}

			if (_image != null)
			{
				_image.X = Input.MousePosition.X - _image.Width / 2f;
				_image.Y = Input.MousePosition.Y - _image.Height / 2f;
				if (hasEmitter)
				{
					_image.Visible = true;
					Screen.CursorVisible = false;
				}
				else
				{
					Screen.CursorVisible = true;
					_image.Visible = false;
				}
			}
		}

		private void Blow(Ray windRay, Emitter fireEmitter)
		{
			bool fireDead = false;
			foreach (var particle in fireEmitter.Actor.GetScriptsInChildren<Particle>())
			{
				particle.Target += windRay.Direction * Time.DeltaTime * 1000f;
				particle.SpeedMultiplier = 3;
				if (particle.Target.Length > FireDeathLength)
				{
					fireDead = true;
				}
			}

			if (fireDead && fireEmitter.ParticleCount != 0)
			{
				DeadFires++;
				FireDeath?.Invoke(this, DeadFires);
				fireEmitter.ParticleCount = 0;
			}
		}
	}
}