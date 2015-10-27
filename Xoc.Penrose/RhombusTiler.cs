//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="RhombusTiler.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the rhombus tiler class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
namespace Xoc.Penrose
{
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Drawing;
	using System.Drawing.Drawing2D;

	/// <summary>A rhombus tiler.</summary>
	public class RhombusTiler
	{
		/// <summary>Establish the number of initial spokes to the wheel.</summary>
		private const int Spokes = 10;

		/// <summary>Initializes a new instance of the <see cref="RhombusTiler"/> class.</summary>
		/// <param name="iterations">The iterations.</param>
		public RhombusTiler(int iterations)
		{
			// Initialize wheel
			PointF a = new PointF(0, 0);
			for (int i = 0; i < Spokes; i++)
			{
				int reverse = i % 2 == 0 ? 1 : -1;
				PointF b = RhombusTiler.PolarToCartesian(1, (float)(((2 * i) - reverse) * Math.PI / Spokes));
				PointF c = RhombusTiler.PolarToCartesian(1, (float)(((2 * i) + reverse) * Math.PI / Spokes));
				Triangle triangle = new Triangle(RhombusType.Fat, a, b, c);
				this.Triangles.Add(triangle);
			}

			for (int i = 0; i < iterations; i++)
			{
				Collection<Triangle> newSet = new Collection<Triangle>();
				foreach (Triangle triangle in this.Triangles)
				{
					Collection<Triangle> subdivided = triangle.Subdivide();
					foreach (Triangle newTriangle in subdivided)
					{
						newSet.Add(newTriangle);
					}
				}

				this.Triangles = newSet;
			}
		}

		/// <summary>Gets the triangles.</summary>
		/// <value>The triangles.</value>
		private Collection<Triangle> Triangles
		{
			get;
		} = new Collection<Triangle>();

		/// <summary>Gets a bitmap with the Penrose tiling in it.</summary>
		/// <param name="size">The size.</param>
		/// <param name="dpi">The DPI.</param>
		/// <returns>The bitmap. The caller must dispose the bitmap.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Returns bitmap")]
		public Bitmap GetBitmap(Size size, int dpi)
		{
			Contract.Ensures(Contract.Result<Bitmap>() != null);
			Bitmap bitmap = new Bitmap(size.Width, size.Height);
			bitmap.SetResolution(dpi, dpi);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				foreach (Triangle triangle in this.Triangles)
				{
					triangle.DrawTriangle(graphics, size, 10000 * (dpi / 300));
				}

				Rectangle rect = new Rectangle(0, 0, size.Width, size.Height);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(
					rect,
					Color.FromArgb(64, Color.Black),
					Color.FromArgb(0, Color.Black),
					LinearGradientMode.Vertical);

				graphics.FillRectangle(linearGradientBrush, rect);
			}

			return bitmap;
		}

		/// <summary>Polar to cartesian coordinate changer.</summary>
		/// <param name="radius">The polar radius.</param>
		/// <param name="angle">The polar angle in radians.</param>
		/// <returns>A PointF.</returns>
		private static PointF PolarToCartesian(float radius, float angle)
		{
			return new PointF((float)(radius * Math.Cos(angle)), (float)(radius * Math.Sin(angle)));
		}

		/// <summary>Object invariant.</summary>
		[Conditional("CONTRACTS_FULL")]
		[ContractInvariantMethod]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Invariant can't be static.")]
		private void ZzObjectInvariant()
		{
			Contract.Invariant(this.Triangles != null);
		}
	}
}