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
	using System.Drawing.Imaging;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using PdfSharp.Drawing;
	using PdfSharp.Pdf;
	using Penrose;
	using Properties;

	/// <summary>
	/// The program to draw the book cover. Draws to both a CreateSpace pdf and a Kindle tif. These are brought up in viewers
	/// and must be saved. A general rule on variable names throughout this class: If a measurement ends in the suffix
	/// "Inches", then the measurement is in inches, otherwise it is in pixels, calibrated for the dpi of the image. The dpi
	/// (dots per inch) can be 300, 600, 1200, or 2400 for the CoverSpace cover and anything for the Kindle cover. All
	/// aspects of the covers are set in the set in .config file.
	/// </summary>
	internal static class Program
	{
		/// <summary>The CreateSpace file name.</summary>
		private const string FileNameCreateSpace = "createspacecover.pdf";

		/// <summary>The Kindle file name.</summary>
		private const string FileNameKindle = "kindlecover.tif";

		/// <summary>Gets the page thickness, which depends on the page type.</summary>
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
		/// <param name="rectSafe">The back cover safe rectangle.</param>
		private static void AddBackCoverText(Graphics graphics, int dpi, RectangleF rectSafe)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			Font fontBlurb = null;

			try
			{
				float marginText = Settings.Default.MarginTextInches * dpi;
				string directoryImages = Settings.Default.DirectoryImages;
				Contract.Assume(directoryImages != null);

				// Draw the blurb
				string bookBlurb = Settings.Default.BookBlurb;
				PointF pointBlurb = new PointF(rectSafe.X + marginText, rectSafe.Y + marginText);
				fontBlurb = new Font(Settings.Default.FontTypeface, Settings.Default.FontBlurbSize);
				SizeF sizeTextBlurb = graphics.MeasureString(bookBlurb, fontBlurb, (int)Math.Ceiling(rectSafe.Width - (2 * marginText)));
				graphics.DrawStringEmbossed(bookBlurb, fontBlurb, Brushes.White, pointBlurb, sizeTextBlurb);

				// Draw the logo
				string logoFileNameFull = string.Format(CultureInfo.InvariantCulture, Settings.Default.FileNameLogo, dpi);
				Contract.Assume(logoFileNameFull != null);
				string logoPathName = Path.Combine(directoryImages, logoFileNameFull);
				if (File.Exists(logoPathName))
				{
					using (Bitmap bitmapLogo = new Bitmap(logoPathName))
					{
						graphics.DrawImage(bitmapLogo, rectSafe.X + marginText, rectSafe.Bottom - marginText - bitmapLogo.Height);
					}
				}

				// Draw the ISBN
				SizeF sizeIsbnBlockInches = Settings.Default.SizeIsbnBlockInches;
				SizeF sizeIsbnBlock = new SizeF(sizeIsbnBlockInches.Width * dpi, sizeIsbnBlockInches.Height * dpi);
				RectangleF rectIsbn = new RectangleF(
					rectSafe.Right - marginText - sizeIsbnBlock.Width,
					rectSafe.Bottom - marginText - sizeIsbnBlock.Height,
					sizeIsbnBlock.Width,
					sizeIsbnBlock.Height);

				graphics.FillRectangle(Brushes.White, rectIsbn);

				if (Settings.Default.ShowIsbn)
				{
					// Get the isbn graphic and draw it on the back. If not done, CoverSpace will automatically add an ISBN barcode to the
					// white block.
					string isbnFileNameFull = string.Format(CultureInfo.InvariantCulture, Settings.Default.FileNameIsbn, dpi);
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
		/// <param name="rectSafe">The front cover safe rectangle.</param>
		private static void AddFrontCoverText(Graphics graphics, int dpi, RectangleF rectSafe)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			Font fontTitle = null;
			Font fontAuthor = null;
			Font fontSubtitle = null;
			Font fontDraft = null;

			try
			{
				string fontTypeface = Settings.Default.FontTypeface;
				string bookTitle = Settings.Default.BookTitle;
				string bookAuthor = Settings.Default.BookAuthor;
				string bookSubtitle = Settings.Default.BookSubtitle;
				string bookDraftText = Settings.Default.BookDraftText;
				float spacingTitle = Settings.Default.SpacingTitleInches * dpi;
				float spacingAuthor = Settings.Default.SpacingAuthorInches * dpi;
				float spacingSubtitle = Settings.Default.SpacingSubtitleInches * dpi;
				float spacingDraft = Settings.Default.SpacingDraftInches * dpi;

				fontAuthor = new Font(fontTypeface, Settings.Default.FontAuthorSize);
				fontDraft = new Font(fontTypeface, Settings.Default.FontDraftSize);
				fontSubtitle = new Font(fontTypeface, Settings.Default.FontSubtitleSize, FontStyle.Italic);
				fontTitle = new Font(fontTypeface, Settings.Default.FontTitleSize, FontStyle.Bold);

				SizeF sizeTextAuthor = graphics.MeasureString(bookAuthor, fontAuthor);
				SizeF sizeTextDraft = graphics.MeasureString(bookDraftText, fontDraft);
				SizeF sizeTextSubtitle = graphics.MeasureString(bookSubtitle, fontSubtitle);
				SizeF sizeTextTitle = graphics.MeasureString(bookTitle, fontTitle);

				PointF pointTextTitle = new PointF(rectSafe.X + ((rectSafe.Width - sizeTextTitle.Width) / 2), rectSafe.Top + spacingTitle);
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
		/// <param name="rectSafe">The spine safe rectangle.</param>
		private static void AddSpineText(Graphics graphics, int dpi, RectangleF rectSafe)
		{
			Contract.Requires<ArgumentNullException>(graphics != null);

			Font fontTitleSpine = null;
			Font fontAuthorSpine = null;

			try
			{
				string fontTypeface = Settings.Default.FontTypeface;
				string bookTitle = Settings.Default.BookTitle;
				string bookAuthor = Settings.Default.BookAuthor;
				float marginText = Settings.Default.MarginTextInches * dpi;

				fontTitleSpine = new Font(fontTypeface, Settings.Default.FontTitleSpineSize);
				fontAuthorSpine = new Font(fontTypeface, Settings.Default.FontAuthorSpineSize);

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

			RectangleF rectTrim = new RectangleF(
				sizeBleed,
				sizeBleed,
				sizeCover.Width - (sizeBleed * 2),
				sizeCover.Height - (sizeBleed * 2));

			RectangleF rectSpine = new RectangleF(rectTrim.X + sizeBookTrim.Width, 0, spineThickness, sizeCover.Height);

			RectangleF rectSafeBack = new RectangleF(
				rectTrim.X + marginSafe,
				rectTrim.Y + marginSafe,
				sizeBookTrim.Width - (marginSafe * 2),
				sizeBookTrim.Height - (marginSafe * 2));

			RectangleF rectSafeSpine = new RectangleF(
				rectSpine.X + marginSafeSpine,
				rectTrim.X + marginSafeSpine,
				rectSpine.Width - (marginSafeSpine * 2),
				rectTrim.Height - (marginSafeSpine * 2));

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
				// Draw the safe area rectangles (areas where text cannot appear)
				using (Pen penBlue = new Pen(Color.Blue, dpi / 150))
				{
					graphics.DrawRectangle(penBlue, rectSafeBack.X, rectSafeBack.Y, rectSafeBack.Width, rectSafeBack.Height);
					graphics.DrawRectangle(penBlue, rectSafeSpine.X, rectSafeSpine.Y, rectSafeSpine.Width, rectSafeSpine.Height);
					graphics.DrawRectangle(penBlue, rectSafeFront.X, rectSafeFront.Y, rectSafeFront.Width, rectSafeFront.Height);
				}
			}

			Program.AddFrontCoverText(graphics, dpi, rectSafeFront);
			Program.AddSpineText(graphics, dpi, rectSafeSpine);
			Program.AddBackCoverText(graphics, dpi, rectSafeBack);
		}

		/// <summary>Creates a PDF of the cover from the bitmap using PDFSharp.</summary>
		/// <param name="image">The image.</param>
		/// <returns>The new space PDF.</returns>
		private static string CreateSpacePdf(Image image)
		{
			Contract.Requires<ArgumentNullException>(image != null);

			using (PdfDocument pdfDocument = new PdfDocument())
			{
				Contract.Assume(pdfDocument.Info != null);
				pdfDocument.Info.Title = Settings.Default.BookTitle;
				pdfDocument.Info.Author = Settings.Default.BookAuthor;
				pdfDocument.Info.CreationDate = DateTime.Now;
				pdfDocument.Info.ModificationDate = DateTime.Now;
				pdfDocument.Info.Creator = AssemblyInfo.Attribute<AssemblyTitleAttribute>()?.Title;
				pdfDocument.Info.Subject = Settings.Default.BookSubtitle;

				// Create an empty page
				PdfPage pdfPage = pdfDocument.AddPage();
				Contract.Assume(pdfPage != null);
				pdfPage.Orientation = PdfSharp.PageOrientation.Landscape;
				pdfPage.Height = XUnit.FromInch(image.Height / image.VerticalResolution);
				pdfPage.Width = XUnit.FromInch(image.Width / image.HorizontalResolution);

				// Draw the bitmap on the page
				XGraphics xgraphics = XGraphics.FromPdfPage(pdfPage);
				XImage ximage = XImage.FromGdiPlusImage(image);
				xgraphics.DrawImage(ximage, 0, 0);

				// Save the document...
				string fileName = Path.Combine(Path.GetTempPath(), FileNameCreateSpace);
				pdfDocument.Save(fileName);

				return fileName;
			}
		}

		/// <summary>Main entry-point for this application.</summary>
		private static void Main()
		{
			try
			{
				Console.OutputEncoding = Encoding.UTF8;
				Console.WriteLine(AssemblyInfo.Attribute<AssemblyTitleAttribute>()?.Title);
				Console.WriteLine(AssemblyInfo.Attribute<AssemblyCopyrightAttribute>()?.Copyright);

				int penroseIterations = Settings.Default.PenroseIterations;
				RhombusTiler rhombusTiler = new RhombusTiler(penroseIterations);

				SizeF sizeBookTrimInches = Settings.Default.SizeBookTrimInches;

				Program.MakeCreateSpaceCover(rhombusTiler, sizeBookTrimInches);
				Program.MakeKindleCover(rhombusTiler, sizeBookTrimInches);
			}
			catch (Exception ex)
			{
				// Generic exception handling at the top of the call stack
				Console.WriteLine(Resources.ErrorMessage, ex.Message);
			}
		}

		/// <summary>Makes create space cover.</summary>
		/// <param name="rhombusTiler">The rhombus tiler.</param>
		/// <param name="sizeBookTrimInches">The size book trim inches.</param>
		private static void MakeCreateSpaceCover(RhombusTiler rhombusTiler, SizeF sizeBookTrimInches)
		{
			Contract.Requires<ArgumentNullException>(rhombusTiler != null);

			int dpi = Settings.Default.CreateSpaceDpi;
			float marginBleed = Settings.Default.MarginBleedInches * dpi;
			SizeF sizeBookTrim = new SizeF(sizeBookTrimInches.Width * dpi, sizeBookTrimInches.Height * dpi);
			float spineThickness = Program.PageThicknessInches * Settings.Default.BookPageCount * dpi;

			Size sizeCover = new Size(
				(int)(((sizeBookTrim.Width + marginBleed) * 2) + spineThickness),
				(int)(sizeBookTrim.Height + (marginBleed * 2)));

			// Get the background image
			using (Bitmap bitmap = rhombusTiler.GetBitmap(sizeCover, dpi))
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

				// Create the pdf
				string fileName = Program.CreateSpacePdf(bitmap);

				// View the pdf
				Process.Start(fileName);
			}
		}

		/// <summary>Makes kindle cover.</summary>
		/// <param name="rhombusTiler">The rhombus tiler.</param>
		/// <param name="sizeBookTrimInches">The size book trim inches.</param>
		private static void MakeKindleCover(RhombusTiler rhombusTiler, SizeF sizeBookTrimInches)
		{
			Contract.Requires<ArgumentNullException>(rhombusTiler != null);

			int kindleHeight = Settings.Default.KindleHeight;
			float kindleAspectRatio = Settings.Default.KindleAspectRatio;
			Size sizeCoverKindle = new Size((int)(kindleHeight / kindleAspectRatio), kindleHeight);
			int dpiKindle = (int)(kindleHeight / sizeBookTrimInches.Height);

			// Get the background image
			using (Bitmap bitmap = rhombusTiler.GetBitmap(sizeCoverKindle, dpiKindle))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
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

				string fileNameKindle = Path.Combine(Path.GetTempPath(), FileNameKindle);
				bitmap.Save(fileNameKindle, ImageFormat.Tiff);

				// View the tif
				Process.Start(fileNameKindle);
			}
		}
	}
}