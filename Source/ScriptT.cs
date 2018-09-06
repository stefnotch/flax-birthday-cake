using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday
{
	public class ScriptT<T> : Script where T : Actor
	{
		new public T Actor { get; private set; }

		public void Awake()
		{
			Actor = base.Actor.As<T>();
			if (Actor == null)
			{
				Debug.LogError("Not attached to an actor of the type " + typeof(T));
			}
		}
	}
}
