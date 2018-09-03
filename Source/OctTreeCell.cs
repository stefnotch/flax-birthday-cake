using FlaxEngine;
using System;
using System.Collections.Generic;

namespace NinaBirthday
{
	public class OctTreeCell
	{
		public static int MinVerticesToSubdivide = 1;

		private OctTreeCell[] _children;
		private List<Vector3> _vertices;
		private BoundingBox _boundingBox;

		public OctTreeCell(BoundingBox boundingBox)
		{
			_boundingBox = boundingBox;
			_vertices = new List<Vector3>();
		}

		public OctTreeCell(BoundingBox boundingBox, List<Vector3> vertices)
		{
			_boundingBox = boundingBox;
			_vertices = vertices;
		}

		public bool IsEmpty => _vertices.Count == 0;

		public void RecursiveSubdivide()
		{
			// Create the current mesh
			if (this.Subdivide())
			{
				foreach (var child in _children)
				{
					child.RecursiveSubdivide(); // And update the mesh
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
			if (_vertices.Count <= MinVerticesToSubdivide) return false;

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
