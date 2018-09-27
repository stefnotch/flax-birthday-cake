using System;
using System.Collections.Generic;
using FlaxEngine;

namespace NinaBirthday.Skeleton
{
	[ExecuteInEditMode]
	public class SyncBone : Script
	{
		public SkeletonToActors SkeletonToActors;
		public int BoneIndex;

		private void Start()
		{
			// Here you can add code that needs to be called when script is created
		}

		private void Update()
		{
			if (SkeletonToActors)
			{
				SkeletonToActors.Bones[BoneIndex].LocalTransform = LocalTransform;
			}
		}
	}
}