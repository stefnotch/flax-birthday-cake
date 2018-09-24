using System.Threading;
using System.Threading.Tasks;
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace NinaBirthday
{
	[CustomEditor(typeof(ImplicitSurface.ImplicitSurface))]
	public class ImplicitSurfaceEditor : GenericEditor
	{
		public override void Initialize(LayoutElementsContainer layout)
		{
			base.Initialize(layout);

			var surface = Values[0] as ImplicitSurface.ImplicitSurface;

			layout.Space(20);

			var updateButton = layout.Button("Update", Color.Green);
			updateButton.Button.Clicked += () =>
			{
				surface.CreateVirtualModel();
				surface.UpdateMesh();
			};

			var saveButton = layout.Button("Save", Color.Green);
			saveButton.Button.Clicked += () =>
			{
				if (surface.Model && surface.Model.IsVirtual)
				{
					new Task(() =>
					{
						surface.Actor.Model = null;
						surface.Model.Save(true, $"Content/ImplicitSurface/{surface.ID}_Model.flax");
					}).Start();
				}
			};
		}
	}
}