using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace NinaBirthday.Source
{
	[ExecuteInEditMode]
	public class Class1 : ScriptT<ModelActor>
	{
		private Texture _texture;
		private Material _material;
		private MaterialInstance _materialInstance;
		public Material Material
		{
			get => _material;
			set
			{
				_material = value;
				MaterialChanged(_material);
			}
		}
		public Texture Texture
		{
			get => _texture;
			set
			{
				_texture = value;
				if (_texture)
				{
					var t = TextureToSDF.BlackAndWhiteToSDF(_texture, 20f);//TextureCopy.CopyTexture(_texture);// TextureToSDF.BlackAndWhiteToSDF(_texture, 20f);
					var param = _materialInstance?.GetParam("Texture");
					if (param != null)
					{
						param.Value = t;
					}
				}
			}
		}

		public void OnEnable()
		{
			Actor.Model.WaitForLoaded();

			Texture = _texture;
		}


		private void MaterialChanged(Material material)
		{
			if (!material) return;

			Actor.Model.WaitForLoaded();
			if (_materialInstance)
			{
				Destroy(ref _materialInstance);
			}
			_materialInstance = material.CreateVirtualInstance();

			Actor.Entries[0].Material = _materialInstance;
			Texture = _texture;
		}
	}
}
