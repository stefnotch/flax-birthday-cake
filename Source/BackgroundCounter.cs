using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.GUI;

namespace NinaBirthday
{
	public class BackgroundCounter : Script
	{
		public Wind Wind;

		public UIControl Text;

		public int MaxCandles = 18;
		public Actor HappyBirthday;
		private int happyBirthdayTransition = 0;
		private float happyBirthdayTransitionValue;

		private float happyBirthdayTransitionTo;
		private Vector3 happyBirthdayOriginalPos;

		public void OnEnable()
		{
			if (Wind)
			{
				HappyBirthday.IsActive = false;
				Wind.FireDeath += Wind_FireDeath;
			}
		}

		private void Wind_FireDeath(Wind wind, int deadFires)
		{
			if (Text)
			{
				Text.Get<Label>().Text = deadFires.ToString();
			}

			if (deadFires >= MaxCandles)
			{
				if (HappyBirthday)
				{
					happyBirthdayOriginalPos = HappyBirthday.LocalPosition;
					HappyBirthday.IsActive = true;
					happyBirthdayTransition = 1;

					happyBirthdayTransitionTo = 250;
				}
			}
		}

		public void Update()
		{
			if (happyBirthdayTransition == 1)
			{
				float value = Mathf.SmoothStep(happyBirthdayTransitionValue) * happyBirthdayTransitionTo;
				happyBirthdayTransitionValue += 1f * Time.DeltaTime;
				HappyBirthday.LocalPosition = happyBirthdayOriginalPos + new Vector3(0, 0, 200 - value);

				if (value >= 249)
				{
					happyBirthdayTransition = 2;
					happyBirthdayTransitionTo = 50;
					happyBirthdayTransitionValue = 0;
					happyBirthdayOriginalPos = HappyBirthday.LocalPosition;
				}
			}
			else if (happyBirthdayTransition == 2)
			{
				float value = Mathf.SmoothStep(happyBirthdayTransitionValue) * happyBirthdayTransitionTo;
				happyBirthdayTransitionValue += 2.5f * Time.DeltaTime;
				HappyBirthday.LocalPosition = happyBirthdayOriginalPos + new Vector3(0, 0, value);

				if (value >= 49)
				{
					happyBirthdayTransition = 3;
				}
			}
		}

		public void OnDisable()
		{
			if (Wind)
			{
				Wind.FireDeath -= Wind_FireDeath;
			}
		}
	}
}