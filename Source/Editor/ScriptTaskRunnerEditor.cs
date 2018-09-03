using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace NinaBirthday
{
	[CustomEditor(typeof(MeshMerger))]
	public class ScriptTaskRunnerEditor : GenericEditor
	{
		public override void Initialize(LayoutElementsContainer layout)
		{
			base.Initialize(layout);

			var merger = Values[0] as MeshMerger;

			layout.Space(20);

			var button = layout.Button("Merge Meshes", Color.Green);
			button.Button.Clicked += () => merger.Run();
		}
	}
}