//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphicsExtensions.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the graphics extensions class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
namespace Xoc.Penrose
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using CoverGenerator.Properties;

	/// <summary>The graphics extensions.</summary>
	internal static class GraphicsExtensions
	{
		/// <summary>The Graphics extension method that draws an embossed string.</summary>
		/// <param name="graphics">The graphics object to act on.</param>
		/// <param name="s">The string to write.</param>
		/// <param name="font">The font to use.</param>
		/// <param name="brush">The brush for the color of the string.</param>
		/// <param name="point">The point that locates the string.</param>
		/// <param name="size">The size of the box (required).</param>
		internal static void DrawStringEmbossed(this Graphics graphics, string s, Font font, Brush brush, PointF point, SizeF size)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);
			Contract.Requires<ArgumentNullException>(font != null);

			graphics.DrawStringEmbossed(
				s,
				font,
				brush,
				new RectangleF(point.X, point.Y, size.Width, size.Height),
				null);
		}

		/// <summary>The Graphics extension method that draws an embossed string.</summary>
		/// <param name="graphics">The graphics object to act on.</param>
		/// <param name="s">The string to write.</param>
		/// <param name="font">The font to use.</param>
		/// <param name="brush">The brush for the color of the string.</param>
		/// <param name="layoutRectangle">The layout rectangle for the string.</param>
		/// <param name="format">Describes the format to use.</param>
		internal static void DrawStringEmbossed(
			this Graphics graphics,
			string s,
			Font font,
			Brush brush,
			RectangleF layoutRectangle,
			StringFormat format)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);
			Contract.Requires<ArgumentNullException>(font != null);

			using (Brush brushSmear = new SolidBrush(Color.FromArgb(96, Color.DarkRed)))
			{
				graphics.FillRoundedRectangle(brushSmear, layoutRectangle, 20);
			}

			RectangleF shadow = layoutRectangle;
			int offset = font.SizeInPoints <= 14 ? Settings.Default.CreateSpaceDpi / 300 : Settings.Default.CreateSpaceDpi / 100;
			shadow.Offset(offset, offset);
			graphics.DrawString(
				s,
				font,
				Brushes.Gray,
				shadow,
				format);
			graphics.DrawString(
				s,
				font,
				brush,
				layoutRectangle,
				format);
		}

		/// <summary>The Graphics extension method that fills a rounded rectangle.</summary>
		/// <param name="graphics">The graphics to act on.</param>
		/// <param name="brush">The brush to draw.</param>
		/// <param name="layoutRectangle">The layout rectangle.</param>
		/// <param name="cornerRadius">The corner radius.</param>
		internal static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF layoutRectangle, int cornerRadius)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddArc(layoutRectangle.X, layoutRectangle.Y, cornerRadius, cornerRadius, 180, 90);
				graphicsPath.AddArc(
					layoutRectangle.X + layoutRectangle.Width - cornerRadius,
					layoutRectangle.Y,
					cornerRadius,
					cornerRadius,
					270,
					90);
				graphicsPath.AddArc(
					layoutRectangle.X + layoutRectangle.Width - cornerRadius,
					layoutRectangle.Y + layoutRectangle.Height - cornerRadius,
					cornerRadius,
					cornerRadius,
					0,
					90);
				graphicsPath.AddArc(
					layoutRectangle.X,
					layoutRectangle.Y + layoutRectangle.Height - cornerRadius,
					cornerRadius,
					cornerRadius,
					90,
					90);
				graphicsPath.CloseAllFigures();
				graphics.FillPath(brush, graphicsPath);
			}
		}
	}
}