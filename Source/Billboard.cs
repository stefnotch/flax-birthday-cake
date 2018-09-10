using System;
using System.Collections.Generic;
using FlaxEngine;

namespace NinaBirthday
{
	public class Billboard : Script
	{
		private Vector3 _up = Vector3.Up;
		private void OnEnable()
		{
			Actor.Orientation = Quaternion.Billboard(Actor.Position, Camera.MainCamera.Position, _up, Camera.MainCamera.Direction);
		}

		private void Update()
		{
			Actor.Orientation = Quaternion.Billboard(Actor.Position, Camera.MainCamera.Position, _up, Camera.MainCamera.Direction);
		}
	}
}
