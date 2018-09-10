using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.Source
{
	/// <summary>
	/// Converts a texture to a signed distance field
	/// </summary>
	class TextureToSDF
	{
		public void test(Texture texture)
		{
			byte[] data = texture.GetMipData(0, out int rowPitch, out int slicePitch);
		}
	}
}
