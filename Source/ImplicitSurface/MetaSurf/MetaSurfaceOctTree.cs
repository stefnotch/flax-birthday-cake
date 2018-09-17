using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{
	public class MetaSurfaceCell
	{
		public Vector3 Position;
		public float Value;
		public int EdgeXIndex = -1;
		public int EdgeYIndex = -1;
		public int EdgeZIndex = -1;
	}


	// TODO: UNFINISHED!!
	public class MetaSurfaceOctTree
	{
		private MetaSurfaceOctTree[] _children;
		private MetaSurfaceCell _element;

		public MetaSurfaceOctTree(BoundingBox boundingBox)
		{
			BoundingBox = boundingBox;
		}

		public static int MinElementsToSubdivide = 1;

		public BoundingBox BoundingBox { get; private set; }

		public bool IsFinal => _element != null;


		private int _boundingBoxCount = 0;

		/// <summary>
		/// Used for adding an <see cref="ImplicitShape"/>
		/// </summary>
		public void AddBoundingBox(ref BoundingBox boundingBox, ref float resolution)
		{
			//Subdivide

			if (BoundingBox.Intersects(ref boundingBox))
			{
				_boundingBoxCount++;
				if (BoundingBox.Size.X < resolution)
				{

				}

				Subdivide(ref boundingBox);

				foreach (var child in _children)
				{
					AddBoundingBox(ref boundingBox, ref resolution);
				}
			}
		}

		/// <summary>
		/// Used for removing an <see cref="ImplicitShape"/>
		/// </summary>
		public void RemoveBoundingBox(ref BoundingBox boundingBox)
		{
			if (_children.Length == 0) return;

			if (BoundingBox.Intersects(ref boundingBox))
			{
				_boundingBoxCount--;


				//Un-subdivide
				for (int i = 0; i < _children.Length; i++)
				{
					_children[i].RemoveBoundingBox(ref boundingBox);
					if (_children[i]._boundingBoxCount <= 0)
					{
						// Remove child
						_children[i] = null;
					}
				}
			}
		}

		/*public MetaSurfaceCell this[int x, int y, int z]
		{
			get
			{

			}
		}*/

		/// <summary>
		/// Subdivides a gives tree as much as it can
		/// </summary>
		public void SubdivideAll()
		{
			var bb = BoundingBox;
			if (Subdivide(ref bb))
			{
				foreach (var child in _children)
				{
					child.SubdivideAll();
				}
			}
		}

		/// <summary>
		/// Subdivies a cell into 0-8 children
		/// </summary>
		/// <returns>If it succeeded</returns>
		public bool Subdivide(ref BoundingBox boundingBox)
		{
			if (IsFinal) return false;

			// Create the child bounding boxes
			Vector3 center = BoundingBox.Center;
			Vector3[] corners = BoundingBox.GetCorners();

			var children = new MetaSurfaceOctTree[corners.Length];

			for (int i = 0; i < corners.Length; i++)
			{
				Vector3 min = Vector3.Min(corners[i], center);
				Vector3 max = Vector3.Max(corners[i], center);

				BoundingBox childBB = new BoundingBox(min, max);
				if (childBB.Intersects(boundingBox))
				{
					children[i] = new MetaSurfaceOctTree(childBB);
				}
			}

			int childCount = 8;
			// And set their vertices
			for (int i = 0; i < _children.Length; i++)
			{
				var child = _children[i];
				if (child == null)
				{
					childCount--;
					continue;
				}

				/*foreach (var cell in _elements)
				{
					if (child.AddElement(cell)) break;
				}

				if (child.IsEmpty)
				{
					childCount--;
					_children[i] = null;
				}*/
			}

			_children = children;

			return childCount > 0;
		}
	}
}
