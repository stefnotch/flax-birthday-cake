using FlaxEngine;
using System;
using System.Collections.Generic;

namespace NinaBirthday
{
	public class CakeCamera : Script
	{
		public Camera Camera { get; set; }

		public float Min = 10;
		public float Max = 120;
		public Vector2 Speed = new Vector2(6);
		public float Damping = 0.01f;
		private const float Smoothing = 0.5f;

		private float _distance;
		private Vector2 _angle;

		private Vector2 _velocity;

		private void Start()
		{
			if (Camera == null) Camera = Camera.MainCamera;
			// Here you can add code that needs to be called when script is created
			_distance = Camera.LocalPosition.Length;
			_angle = new Vector2(Camera.LocalEulerAngles);
			_angle.X = Mathf.Clamp(_angle.X, Min, Max);
		}

		private void Update()
		{
			if (Input.GetMouseButton(MouseButton.Right))
			{
				_velocity += new Vector2(-Input.MousePositionDelta.Y, Input.MousePositionDelta.X);
			}
		}

		private void FixedUpdate()
		{
			_velocity = _velocity * (Damping / Time.DeltaTime);

			// Here you can add code that needs to be called every frame
			_angle += _velocity * Speed * Time.DeltaTime;
			_angle.X = Mathf.Clamp(_angle.X, Min, Max);

			Vector3 cameraOffsetVector = Vector3.Up * Quaternion.Euler(_angle.X, _angle.Y, 0);

			Vector3 cameraPosition = cameraOffsetVector * _distance;

			Vector3 lerpedCameraPosition = Vector3.Lerp(Camera.LocalPosition, cameraPosition, Smoothing);
			lerpedCameraPosition *= _distance / lerpedCameraPosition.Length;

			Camera.LocalPosition = lerpedCameraPosition;
			Camera.LookAt(Camera.Parent.Position);
		}
	}
}
