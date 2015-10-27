//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Triangle.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the triangle class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
namespace Xoc.Penrose
{
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.Contracts;
	using System.Drawing;

	/// <summary>A triangle.</summary>
	internal class Triangle
	{
		/// <summary>The fat rhombus brush.</summary>
		private static readonly Brush BrushFatRhombus = new SolidBrush(Color.FromArgb(0x83, 0x15, 0x18));

		/// <summary>The thin rhombus brush.</summary>
		private static readonly Brush BrushThinRhombus = new SolidBrush(Color.FromArgb(0xb3, 0x1c, 0x1f));

		/// <summary>The Golden ratio.</summary>
		private static readonly float Phi = (float)((1 + Math.Sqrt(5)) / 2);

		/// <summary>The offset.</summary>
		private PointF offset;

		/// <summary>The scale.</summary>
		private float scale;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Triangle"/> class.
		/// </summary>
		/// <param name="penroseRhombus">The type of the rhombus (fat or thin).</param>
		/// <param name="a">The A corner of the triangle.</param>
		/// <param name="b">The B corner of the triangle.</param>
		/// <param name="c">The C corner of the triangle.</param>
		internal Triangle(RhombusType penroseRhombus, PointF a, PointF b, PointF c)
		{
			this.RhombusType = penroseRhombus;
			this.A = a;
			this.B = b;
			this.C = c;
		}

		/// <summary>Gets or sets the A corner of the triangle.</summary>
		/// <value>The A corner of the triangle.</value>
		internal PointF A
		{
			get;
			set;
		} = new PointF(0, 0);

		/// <summary>Gets or sets the B corner of the triangle.</summary>
		/// <value>The B corner of the triangle.</value>
		internal PointF B
		{
			get;
			set;
		} = new PointF(0, 0);

		/// <summary>Gets or sets the C corner of the triangle.</summary>
		/// <value>The C corner of the triangle.</value>
		internal PointF C
		{
			get;
			set;
		} = new PointF(0, 0);

		/// <summary>Gets or sets the type of rhombus, fat or thin.</summary>
		/// <value>The type of the rhombus.</value>
		internal RhombusType RhombusType
		{
			get;
			set;
		} = RhombusType.Fat;

		/// <summary>Gets the scaled value of the A corner of the rectangle.</summary>
		/// <value>The A corner scaled.</value>
		private PointF AScale
		{
			get
			{
				return new PointF((this.A.X * this.scale) + this.offset.X, (this.A.Y * this.scale) + this.offset.Y);
			}
		}

		/// <summary>Gets the scaled value of the B corner of the rectangle.</summary>
		/// <value>The B corner scaled.</value>
		private PointF BScale
		{
			get
			{
				return new PointF((this.B.X * this.scale) + this.offset.X, (this.B.Y * this.scale) + this.offset.Y);
			}
		}

		/// <summary>Gets the scaled value of the C corner of the rectangle.</summary>
		/// <value>The C corner scaled.</value>
		private PointF CScale
		{
			get
			{
				return new PointF((this.C.X * this.scale) + this.offset.X, (this.C.Y * this.scale) + this.offset.Y);
			}
		}

		/// <summary>Draw the triangle.</summary>
		/// <param name="graphics">The graphics object to draw on.</param>
		/// <param name="bitmapSize">Size of the bitmap.</param>
		/// <param name="scaleImage">The scale of the image.</param>
		/// <param name="pen">The pen.</param>
		internal void DrawTriangle(Graphics graphics, Size bitmapSize, float scaleImage, Pen pen)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			this.scale = scaleImage;
			this.offset = new PointF(bitmapSize.Width / 2, bitmapSize.Height / 2);

			Brush brush;

			switch (this.RhombusType)
			{
				default:
				case RhombusType.Fat:
					brush = Triangle.BrushFatRhombus;
					break;
				case RhombusType.Thin:
					brush = Triangle.BrushThinRhombus;
					break;
			}

			PointF[] points = new PointF[] { this.AScale, this.BScale, this.CScale };

			graphics.DrawPolygon(pen, points);
			graphics.FillPolygon(brush, points);
		}

		/// <summary>Gets the triangle subdivided into two or three smaller triangles.</summary>
		/// <returns>A Collection of Triangles.</returns>
		internal Collection<Triangle> Subdivide()
		{
			Collection<Triangle> result = new Collection<Triangle>();

			switch (this.RhombusType)
			{
				default:
				case RhombusType.Fat:
					PointF p = new PointF(this.A.X + ((this.B.X - this.A.X) / Phi), this.A.Y + ((this.B.Y - this.A.Y) / Phi));
					result.Add(new Triangle(RhombusType.Fat, this.C, p, this.B));
					result.Add(new Triangle(RhombusType.Thin, p, this.C, this.A));
					break;
				case RhombusType.Thin:
					PointF q = new PointF(this.B.X + ((this.A.X - this.B.X) / Phi), this.B.Y + ((this.A.Y - this.B.Y) / Phi));
					PointF r = new PointF(this.B.X + ((this.C.X - this.B.X) / Phi), this.B.Y + ((this.C.Y - this.B.Y) / Phi));
					result.Add(new Triangle(RhombusType.Thin, r, this.C, this.A));
					result.Add(new Triangle(RhombusType.Thin, q, r, this.B));
					result.Add(new Triangle(RhombusType.Fat, r, q, this.A));
					break;
			}

			return result;
		}
	}
}