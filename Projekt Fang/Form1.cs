using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;


namespace Projekt_Fang
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        System.Drawing.Image imTotxt = null;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage()) { return; }
            if (!(sender is PictureBox)) { return; }

            PictureBox pictureBox = (sender as PictureBox);

            if (pictureBox.SizeMode != PictureBoxSizeMode.Zoom) { pictureBox.SizeMode = PictureBoxSizeMode.Zoom;}
            imTotxt = Clipboard.GetImage();
            pictureBox.Image = imTotxt;
            Clipboard.Clear();

            richTextBox1.Text = imToTxt();
        }
        string imToTxt()
        {

            //using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly))
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var img = Tesseract.PixConverter.ToPix(new Bitmap(imTotxt)))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText();
                    }
                }
            }
        }
    }
}
