using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace FontViewer {
    public partial class MainForm : Form {
        private List<Bitmap> bitmaps;
        private int curPage;
        public MainForm() {
            InitializeComponent();
            refresh();
        }

        private void savePdf(string fileName) {
            if (bitmaps == null || bitmaps.Count == 0) {
                Utils.Utils.errMsg("No images");
                return;
            }
            int nPages = bitmaps.Count;
            PdfWriter writer = new PdfWriter(fileName);
            PdfDocument pdf = new PdfDocument(writer);
            // Units are points (y default)
            float pageWidth = 8.5f * 72;
            float pageHeight = 11.0f * 72;
            iText.Kernel.Geom.Rectangle pagesize =
                new iText.Kernel.Geom.Rectangle(pageWidth, pageHeight);
            float margin = 0.5f * 72;
            Document document = new Document(pdf, new PageSize(pagesize));
            document.SetMargins(margin, margin, margin, margin);
            DateTime now = DateTime.Now;
            for (int page = 0; page < nPages; page++) {
                // Header
                Paragraph paragraph = new Paragraph(Environment.MachineName
                    + " Installed Fonts"
                    + " [" + now + "]");
                paragraph.Add(new Tab());
                paragraph.AddTabStops(new TabStop(1000,
                    iText.Layout.Properties.TabAlignment.RIGHT));
                paragraph.Add("Page " + (page + 1) + " of " + nPages);
                paragraph.SetBold().SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(10);
                document.Add(paragraph);

                System.Drawing.Image image = bitmaps[page];
                if (image == null) {
                    Paragraph error = new Paragraph("Blank")
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetFontSize(12);
                    document.Add(error);
                } else {
                    byte[] bytes = (byte[])(new ImageConverter())
                        .ConvertTo(image, typeof(byte[]));
                    ImageData imageData = ImageDataFactory.Create(bytes);
                    iText.Layout.Element.Image pdfImage =
                        new iText.Layout.Element.Image(imageData)
                        .SetTextAlignment(TextAlignment.CENTER);
                    pdfImage.ScaleToFit(pageWidth - 2 * margin, pageHeight - 2 * margin);
                    document.Add(pdfImage);
                }
                if (page < nPages - 1) {
                    document.Add(new AreaBreak());
                }
            }
            document.Close();
        }

        private void refresh() {
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFontCollection.Families;
            int fontSize = 40; // In pixels = 9.6 pts at 300 dpi
            int nFonts = fontFamilies.Length;
            int defaultHeight = 50;
            int margin = 75;
            int width = 2550, height = 3150;  // 7.5" x 10.5"
            int nCols = 5;
            int deltaY = defaultHeight;
            int deltaX = (width - 2 * margin) / nCols;
            int itemsPerPage = (height - 2 * margin) / defaultHeight * nCols;
            int itemsPerColumn = itemsPerPage / nCols;
            int nPages = (int)Math.Ceiling((double)nFonts / (double)itemsPerColumn / (double)nCols);

            bitmaps = new List<Bitmap>();
            for (int page = 0; page < nPages; page++) {
                SolidBrush solidBrush = new SolidBrush(Color.Black);
                Bitmap image = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(image);
                System.Drawing.SolidBrush brush;
                // Fill with white
                using (brush = new SolidBrush(Color.White)) {
                    g.FillRectangle(brush, 0, 0, width, height);
                }
                brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                RectangleF rectF;

                // DEBUG
                //using (Font font = new System.Drawing.Font("Helvetica", fontSize)) {
                //    rectF = new RectangleF(margin, margin, deltaX, defaultHeight);
                //    using (brush = new SolidBrush(Color.Red)) {
                //        g.FillRectangle(brush, rectF);
                //    }
                //    g.DrawString("This is a test", font, solidBrush, rectF);
                //}

                int xOffset = 0;
                int yOffset;
                int jStart = page * itemsPerPage;
                int jEnd = (page + 1) * itemsPerPage;
                if (jEnd > nFonts) jEnd = nFonts;
                for (int j = jStart; j < jEnd; ++j) {
                    yOffset = ((j - jStart) % itemsPerColumn) * deltaY;
                    if (j != jStart && (j % itemsPerColumn) == 0) {
                        xOffset += deltaX;
                    }
                    using (Font font = new System.Drawing.Font(fontFamilies[j], fontSize, GraphicsUnit.Pixel)) {
                        rectF = new RectangleF(margin + xOffset, margin + yOffset, deltaX, deltaY);
                        g.DrawString(fontFamilies[j].Name, font, solidBrush, rectF);
                    }
                }
                brush.Dispose();
                g.Dispose();
                bitmaps.Add(image);
                if (page == 0) {
                    curPage = page;
                    pictureBox.Image = image;
                }
            }
        }

        private void OnQuitClicked(object sender, EventArgs e) {
            Close();
        }

        private void OnRefreshClicked(object sender, EventArgs e) {
            refresh();
        }

        private void OnSaveImageClicked(object sender, EventArgs e) {
            if (pictureBox == null | pictureBox.Image == null) return;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "JPEG|*.jpg";
            dlg.Title = "Save Image as JPEG";
            dlg.FileName = "Fonts.jpg";
            dlg.CheckFileExists = true;
            string fileName = dlg.FileName;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    pictureBox.Image.Save(fileName, ImageFormat.Jpeg);
                } catch (Exception ex) {
                    Utils.Utils.excMsg("Error saving JPEG", ex);
                }
            }

        }
        private void OnSavePdfClicked(object sender, EventArgs e) {
            if (bitmaps == null || bitmaps.Count == 0) {
                Utils.Utils.errMsg("No images");
                return;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PDF|*.pdf";
            dlg.Title = "Save Image as PDF";
            dlg.FileName = "Fonts.pdf";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string fileName = dlg.FileName;
                try {
                    savePdf(fileName);
                } catch (Exception ex) {
                    Utils.Utils.excMsg("Error saving PDF", ex);
                }
            }

        }
        private void OnBackClicked(object sender, EventArgs e) {
            if (bitmaps == null || bitmaps.Count == 0) return;
            if (curPage > 0) {
                curPage--;
                if (curPage < 0) curPage = 0;
                System.Drawing.Image image = bitmaps[curPage];
                pictureBox.Image = image;
            }
        }
        private void OnForwardClicked(object sender, EventArgs e) {
            if (bitmaps == null || bitmaps.Count == 0) return;
            if (curPage < bitmaps.Count - 1) {
                curPage++;
                if (curPage >= bitmaps.Count()) curPage = bitmaps.Count - 1;
                System.Drawing.Image image = bitmaps[curPage];
                pictureBox.Image = image;
            }
        }
    }
}
