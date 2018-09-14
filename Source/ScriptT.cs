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
		private T _actor;

		new public T Actor
		{
			get
			{
				if (_actor == null)
				{
					_actor = base.Actor.As<T>();
					if (_actor == null)
					{
						Debug.LogError("Not attached to an actor of the type " + typeof(T));
					}
				}
				return _actor;
			}
		}
	}
}
