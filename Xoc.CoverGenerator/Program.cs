//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Xoc Software">
// Copyright © 2015 Xoc Software
// </copyright>
// <summary>Implements the program class</summary>
//------------------------------------------------------------------------------------------------------------------------------------------
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

	/// <summary>
	/// The program to draw the book cover to a PDF. A general rule on variable names throughout this class: If a measurement
	/// ends in the suffix "Inches", then the measurement is in inches, otherwise it is in pixels, calibrated for the dpi of
	/// the final image. The dpi (dots per inch) can be 300, 600, 1200, or 2400.
	/// </summary>
	internal static class Program
	{
		/// <summary>Gets the page thickness.</summary>
		/// <value>The page thickness in fractions of an inch.</value>
		private static float PageThicknessInches
		{
			get
			{
				float result;

				PageType pageType = Settings.Default.PageType;
				switch (pageType)
				{
					default:
					case PageType.White:
						result = Settings.Default.PageThicknessWhiteInches;
						break;
					case PageType.Cream:
						result = Settings.Default.PageThicknessCreamInches;
						break;
					case PageType.Color:
						result = Settings.Default.PageThicknessColorInches;
						break;
				}

				return result;
			}
		}

		/// <summary>Adds a back cover text.</summary>
		/// <param name="graphics">The graphics object to draw to.</param>
		/// <param name="dpi">The DPI of the cover.</param>
		/// <param name="rectSafe">The rectangle safe.</param>
		private static void AddBackCoverText(
			Graphics graphics,
			int dpi,
			RectangleF rectSafe)
		{
			Font fontBlurb = null;

			try
			{
				string bookBlurb = Settings.Default.BookBlurb;
				string directoryImages = Settings.Default.DirectoryImages;
				string fileNameIsbn = Settings.Default.FileNameIsbn;
				string fileNameLogo = Settings.Default.FileNameLogo;
				float fontBlurbSize = Settings.Default.FontBlurbSize;
				SizeF sizeIsbnBlockInches = Settings.Default.SizeIsbnBlockInches;
				float marginText = Settings.Default.MarginTextInches * dpi;
				string fontTypeface = Settings.Default.FontTypeface;

				Contract.Assume(directoryImages != null);
				Contract.Assume(fileNameIsbn != null);
				Contract.Assume(fileNameLogo != null);

				SizeF sizeIsbnBlock = new SizeF(sizeIsbnBlockInches.Width * dpi, sizeIsbnBlockInches.Height * dpi);

				// Rectangle for the ISBN on cover back
				RectangleF rectIsbn = new RectangleF(
					rectSafe.Right - marginText - sizeIsbnBlock.Width,
					rectSafe.Bottom - marginText - sizeIsbnBlock.Height,
					sizeIsbnBlock.Width,
					sizeIsbnBlock.Height);

				// Location for the blurb on the back
				PointF pointBlurb = new PointF(rectSafe.X + marginText, rectSafe.Y + marginText);

				fontBlurb = new Font(fontTypeface, fontBlurbSize);

				// Calculate where text should appear
				SizeF sizeTextBlurb = graphics.MeasureString(
					bookBlurb,
					fontBlurb,
					(int)Math.Ceiling(rectSafe.Width - (2 * marginText)));

				graphics.DrawStringEmbossed(bookBlurb, fontBlurb, Brushes.White, pointBlurb, sizeTextBlurb);

				// Get logo graphic and draw it on the back
				string logoFileNameFull = string.Format(CultureInfo.InvariantCulture, fileNameLogo, dpi);
				Contract.Assume(logoFileNameFull != null);
				string logoPathName = Path.Combine(directoryImages, logoFileNameFull);
				if (File.Exists(logoPathName))
				{
					using (Bitmap bitmapLogo = new Bitmap(logoPathName))
					{
						graphics.DrawImage(bitmapLogo, rectSafe.X + marginText, rectSafe.Bottom - marginText - bitmapLogo.Height);
					}
				}

				// Fill ISBN area on the cover with white
				graphics.FillRectangle(Brushes.White, rectIsbn);

				if (Settings.Default.ShowIsbn)
				{
					// Get the isbn graphic and draw it on the back
					string isbnFileNameFull = string.Format(CultureInfo.InvariantCulture, fileNameIsbn, dpi);
					Contract.Assume(isbnFileNameFull != null);
					string isbnPathName = Path.Combine(directoryImages, isbnFileNameFull);
					if (File.Exists(isbnPathName))
					{
						using (Bitmap bitmapIsbn = new Bitmap(isbnPathName))
						{
							graphics.DrawImage(bitmapIsbn, rectIsbn.Left, rectIsbn.Top, rectIsbn.Width, rectIsbn.Height);
						}
					}
				}
			}
			finally
			{
				fontBlurb?.Dispose();
			}
		}

		/// <summary>Adds a front cover text.</summary>
		/// <param name="graphics">The graphics object to draw to.</param>
		/// <param name="dpi">The DPI of the cover.</param>
		/// <param name="rectSafe">The rectangle safe.</param>
		private static void AddFrontCoverText(
			Graphics graphics,
			int dpi,
			RectangleF rectSafe)
		{
			Font fontTitle = null;
			Font fontAuthor = null;
			Font fontSubtitle = null;
			Font fontDraft = null;

			try
			{
				string bookTitle = Settings.Default.BookTitle;
				string bookAuthor = Settings.Default.BookAuthor;
				string bookSubtitle = Settings.Default.BookSubtitle;
				string bookDraftText = Settings.Default.BookDraftText;
				float fontAuthorSize = Settings.Default.FontAuthorSize;
				float fontDraftSize = Settings.Default.FontDraftSize;
				float fontSubtitleSize = Settings.Default.FontSubtitleSize;
				float fontTitleSize = Settings.Default.FontTitleSize;
				string fontTypeface = Settings.Default.FontTypeface;
				float spacingTitle = Settings.Default.SpacingTitleInches * dpi;
				float spacingAuthor = Settings.Default.SpacingAuthorInches * dpi;
				float spacingSubtitle = Settings.Default.SpacingSubtitleInches * dpi;
				float spacingDraft = Settings.Default.SpacingDraftInches * dpi;

				fontAuthor = new Font(fontTypeface, fontAuthorSize);
				fontDraft = new Font(fontTypeface, fontDraftSize);
				fontSubtitle = new Font(fontTypeface, fontSubtitleSize, FontStyle.Italic);
				fontTitle = new Font(fontTypeface, fontTitleSize, FontStyle.Bold);

				SizeF sizeTextAuthor = graphics.MeasureString(bookAuthor, fontAuthor);
				SizeF sizeTextDraft = graphics.MeasureString(bookDraftText, fontDraft);
				SizeF sizeTextSubtitle = graphics.MeasureString(bookSubtitle, fontSubtitle);
				SizeF sizeTextTitle = graphics.MeasureString(bookTitle, fontTitle);

				PointF pointTextTitle = new PointF(
					rectSafe.X + ((rectSafe.Width - sizeTextTitle.Width) / 2),
					rectSafe.Top + spacingTitle);
				PointF pointTextAuthor = new PointF(
					rectSafe.X + ((rectSafe.Width - sizeTextAuthor.Width) / 2),
					pointTextTitle.Y + spacingAuthor);
				PointF pointTextSubtitle = new PointF(
					rectSafe.X + ((rectSafe.Width - sizeTextSubtitle.Width) / 2),
					pointTextAuthor.Y + spacingSubtitle);
				PointF pointTextDraft = new PointF(
					rectSafe.X + ((rectSafe.Width - sizeTextDraft.Width) / 2),
					pointTextSubtitle.Y + spacingDraft);

				// Draw the text on the Front Cover
				graphics.DrawStringEmbossed(bookTitle, fontTitle, Brushes.White, pointTextTitle, sizeTextTitle);
				graphics.DrawStringEmbossed(bookAuthor, fontAuthor, Brushes.White, pointTextAuthor, sizeTextAuthor);
				graphics.DrawStringEmbossed(bookSubtitle, fontSubtitle, Brushes.White, pointTextSubtitle, sizeTextSubtitle);
				if (Settings.Default.ShowDraft)
				{
					// Puts the draft text on the front cover
					graphics.DrawStringEmbossed(bookDraftText, fontDraft, Brushes.Black, pointTextDraft, sizeTextDraft);
				}
			}
			finally
			{
				fontTitle?.Dispose();
				fontAuthor?.Dispose();
				fontSubtitle?.Dispose();
				fontDraft?.Dispose();
			}
		}

		/// <summary>Adds a spine text.</summary>
		/// <param name="graphics">The graphics object to draw to.</param>
		/// <param name="dpi">The DPI of the cover.</param>
		/// <param name="rectSafe">The rectangle safe.</param>
		private static void AddSpineText(
			Graphics graphics,
			int dpi,
			RectangleF rectSafe)
		{
			Font fontTitleSpine = null;
			Font fontAuthorSpine = null;

			try
			{
				string bookTitle = Settings.Default.BookTitle;
				string bookAuthor = Settings.Default.BookAuthor;
				float fontTitleSpineSize = Settings.Default.FontTitleSpineSize;
				float fontAuthorSpineSize = Settings.Default.FontAuthorSpineSize;
				float marginText = Settings.Default.MarginTextInches * dpi;
				string fontTypeface = Settings.Default.FontTypeface;

				fontTitleSpine = new Font(fontTypeface, fontTitleSpineSize);
				fontAuthorSpine = new Font(fontTypeface, fontAuthorSpineSize);

				SizeF sizeTextTitleSpine = graphics.MeasureString(bookTitle, fontTitleSpine);
				SizeF sizeTextAuthorSpine = graphics.MeasureString(bookAuthor, fontAuthorSpine);

				using (StringFormat stringFormat = new StringFormat(StringFormatFlags.DirectionVertical))
				{
					RectangleF rectTitleSpine = new RectangleF(
						rectSafe.X + ((rectSafe.Width - sizeTextTitleSpine.Height) / 2),
						rectSafe.Y + marginText,
						sizeTextTitleSpine.Height,
						sizeTextTitleSpine.Width);
					graphics.DrawStringEmbossed(bookTitle, fontTitleSpine, Brushes.White, rectTitleSpine, stringFormat);
					RectangleF rectAuthorSpine = new RectangleF(
						rectSafe.X + ((rectSafe.Width - sizeTextAuthorSpine.Height) / 2),
						rectSafe.Bottom - marginText - sizeTextAuthorSpine.Width,
						sizeTextAuthorSpine.Height,
						sizeTextAuthorSpine.Width);

					// Draw the text on the spine
					graphics.DrawStringEmbossed(bookAuthor, fontAuthorSpine, Brushes.White, rectAuthorSpine, stringFormat);
				}
			}
			finally
			{
				fontTitleSpine?.Dispose();
				fontAuthorSpine?.Dispose();
			}
		}

		/// <summary>Adds text and images to the background image.</summary>
		/// <param name="graphics">The graphics object to draw to.</param>
		/// <param name="sizeCover">The cover size.</param>
		/// <param name="dpi">The DPI of the cover.</param>
		/// <param name="sizeBookTrim">The book trim.</param>
		/// <param name="spineThickness">The spine thickness DPI.</param>
		/// <param name="sizeBleed">Size of the bleed.</param>
		private static void AddTextAndImages(
			Graphics graphics,
			Size sizeCover,
			int dpi,
			SizeF sizeBookTrim,
			float spineThickness,
			float sizeBleed)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			float marginSafe = Settings.Default.MarginSafeInches * dpi;
			float marginSafeSpine = Settings.Default.MarginSafeSpineInches * dpi;

			// Trim Rectangle
			RectangleF rectTrim = new RectangleF(
				sizeBleed,
				sizeBleed,
				sizeCover.Width - (sizeBleed * 2),
				sizeCover.Height - (sizeBleed * 2));

			// Spine Rectangle
			RectangleF rectSpine = new RectangleF(rectTrim.X + sizeBookTrim.Width, 0, spineThickness, sizeCover.Height);

			// Safe area of cover back rectangle
			RectangleF rectSafeBack = new RectangleF(
				rectTrim.X + marginSafe,
				rectTrim.Y + marginSafe,
				sizeBookTrim.Width - (marginSafe * 2),
				sizeBookTrim.Height - (marginSafe * 2));

			// Safe area of cover spine rectangle
			RectangleF rectSafeSpine = new RectangleF(
				rectSpine.X + marginSafeSpine,
				rectTrim.X + marginSafeSpine,
				rectSpine.Width - (marginSafeSpine * 2),
				rectTrim.Height - (marginSafeSpine * 2));

			// Safe area of cover front rectangle
			RectangleF rectSafeFront = new RectangleF(
				rectSpine.Right + marginSafe,
				rectTrim.Y + marginSafe,
				sizeBookTrim.Width - (marginSafe * 2),
				sizeBookTrim.Height - (marginSafe * 2));

			if (Settings.Default.ShowTrim)
			{
				// Draw the trim rectangles
				using (Pen penGreen = new Pen(Color.Green, dpi / 150))
				{
					graphics.DrawRectangle(penGreen, rectTrim.X, rectTrim.Y, rectTrim.Width, rectTrim.Height);
					graphics.DrawRectangle(penGreen, rectSpine.X, rectSpine.Y, rectSpine.Width, rectSpine.Height);
				}
			}

			if (Settings.Default.ShowSafe)
			{
				// Draw the safe area rectangles
				using (Pen penBlue = new Pen(Color.Blue, dpi / 150))
				{
					graphics.DrawRectangle(penBlue, rectSafeBack.X, rectSafeBack.Y, rectSafeBack.Width, rectSafeBack.Height);
					graphics.DrawRectangle(penBlue, rectSafeSpine.X, rectSafeSpine.Y, rectSafeSpine.Width, rectSafeSpine.Height);
					graphics.DrawRectangle(penBlue, rectSafeFront.X, rectSafeFront.Y, rectSafeFront.Width, rectSafeFront.Height);
				}
			}

			AddFrontCoverText(graphics, dpi, rectSafeFront);
			AddSpineText(graphics, dpi, rectSafeSpine);
			AddBackCoverText(graphics, dpi, rectSafeBack);
		}

		/// <summary>Creates a PDF of the cover from the bitmap using PDFSharp.</summary>
		/// <param name="image">The image.</param>
		private static void CreateSpacePdf(Image image)
		{
			Contract.Requires<ArgumentNullException>(image != null);

			using (PdfDocument pdfDocument = new PdfDocument())
			{
				Contract.Assume(pdfDocument.Info != null);
				string bookTitle = Settings.Default.BookTitle;
				string bookAuthor = Settings.Default.BookAuthor;
				string bookSubtitle = Settings.Default.BookSubtitle;
				pdfDocument.Info.Title = bookTitle;
				pdfDocument.Info.Author = bookAuthor;
				pdfDocument.Info.CreationDate = DateTime.Now;
				pdfDocument.Info.ModificationDate = DateTime.Now;
				pdfDocument.Info.Creator = "Xoc Cover Generator";
				pdfDocument.Info.Subject = bookSubtitle;

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
				string fileName = Path.Combine(Path.GetTempPath(), "cover.pdf");
				pdfDocument.Save(fileName);

				// View the pdf
				Process.Start(fileName);
			}
		}

		/// <summary>Creates kindle TIF.</summary>
		/// <param name="bitmap">The bitmap.</param>
		private static void KindleTif(Bitmap bitmap)
		{
			string fileNameKindle = Path.Combine(Path.GetTempPath(), "coverKindle.tif");

			bitmap.Save(fileNameKindle);

			// View the tif
			Process.Start(fileNameKindle);
		}

		/// <summary>Main entry-point for this application.</summary>
		private static void Main()
		{
			try
			{
				Console.OutputEncoding = Encoding.UTF8;
				Console.WriteLine(AssemblyInfo.Attribute<AssemblyTitleAttribute>()?.Title);
				Console.WriteLine(AssemblyInfo.Attribute<AssemblyCopyrightAttribute>()?.Copyright);

				int dpi = Settings.Default.CreateSpaceDpi;
				SizeF sizeBookTrimInches = Settings.Default.SizeBookTrimInches;
				float marginBleed = Settings.Default.MarginBleedInches * dpi;
				int penroseIterations = Settings.Default.PenroseIterations;
				SizeF sizeBookTrim = new SizeF(sizeBookTrimInches.Width * dpi, sizeBookTrimInches.Height * dpi);
				float spineThickness = Program.PageThicknessInches * Settings.Default.BookPageCount * dpi;

				RhombusTiler rhomTiler = new RhombusTiler(penroseIterations);

				Size sizeCover = new Size(
					(int)(((sizeBookTrim.Width + marginBleed) * 2) + spineThickness),
					(int)(sizeBookTrim.Height + (marginBleed * 2)));

				// Get the background image
				using (Bitmap bitmap = rhomTiler.GetBitmap(sizeCover, dpi))
				{
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						// Add the elements to the image
						Program.AddTextAndImages(
							graphics,
							bitmap.Size,
							dpi,
							sizeBookTrim,
							spineThickness,
							marginBleed);
					}

					// Add bitmap to the pdf
					CreateSpacePdf(bitmap);
				}

				int kindleHeight = Settings.Default.KindleHeight;
				float kindleAspectRatio = Settings.Default.KindleAspectRatio;
				Size sizeCoverKindle = new Size((int)(kindleHeight / kindleAspectRatio), kindleHeight);
				int dpiKindle = (int)(kindleHeight / sizeBookTrimInches.Height);

				// Get the background image
				using (Bitmap bitmap = rhomTiler.GetBitmap(sizeCoverKindle, dpiKindle))
				{
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						// Add the elements to the image
						// Safe area of cover front rectangle
						float marginSafe = Settings.Default.MarginSafeInches * dpiKindle;
						RectangleF rectSafeFront = new RectangleF(
							marginSafe,
							marginSafe,
							bitmap.Width - (marginSafe * 2),
							bitmap.Height - (marginSafe * 2));

						Program.AddFrontCoverText(graphics, dpiKindle, rectSafeFront);
					}

					dpiKindle = Settings.Default.KindleDpi;
					bitmap.SetResolution(dpiKindle, dpiKindle);
					KindleTif(bitmap);
				}
			}
			catch (Exception ex)
			{
				// Generic exception handling at the top of the call stack
				Console.WriteLine(Resources.ErrorMessage, ex.Message);
			}
		}
	}
}