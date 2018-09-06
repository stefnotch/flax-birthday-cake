using System;
using System.Collections.Generic;
using FlaxEngine;

namespace NinaBirthday
{
	public class BBOctTreeCell
	{
		public static int MinToSubdivide = 1;

		private OctTreeCell[] _children;
		private List<BoundingBox> _elements;
		private BoundingBox _boundingBox;

		public BBOctTreeCell(BoundingBox cellBoundingBox)
		{
			_boundingBox = cellBoundingBox;
			_elements = new List<BoundingBox>();
		}

		public BBOctTreeCell(BoundingBox cellBoundingBox, List<BoundingBox> elements)
		{
			_boundingBox = cellBoundingBox;
			_elements = elements;
		}

		public bool IsEmpty => _elements.Count == 0;

		//public IEnumerable<List<BoundingBox>> 

		public void RecursiveSubdivide()
		{
			// Create the current mesh
			if (this.Subdivide())
			{
				foreach (var child in _children)
				{
					child.RecursiveSubdivide();
				}
			}
		}

		/// <summary>
		/// Subdivies a cell into 8 children
		/// </summary>
		/// <returns>If it succeeded</returns>
		public bool Subdivide()
		{
			if (_vertices.Count <= 0) return false;
			if (_vertices.Count <= MinToSubdivide) return false;

			// Create the child bounding boxes
			Vector3 center = _boundingBox.Center;
			Vector3[] corners = _boundingBox.GetCorners();

			_children = new OctTreeCell[corners.Length];

			for (int i = 0; i < corners.Length; i++)
			{
				Vector3 min = Vector3.Min(corners[i], center);
				Vector3 max = Vector3.Min(corners[i], center);
				_children[i] = new OctTreeCell(new BoundingBox(min, max));
			}

			// And set their vertices
			foreach (var vertex in _vertices)
			{
				foreach (var child in _children)
				{
					if (AddVertex(vertex)) break;
				}
			}
			return true;
		}

		/// <summary>
		/// Attempts to add a vertex to this <see cref="OctTreeCell"/>
		/// </summary>
		/// <param name="vertex">The vertex to add</param>
		/// <returns>If it succeeded</returns>
		private bool AddVertex(Vector3 vertex)
		{
			if (ContainsVertex(vertex))
			{
				_vertices.Add(vertex);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if this <see cref="OctTreeCell"/> contains the given vertex
		/// </summary>
		/// <param name="vertex">The vertex to check</param>
		/// <returns>If it's a part of this <see cref="OctTreeCell"/></returns>
		private bool ContainsVertex(Vector3 vertex)
		{
			return
				_boundingBox.Minimum.X <= vertex.X && vertex.X < _boundingBox.Maximum.X &&
				_boundingBox.Minimum.Y <= vertex.Y && vertex.Y < _boundingBox.Maximum.Y &&
				_boundingBox.Minimum.Z <= vertex.Z && vertex.Z < _boundingBox.Maximum.Z;
		}
	}
}
