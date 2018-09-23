using System;
using System.Collections.Generic;
using FlaxEngine;

namespace NinaBirthday
{
	public class Billboard : Script
	{
		private Vector3 _up = Vector3.Up;
		private Quaternion _orientation;

		private void OnEnable()
		{
			_orientation = Actor.Orientation;
			Actor.Orientation = Quaternion.Billboard(Actor.Position, Camera.MainCamera.Position, _up, Camera.MainCamera.Direction) * _orientation;
		}

		private void Update()
		{
			Actor.Orientation = Quaternion.Billboard(Actor.Position, Camera.MainCamera.Position, _up, Camera.MainCamera.Direction) * _orientation;
		}
	}
}