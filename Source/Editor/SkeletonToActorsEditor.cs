using System.Threading;
using System.Threading.Tasks;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;
using NinaBirthday.Skeleton;

namespace NinaBirthday
{
	[CustomEditor(typeof(SkeletonToActors))]
	public class SkeletonToActorsEditor : GenericEditor
	{
		public override void Initialize(LayoutElementsContainer layout)
		{
			base.Initialize(layout);

			var skeletonActor = Values[0] as SkeletonToActors;

			layout.Space(20);

			var updateButton = layout.Button("Create", Color.Green);
			updateButton.Button.Clicked += () =>
			{
				skeletonActor.Create();
			};

			var saveButton = layout.Button("Save", Color.Green);
			saveButton.Button.Clicked += () =>
			{
				if (skeletonActor.VirtualModel && skeletonActor.VirtualModel.IsVirtual)
				{
					new Task(() =>
					{
						skeletonActor.VirtualModel.Save(true, $"Content/SkeletonActor/{skeletonActor.ID}_SkinnedModel.flax");
					}).Start();
				}
			};
		}
	}
}