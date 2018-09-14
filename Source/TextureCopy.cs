using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace NinaBirthday.Source
{
	public static class TextureCopy
	{
		public static Texture CopyTexture(Texture texture)
		{
			texture.WaitForLoaded();
			var sdfTexture = Content.CreateVirtualAsset<Texture>();
			TextureBase.InitData initData = new TextureBase.InitData();
			initData.Width = texture.Width;
			initData.Height = texture.Height;
			initData.ArraySize = 1;
			initData.Format = PixelFormat.R8G8B8A8_UNorm;

			var sdfData = new byte[initData.Width * initData.Height * initData.Format.SizeInBytes()];

			byte[] data = texture.GetMipData(0, out int rowPitch, out int slicePitch);

			/*Debug.Log(texture.Format);

			Debug.Log(initData.Format.SizeInBits() + ":" + texture.Format.SizeInBits());

			Debug.Log(
					 //sdfData.Length / initData.Height / initData.Format.SizeInBits() + ":" +
					 //rowPitch / texture.Format.SizeInBits() + ":"

					 (sdfData.Length / initData.Format.SizeInBits()) / initData.Height
					 + "sss" +
					(slicePitch / texture.Format.SizeInBits()) / texture.Height +
					"sss" + rowPitch / texture.Format.SizeInBits()
				);
			Debug.Log(sdfData.Length / initData.Format.SizeInBits() + ":" + slicePitch / texture.Format.SizeInBits());*/

			unsafe
			{
				fixed (byte* dataPtr = data)
				{
					fixed (byte* sdfDataPtr = sdfData)
					{
						var colorsPtr = (Color32*)dataPtr;
						var sdfColorsPtr = (Color32*)sdfDataPtr;
						for (int y = 0; y < initData.Height; y++)
						{
							for (int x = 0; x < initData.Width; x++)
							{
								var color = colorsPtr[y * initData.Width + x];
								int dist = (color.R > 128) ? 1 : 0;

								sdfColorsPtr[y * initData.Width + x] = Color32.Lerp(Color.Black, Color.White, dist);
							}
						}
					}
				}
			}

			initData.Mips = new[]
			{
				new TextureBase.InitData.MipData
				{
					Data = sdfData,
					RowPitch = sdfData.Length / initData.Height,
					SlicePitch = sdfData.Length
				}
			};

			sdfTexture.Init(initData);
			return sdfTexture;
		}
	}
}
