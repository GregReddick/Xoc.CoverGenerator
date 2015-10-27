//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the program class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------

// Uncomment these to draw the trim, safe lines, and/or draft on the cover
////#define DRAWTRIM
////#define DRAWSAFE
#define DRAFT

namespace Xoc.CoverGenerator
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using PdfSharp.Drawing;
	using PdfSharp.Pdf;
	using Penrose;
	using Properties;

	/// <summary>A program.</summary>
	internal static class Program
	{
		/// <summary>Gets the page thickness.</summary>
		/// <value>The page thickness in fractions of an inch.</value>
		private static float PageThickness
		{
			get
			{
				float result;

				PageType pageType = Settings.Default.PageType;
				switch (pageType)
				{
					default:
					case PageType.White:
						result = Settings.Default.PageThicknessWhite;
						break;
					case PageType.Cream:
						result = Settings.Default.PageThicknessCream;
						break;
					case PageType.Color:
						result = Settings.Default.PageThicknessColor;
						break;
				}

				return result;
			}
		}

		/// <summary>
		/// Adds text and images to the background image.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="size">The size.</param>
		/// <param name="dpi">The DPI of the cover.</param>
		/// <param name="bookTrim">The book trim.</param>
		/// <param name="bleedSize">Size of the bleed.</param>
		/// <param name="bookPageCount">Number of book pages.</param>
		/// <param name="title">The title.</param>
		/// <param name="author">The author.</param>
		/// <param name="subtitle">The subtitle.</param>
		private static void AddTextAndImages(
			Graphics graphics,
			Size size,
			int dpi,
			SizeF bookTrim,
			float bleedSize,
			int bookPageCount,
			string title,
			string author,
			string subtitle)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			Font fontTitle = null;
			Font fontAuthor = null;
			Font fontSubtitle = null;
			Font fontTitleSpine = null;
			Font fontAuthorSpine = null;
			Font fontBlurb = null;
			Font fontDraft = null;

			float safeBorder = Settings.Default.SafeBorder;
			float safeSpineBorder = Settings.Default.SafeSpineBorder;
			SizeF isbnBlockSize = Settings.Default.IsbnBlockSize;
			string directory = Settings.Default.Directory;
			string isbnFileName = Settings.Default.IsbnFileName;
			string logoFileName = Settings.Default.LogoFileName;
			string draft = Settings.Default.Draft;
			string blurb = Settings.Default.Blurb;
			Contract.Assume(directory != null);
			Contract.Assume(isbnFileName != null);
			Contract.Assume(logoFileName != null);

			// Trim Rectangle
			RectangleF rectTrim = new RectangleF(
				bleedSize * dpi,
				bleedSize * dpi,
				size.Width - (bleedSize * 2 * dpi),
				size.Height - (bleedSize * 2 * dpi));

			// Spine Rectangle
			RectangleF rectSpine = new RectangleF(
				rectTrim.X + (bookTrim.Width * dpi),
				0,
				Program.PageThickness * bookPageCount * dpi,
				size.Height);

			// Safe area of cover back rectangle
			RectangleF rectSafeBack = new RectangleF(
				rectTrim.X + (safeBorder * dpi),
				rectTrim.Y + (safeBorder * dpi),
				(bookTrim.Width - (safeBorder * 2)) * dpi,
				(bookTrim.Height - (safeBorder * 2)) * dpi);

			// Safe area of cover spine rectangle
			RectangleF rectSafeSpine = new RectangleF(
				rectSpine.X + (safeSpineBorder * dpi),
				rectTrim.X + (safeSpineBorder * dpi),
				rectSpine.Width - (safeSpineBorder * 2 * dpi),
				rectTrim.Height - (safeSpineBorder * 2 * dpi));

			// Safe area of cover front rectangle
			RectangleF rectSafeFront = new RectangleF(
				rectSpine.Right + (safeBorder * dpi),
				rectTrim.Y + (safeBorder * dpi),
				(bookTrim.Width - (safeBorder * 2)) * dpi,
				(bookTrim.Height - (safeBorder * 2)) * dpi);

			// Rectangle for the ISBN on cover back
			RectangleF rectIsbn = new RectangleF(
				rectSafeBack.Right - (isbnBlockSize.Width * dpi),
				rectSafeBack.Bottom - (isbnBlockSize.Height * dpi),
				isbnBlockSize.Width * dpi,
				isbnBlockSize.Height * dpi);

			// Rectangle for the blurb on the back
			RectangleF rectBlurb = new RectangleF(
				rectSafeBack.X + (.125f * dpi),
				rectSafeBack.Y + (.125f * dpi),
				rectSafeBack.Width - (.5f * dpi),
				1.6f * dpi);

			try
			{
				fontTitle = new Font("Cambria", 28, FontStyle.Bold);
				fontAuthor = new Font("Cambria", 22);
				fontSubtitle = new Font("Cambria", 18, FontStyle.Italic);
				fontTitleSpine = new Font("Cambria", 18);
				fontAuthorSpine = new Font("Cambria", 12);
				fontBlurb = new Font("Cambria", 12);
				fontDraft = new Font("Cambria", 48);

				// Calculate where text should appear
				SizeF sizeTextTitle = graphics.MeasureString(title, fontTitle);
				SizeF sizeTextTitleSpine = graphics.MeasureString(title, fontTitleSpine);
				SizeF sizeTextAuthor = graphics.MeasureString(author, fontAuthor);
				SizeF sizeTextAuthorSpine = graphics.MeasureString(author, fontAuthorSpine);
				SizeF sizeTextSubtitle = graphics.MeasureString(subtitle, fontSubtitle);
				SizeF sizeTextDraft = graphics.MeasureString(draft, fontDraft);
				PointF pointTextTitle = new PointF(
					rectSafeFront.X + ((rectSafeFront.Width - sizeTextTitle.Width) / 2),
					rectTrim.Top + (2 * dpi));
				PointF pointTextAuthor = new PointF(
					rectSafeFront.X + ((rectSafeFront.Width - sizeTextAuthor.Width) / 2),
					pointTextTitle.Y + (.85F * dpi));
				PointF pointTextSubtitle = new PointF(
					rectSafeFront.X + ((rectSafeFront.Width - sizeTextSubtitle.Width) / 2),
					pointTextAuthor.Y + (1.25F * dpi));
				PointF pointTextDraft = new PointF(
					rectSafeFront.X + ((rectSafeFront.Width - sizeTextDraft.Width) / 2),
					pointTextSubtitle.Y + (1.75F * dpi));

#if DRAWTRIM
				// Draw the trim rectangles
				using (Pen penGreen = new Pen(Color.Green, 2))
				{
					graphics.DrawRectangle(penGreen, rectTrim.X, rectTrim.Y, rectTrim.Width, rectTrim.Height);
					graphics.DrawRectangle(penGreen, rectSpine.X, rectSpine.Y, rectSpine.Width, rectSpine.Height);
				}
#endif

#if DRAWSAFE
				// Draw the safe area rectangles
				using (Pen penBlue = new Pen(Color.Blue, 2))
				{
					graphics.DrawRectangle(penBlue, rectSafeBack.X, rectSafeBack.Y, rectSafeBack.Width, rectSafeBack.Height);
					graphics.DrawRectangle(penBlue, rectSafeSpine.X, rectSafeSpine.Y, rectSafeSpine.Width, rectSafeSpine.Height);
					graphics.DrawRectangle(penBlue, rectSafeFront.X, rectSafeFront.Y, rectSafeFront.Width, rectSafeFront.Height);
				}
#endif

				// Fill ISBN area on the cover with white
				graphics.FillRectangle(Brushes.White, rectIsbn);

				// Draw the text on the Front Cover
				graphics.DrawStringEmbossed(title, fontTitle, Brushes.White, pointTextTitle, sizeTextTitle);
				graphics.DrawStringEmbossed(author, fontAuthor, Brushes.White, pointTextAuthor, sizeTextAuthor);
				graphics.DrawStringEmbossed(subtitle, fontSubtitle, Brushes.White, pointTextSubtitle, sizeTextSubtitle);
#if DRAFT
				graphics.DrawStringEmbossed(draft, fontDraft, Brushes.Black, pointTextDraft, sizeTextDraft);
#endif

				// Draw the text on the spine
				using (StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical))
				{
					RectangleF rectTitleSpine = new RectangleF(
						rectSafeSpine.X + ((rectSafeSpine.Width - sizeTextTitleSpine.Height) / 2),
						rectSafeSpine.Y + (safeSpineBorder * dpi),
						sizeTextTitleSpine.Height,
						sizeTextTitleSpine.Width);
					graphics.DrawStringEmbossed(title, fontTitleSpine, Brushes.White, rectTitleSpine, stringFormat);
					RectangleF rectAuthorSpine = new RectangleF(
						rectSafeSpine.X + ((rectSafeSpine.Width - sizeTextAuthorSpine.Height) / 2),
						rectSafeSpine.Bottom - (safeSpineBorder * dpi) - sizeTextAuthorSpine.Width,
						sizeTextAuthorSpine.Height,
						sizeTextAuthorSpine.Width);
					graphics.DrawStringEmbossed(author, fontAuthorSpine, Brushes.White, rectAuthorSpine, stringFormat);
				}

				graphics.DrawStringEmbossed(blurb, fontBlurb, Brushes.White, rectBlurb.Location, rectBlurb.Size);

				// Get the isbn graphic and draw it on the back
				string isbnFileNameFull = string.Format(CultureInfo.InvariantCulture, isbnFileName, dpi);
				string isbnPathName = Path.Combine(directory, isbnFileNameFull);
				using (Bitmap bitmapIsbn = new Bitmap(isbnPathName))
				{
					graphics.DrawImage(bitmapIsbn, rectIsbn.Left, rectIsbn.Top, rectIsbn.Width, rectIsbn.Height);
				}

				// Get logo graphic and draw it on the back
				string logoFileNameFull = string.Format(CultureInfo.InvariantCulture, logoFileName, dpi);
				string logoPathName = Path.Combine(directory, logoFileNameFull);
				using (Bitmap bitmapLogo = new Bitmap(logoPathName))
				{
					graphics.DrawImage(bitmapLogo, rectSafeBack.X, rectSafeBack.Bottom - bitmapLogo.Height);
				}
			}
			finally
			{
				fontTitle?.Dispose();
				fontAuthor?.Dispose();
				fontSubtitle?.Dispose();
				fontTitleSpine?.Dispose();
				fontAuthorSpine?.Dispose();
				fontBlurb?.Dispose();
				fontDraft?.Dispose();
			}
		}

		/// <summary>Creates a PDF.</summary>
		/// <param name="image">The image.</param>
		/// <param name="fileName">Filename of the file.</param>
		/// <param name="title">The title.</param>
		/// <param name="author">The author.</param>
		/// <param name="subtitle">The subtitle.</param>
		private static void CreatePdf(Image image, string fileName, string title, string author, string subtitle)
		{
			Contract.Requires<ArgumentNullException>(image != null);

			using (PdfDocument pdfDocument = new PdfDocument())
			{
				Contract.Assume(pdfDocument.Info != null);
				pdfDocument.Info.Title = title;
				pdfDocument.Info.Author = author;
				pdfDocument.Info.CreationDate = DateTime.Now;
				pdfDocument.Info.ModificationDate = DateTime.Now;
				pdfDocument.Info.Creator = "Xoc Cover Generator";
				pdfDocument.Info.Subject = subtitle;

				// Create an empty page
				PdfPage pdfPage = pdfDocument.AddPage();
				Contract.Assume(pdfPage != null);
				pdfPage.Orientation = PdfSharp.PageOrientation.Landscape;
				pdfPage.Height = XUnit.FromInch(image.Height / image.VerticalResolution);
				pdfPage.Width = XUnit.FromInch(image.Width / image.HorizontalResolution);

				// Get an XGraphics object for drawing
				XGraphics xgraphics = XGraphics.FromPdfPage(pdfPage);

				XImage ximage = XImage.FromGdiPlusImage(image);
				xgraphics.DrawImage(ximage, 0, 0);

				// Save the document...
				pdfDocument.Save(fileName);
			}
		}

		/// <summary>Main entry-point for this application.</summary>
		private static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.WriteLine(AssemblyInfo.Attribute<AssemblyTitleAttribute>()?.Title);
			Console.WriteLine(AssemblyInfo.Attribute<AssemblyCopyrightAttribute>()?.Copyright);

			string fileName = Path.Combine(Path.GetTempPath(), "cover.pdf");

			int dpi = Settings.Default.Dpi;
			SizeF bookTrim = Settings.Default.BookTrim;
			float bleedSize = Settings.Default.BleedSize;
			int bookPageCount = Settings.Default.BookPageCount;
			string title = Settings.Default.Title;
			string author = Settings.Default.Author;
			string subtitle = Settings.Default.Subtitle;

			RhombusTiler rhomTiler = new RhombusTiler(13);
			Size size = new Size(
				(int)(((bookTrim.Width * 2) + (Program.PageThickness * bookPageCount) + (bleedSize * 2)) * dpi),
				(int)((bookTrim.Height + (bleedSize * 2)) * dpi));

			// Get the background image
			using (Bitmap bitmap = rhomTiler.GetBitmap(size, dpi))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					// Add the elements to the image
					Program.AddTextAndImages(graphics, bitmap.Size, dpi, bookTrim, bleedSize, bookPageCount, title, author, subtitle);
				}

				// Add everything to the pdf
				CreatePdf(bitmap, fileName, title, author, subtitle);

				// View the pdf
				Process.Start(fileName);
			}
		}
	}
}