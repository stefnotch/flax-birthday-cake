using System;
using System.Collections.Generic;
using FlaxEngine;

namespace NinaBirthday.Skeleton
{
	[ExecuteInEditMode]
	public class SkeletonToActors : ScriptT<AnimatedModel>
	{
		public SkinnedModel SavedModel;

		[HideInEditor]
		[NoSerialize]
		public SkinnedModel VirtualModel;

		[HideInEditor]
		public SkeletonBone[] Bones;

		private void Start()
		{
			if (SavedModel)
			{
				Actor.SkinnedModel = SavedModel;
			}
			else
			{
				if (Actor.SkinnedModel)
				{
					Actor.SkinnedModel.WaitForLoaded();
					var temp = Actor.SkinnedModel;
					//Actor.SkinnedModel = null;
					VirtualModel = CreateVirtualModel(temp);
					//Actor.SkinnedModel = temp;
					SetBones();
				}
			}
		}

		private void SetBones()
		{
			if (!VirtualModel) return;
			if (Bones == null || Bones.Length != VirtualModel.Skeleton?.Length) return;

			for (int i = 0; i < Bones.Length; i++)
			{
				VirtualModel.Skeleton[i].LocalTransform = Bones[i].LocalTransform;
			}
		}

		public void Create()
		{
			if (!VirtualModel) return;

			var skeletonContainer = CreateSkeletonActors(VirtualModel.Skeleton);
			Actor.AddChild(skeletonContainer, false);
			skeletonContainer.LocalPosition = Vector3.Zero;
			skeletonContainer.Name = "Skeleton Container";
		}

		private SkinnedModel CreateVirtualModel(SkinnedModel skinnedModel)
		{
			if (!skinnedModel || skinnedModel.Skeleton == null) return null;

			SkinnedModel virtualModel = Content.CreateVirtualAsset<SkinnedModel>();
			virtualModel.Skeleton = skinnedModel.Skeleton;

			virtualModel.SetupMaterialSlots(skinnedModel.MaterialSlotsCount);
			for (int i = 0; i < virtualModel.MaterialSlotsCount; i++)
			{
				virtualModel.MaterialSlots[i].Material = skinnedModel.MaterialSlots[i].Material;
				virtualModel.MaterialSlots[i].Name = skinnedModel.MaterialSlots[i].Name;
				virtualModel.MaterialSlots[i].ShadowsMode = skinnedModel.MaterialSlots[i].ShadowsMode;
			}

			virtualModel.SetupMeshes(skinnedModel.MeshesCount);
			for (int i = 0; i < virtualModel.MeshesCount; i++)
			{
				virtualModel.Meshes[i].MaterialSlotIndex = skinnedModel.Meshes[i].MaterialSlotIndex;
				int[] indices = skinnedModel.Meshes[i].DownloadIndexBuffer();

				var vertices = skinnedModel.Meshes[i].DownloadVertexBuffer();
				Vector3[] positions = new Vector3[vertices.Length];
				Int4[] blendIndices = new Int4[vertices.Length];
				Vector4[] blendWeights = new Vector4[vertices.Length];
				Vector3[] normals = new Vector3[vertices.Length];
				Vector3[] tangents = new Vector3[vertices.Length];
				Vector2[] uv = new Vector2[vertices.Length];

				for (int j = 0; j < vertices.Length; j++)
				{
					positions[j] = vertices[j].Position;
					blendIndices[j] = vertices[j].BlendIndices;
					blendWeights[j] = vertices[j].BlendWeights;
					normals[j] = vertices[j].Normal;
					tangents[j] = vertices[j].Tangent;
					uv[j] = vertices[j].TexCoord;
				}

				virtualModel.Meshes[i].UpdateMesh(positions, indices, blendIndices, blendWeights, normals, tangents, uv);
			}
			return virtualModel;
		}

		private Actor CreateSkeletonActors(SkeletonBone[] _bones)
		{
			if (_bones == null) return null;

			Actor container = New<EmptyActor>();
			container.HideFlags = HideFlags.DontSave;
			Actor[] skeletonActors = new Actor[_bones.Length];
			for (int i = 0; i < _bones.Length; i++)
			{
				var bone = _bones[i];
				skeletonActors[i] = New<EmptyActor>();
				skeletonActors[i].Name = bone.Name;
				skeletonActors[i].HideFlags = HideFlags.DontSave;
				skeletonActors[i].LocalTransform = bone.LocalTransform;

				var boneSynchronisation = skeletonActors[i].AddScript<SyncBone>();
				boneSynchronisation.SkeletonToActors = this;
				boneSynchronisation.BoneIndex = i;
			}

			for (int i = 0; i < _bones.Length; i++)
			{
				var bone = _bones[i];
				if (bone.ParentIndex == -1)
				{
					//Attach it to the container
					skeletonActors[i].SetParent(container, false);
				}
				else
				{
					skeletonActors[i].SetParent(skeletonActors[bone.ParentIndex], false);
				}
			}

			return container;
		}

		private void OnDestroy()
		{
			Destroy(ref VirtualModel);
		}
	}
}