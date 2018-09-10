using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.Source
{
	public class PerlinNoiseGenerator
	{
		// Noise1234
		// Author: Stefan Gustavson (stegu@itn.liu.se)
		//
		// This library is public domain software, released by the author
		// into the public domain in February 2011. You may do anything
		// you like with it. You may even remove all attributions,
		// but of course I'd appreciate it if you kept my name somewhere.
		//
		// This library is distributed in the hope that it will be useful,
		// but WITHOUT ANY WARRANTY; without even the implied warranty of
		// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
		// General Public License for more details.

		/** \file
				\brief Implements the Noise1234 class for producing Perlin noise.
				\author Stefan Gustavson (stegu@itn.liu.se)
*/

		/*
		 * This implementation is "Improved Noise" as presented by
		 * Ken Perlin at Siggraph 2002. The 3D function is a direct port
		 * of his Java reference code available on www.noisemachine.com
		 * (although I cleaned it up and made the code more readable),
		 * but the 1D, 2D and 4D cases were implemented from scratch
		 * by me.
		 *
		 * This is a highly reusable class. It has no dependencies
		 * on any other file, apart from its own header file.
		 */


		// This is the new and improved, C(2) continuous interpolant
		private float FADE(float t) => (t * t * t * (t * (t * 6 - 15) + 10));



		//---------------------------------------------------------------------
		// Static data

		/*
		 * Permutation table. This is just a random jumble of all numbers 0-255,
		 * repeated twice to avoid wrapping the index at 255 for each lookup.
		 * This needs to be exactly the same for all instances on all platforms,
		 * so it's easiest to just keep it as static explicit data.
		 * This also removes the need for any initialisation of this class.
		 *
		 * Note that making this an int[] instead of a char[] might make the
		 * code run faster on platforms with a high penalty for unaligned single
		 * byte addressing. Intel x86 is generally single-byte-friendly, but
		 * some other CPUs are faster with 4-aligned reads.
		 * However, a char[] is smaller, which avoids cache trashing, and that
		 * is probably the most important aspect on most architectures.
		 * This array is accessed a *lot* by the noise functions.
		 * A vector-valued noise over 3D accesses it 96 times, and a
		 * float-valued 4D noise 64 times. We want this to fit in the cache!
		 */
		private int[] _permutations = new[]{151,160,137,91,90,15,
  131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
  190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
  88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
  77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
  102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
  135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
  5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
  223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
  129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
  251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
  49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
  138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
  151,160,137,91,90,15,
  131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
  190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
  88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
  77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
  102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
  135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
  5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
  223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
  129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
  251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
  49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
  138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
};

		//---------------------------------------------------------------------

		/*
		 * Helper functions to compute gradients-dot-residualvectors (1D to 4D)
		 * Note that these generate gradients of more than unit length. To make
		 * a close match with the value range of classic Perlin noise, the final
		 * noise values need to be rescaled. To match the RenderMan noise in a
		 * statistical sense, the approximate scaling values (empirically
		 * determined from test renderings) are:
		 * 1D noise needs rescaling with 0.188
		 * 2D noise needs rescaling with 0.507
		 * 3D noise needs rescaling with 0.936
		 * 4D noise needs rescaling with 0.87
		 * Note that these noise functions are the most practical and useful
		 * signed version of Perlin noise. To return values according to the
		 * RenderMan specification from the SL noise() and pnoise() functions,
		 * the noise values need to be scaled and offset to [0,1], like this:
		 * float SLnoise = (Noise1234::noise(x,y,z) + 1.0) * 0.5;
		 */

		float grad(int hash, float x, float y)
		{
			int h = hash & 7;      // Convert low 3 bits of hash code
			float u = h < 4 ? x : y;  // into 8 simple gradient directions,
			float v = h < 4 ? y : x;  // and compute the dot product with (x,y).
			return (((h & 1) != 0) ? -u : u) + (((h & 2) != 0) ? -2.0f * v : 2.0f * v);
		}

		//---------------------------------------------------------------------
		/** 2D float Perlin periodic noise.
		 */
		float pnoise(float x, float y, int px, int py)
		{
			int ix0, iy0, ix1, iy1;
			float fx0, fy0, fx1, fy1;
			float s, t, nx0, nx1, n0, n1;

			ix0 = (int)Mathf.Floor(x); // Integer part of x
			iy0 = (int)Mathf.Floor(y); // Integer part of y
			fx0 = x - ix0;        // Fractional part of x
			fy0 = y - iy0;        // Fractional part of y
			fx1 = fx0 - 1.0f;
			fy1 = fy0 - 1.0f;
			ix1 = ((ix0 + 1) % px) & 0xff;  // Wrap to 0..px-1 and wrap to 0..255
			iy1 = ((iy0 + 1) % py) & 0xff;  // Wrap to 0..py-1 and wrap to 0..255
			ix0 = (ix0 % px) & 0xff;
			iy0 = (iy0 % py) & 0xff;

			t = FADE(fy0);
			s = FADE(fx0);

			nx0 = grad(_permutations[ix0 + _permutations[iy0]], fx0, fy0);
			nx1 = grad(_permutations[ix0 + _permutations[iy1]], fx0, fy1);
			n0 = Mathf.LerpUnclamped(t, nx0, nx1);

			nx0 = grad(_permutations[ix1 + _permutations[iy0]], fx1, fy0);
			nx1 = grad(_permutations[ix1 + _permutations[iy1]], fx1, fy1);
			n1 = Mathf.LerpUnclamped(t, nx0, nx1);

			return 0.507f * (Mathf.LerpUnclamped(s, n0, n1));
		}

	}
}
