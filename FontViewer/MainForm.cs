using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace FontViewer {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void createPdfDemo(string fileName) {
            System.Drawing.Image image = pictureBox.Image;
            if (image == null) {
                Utils.Utils.errMsg("No image");
                return;
            }
            byte[] bytes = (byte[])(new ImageConverter()).ConvertTo(image, typeof(byte[]));
            ImageData imageData = ImageDataFactory.Create(bytes);
            iText.Layout.Element.Image pdfImage =
                new iText.Layout.Element.Image(imageData).SetTextAlignment(TextAlignment.CENTER);
            //ScaleAbsolute(100, 200).SetFixedPosition(1, 25, 25);
            PdfWriter writer = new PdfWriter(fileName);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            //Paragraph header = new Paragraph("HEADER")
            //   .SetTextAlignment(TextAlignment.CENTER)
            //   .SetFontSize(12);
            //document.Add(header);
            //Paragraph subheader = new Paragraph("SUB HEADER")
            //   .SetTextAlignment(TextAlignment.CENTER)
            //   .SetFontSize(10);
            //document.Add(subheader);
            document.Add(pdfImage);
            document.Close();
        }

        private void refresh() {
            FontFamily fontFamily = new FontFamily("Arial");
            SolidBrush solidBrush = new SolidBrush(Color.Black);
            // 8 x 10.5 @ 300 dpi
            int width = 2400, height = 3150;
            Bitmap image = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(image);
            System.Drawing.SolidBrush brush;
            // Fill with white
            using (brush = new SolidBrush(Color.White)) {
                g.FillRectangle(brush, 0, 0, width, height);
            }
            brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFontCollection.Families;
            int nFonts = fontFamilies.Length;
            RectangleF rectF;
            int nItems = nFonts;
            int defaultHeight = 50;
            int margin = 20;
            int nCols = 5;
            int deltaY = (height - 2 * margin) / nItems;
            int deltaX = (width - 2 * margin) / nCols;
            if (deltaY < defaultHeight) {
                deltaY = defaultHeight;
                nItems = (height - 2 * margin) / defaultHeight * nCols;
            }
            // DEBUG
            //using (Font font = new System.Drawing.Font("Helvetica", 10)) {
            //    rectF = new RectangleF(margin, margin, deltaX, defaultHeight);
            //    using (brush = new SolidBrush(Color.Red)) {
            //        g.FillRectangle(brush, rectF);
            //    }
            //    g.DrawString("This is a test", font, solidBrush, rectF);
            //}

            int xOffset = 0;
            int yOffset = 0;
            int itemsPerColumn = nItems / nCols;
            for (int j = 0; j < nItems; ++j) {
                yOffset = (j % itemsPerColumn) * deltaY;
                if (j != 0 && (j % itemsPerColumn) == 0) {
                    xOffset += deltaX;
                }
                using (Font font = new System.Drawing.Font(fontFamilies[j], 10)) {
                    rectF = new RectangleF(margin + xOffset, margin + yOffset, deltaX, deltaY);
                    g.DrawString(fontFamilies[j].Name, font, solidBrush, rectF);
                }
            }
            brush.Dispose();
            g.Dispose();
            pictureBox.Image = image;
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
            if (pictureBox == null | pictureBox.Image == null) return;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PDF|*.pdf";
            dlg.Title = "Save Image as PDF";
            dlg.FileName = "Fonts.pdf";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string fileName = dlg.FileName;
                try {
                    createPdfDemo(fileName);
                } catch (Exception ex) {
                    Utils.Utils.excMsg("Error saving PDF", ex);
                }
            }

        }
    }
}
