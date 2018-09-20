using System;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace NinaBirthday.Source
{
	/// <summary>
	/// Converts a texture to a signed distance field
	/// </summary>
	public static class TextureToSDF
	{
		/// <summary>
		/// Converts a black and white image to a SDF
		/// </summary>
		public static Texture BlackAndWhiteToSDF(Texture texture, float spreadRadius)
		{
			SDFGrid grid = SDFGrid.FromTexture(texture);
			return grid.ToTexture(spreadRadius);
		}

		public class SDFGrid
		{
			public SDFGrid(int width, int height)
			{
				Width = width;
				Height = height;

				_gridPos = new Int2[Width + 2, Height + 2];
				_gridNeg = new Int2[Width + 2, Height + 2];
			}

			public int Width { get; }
			public int Height { get; }

			public float this[int x, int y]
			{
				get
				{
					float dist1 = _gridPos[x + 1, y + 1].Length;
					float dist2 = _gridNeg[x + 1, y + 1].Length;
					return dist1 - dist2;
				}
			}

			private readonly Int2[,] _gridPos;
			private readonly Int2[,] _gridNeg;

			public static SDFGrid FromTexture(Texture texture)
			{
				var grid = new SDFGrid(texture.Width, texture.Height);
				const int EMPTY = 9999;

				if (texture.Format.SizeInBytes() != 4)
				{
					Debug.Log("Don't compress the textures!!");
				}

				byte[] data = texture.GetMipData(0, out int rowPitch, out int slicePitch);
				unsafe
				{
					fixed (byte* dataPtr = data)
					{
						var colorsPtr = (Color32*)dataPtr;
						for (int y = 0; y < texture.Height; y++)
						{
							for (int x = 0; x < texture.Width; x++)
							{
								var color = colorsPtr[y * texture.Width + x];
								int dist = (int)(color.R > 128 + 50 ? EMPTY : 0);
								grid._gridPos[x + 1, y + 1] = new Int2(dist);
								grid._gridNeg[x + 1, y + 1] = new Int2(EMPTY - dist);
							}
						}
					}
				}

				int gridWidth = grid._gridPos.GetLength(0);
				int gridHeight = grid._gridPos.GetLength(1);

				for (int i = 0; i < gridWidth; i++)
				{
					grid._gridPos[i, 0] = new Int2(0);
					grid._gridNeg[i, 0] = new Int2(EMPTY);

					grid._gridPos[i, gridHeight - 1] = new Int2(0);
					grid._gridNeg[i, gridHeight - 1] = new Int2(EMPTY);
				}

				for (int i = 0; i < gridHeight; i++)
				{
					grid._gridPos[0, i] = new Int2(0);
					grid._gridNeg[0, i] = new Int2(EMPTY);

					grid._gridPos[gridWidth - 1, i] = new Int2(0);
					grid._gridNeg[gridWidth - 1, i] = new Int2(EMPTY);
				}

				var positiveOffsets = new[]
				{
					new Int2(-1,  0),
					new Int2( 0, -1),
					new Int2(-1, -1),
					new Int2( 1, -1)
				};

				grid.RunPass(grid._gridPos, positiveOffsets, new Int2(1, 0), false);
				grid.RunPass(grid._gridNeg, positiveOffsets, new Int2(1, 0), false);

				var negativeOffsets = new[]
				{
					new Int2( 1,  0),
					new Int2( 0,  1),
					new Int2(-1,  1),
					new Int2( 1,  1)
				};

				grid.RunPass(grid._gridPos, negativeOffsets, new Int2(-1, 0), true);
				grid.RunPass(grid._gridNeg, negativeOffsets, new Int2(-1, 0), true);
				return grid;
			}

			private void RunPass(Int2[,] grid, Int2[] offsets1, Int2 offset2, bool inverted)
			{
				for (int y = 1; y < Height; y++)
				{
					int yPos = inverted ? ReverseIndex(y, Height) + 1 : y;

					for (int x = 1; x < Width; x++)
					{
						int xPos = inverted ? ReverseIndex(x, Width) + 1 : x;
						for (int i = 0; i < offsets1.Length; i++)
						{
							Update(grid, xPos, yPos, ref offsets1[i]);
						}
					}

					for (int x = 1; x < Width; x++)
					{
						int xPos = inverted ? x : ReverseIndex(x, Width) + 1;
						Update(grid, xPos, yPos, ref offset2);

					}
				}
			}

			private void Update(Int2[,] grid, int xPos, int yPos, ref Int2 offset)
			{
				Int2 point = grid[xPos, yPos];

				int otherXPos = xPos + offset.X;
				int otherYPos = yPos + offset.Y;

				Int2 other = grid[otherXPos, otherYPos];

				int dx = other.X + offset.X;
				int dy = other.Y + offset.Y;
				int lengthSquared = dx * dx + dy * dy;
				if (lengthSquared < point.LengthSquared)
				{
					point.X = dx;
					point.Y = dy;

					grid[xPos, yPos] = point;
				}
			}

			private int ReverseIndex(int index, int length)
			{
				return length - index - 1;
			}

			public Texture ToTexture(float spreadRadius = 1f)
			{
				var sdfTexture = Content.CreateVirtualAsset<Texture>();
				TextureBase.InitData initData;
				initData.Width = Width;
				initData.Height = Height;
				initData.ArraySize = 1;
				initData.Format = PixelFormat.R8G8B8A8_UNorm;
				var sdfData = new byte[initData.Width * initData.Height * initData.Format.SizeInBytes()];
				unsafe
				{
					fixed (byte* dataPtr = sdfData)
					{
						// Generate pixels data (linear gradient)
						var colorsPtr = (Color32*)dataPtr;
						for (int y = 0; y < initData.Height; y++)
						{
							for (int x = 0; x < initData.Width; x++)
							{

								float dist = (Mathf.Clamp(this[x, y] / spreadRadius, -1f, 1f) + 1f) / 2f;

								colorsPtr[y * initData.Width + x] = Color32.Lerp(Color.Black, Color.White, dist);
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
}
