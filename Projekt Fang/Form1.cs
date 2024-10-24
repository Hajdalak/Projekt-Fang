using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tesseract;



namespace Projekt_Fang
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            comboBox1.Items.Add("eng");
            comboBox1.Items.Add("ces");
            comboBox1.Items.Add("lat");
            comboBox1.Text = comboBox1.Items[1].ToString();

            saveSettingsWS.SettingsInitilize(panel1.Controls);
        }
        //Image imSa = null;
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage()) { return; }
            if (!(sender is PictureBox)) { return; }

            PictureBox pictureBox = (sender as PictureBox);

            if (pictureBox.SizeMode != PictureBoxSizeMode.Zoom) { pictureBox.SizeMode = PictureBoxSizeMode.Zoom; }
            Bitmap scImage = (Bitmap)Clipboard.GetImage();
            pictureBox.Image = scImage;
            //Clipboard.Clear();
            string radek = imToTxt((Bitmap)scImage.Clone()).Replace("\n", " ");
            radek = radek.Replace("  ", "\n");
            radek = radek.Replace("\n\n", "\n");
            richTextBox1.Text = radek;
        }

        string imToTxt(Bitmap obraz)
        {
            //using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractOnly))
            TesseractEngine engine = new TesseractEngine(@"./tessdata", comboBox1.Text, EngineMode.Default);

            Pix img = PixConverter.ToPix(obraz);
            Page page = engine.Process(img);
            ResultIterator iter = page.GetIterator();

            iter.Begin();
            do
            {
                if (iter.IsAtBeginningOf(PageIteratorLevel.Word))
                {
                    Rect bounds;
                    if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out bounds))
                    {
                        /* 
                         Console.WriteLine($"Word: {iter.GetText(PageIteratorLevel.Word)}");
                         Console.WriteLine($"BoundingBox: X={bounds.X1}, Y={bounds.Y1}, Width={bounds.Width}, Height={bounds.Height}");
                        */
                        if (!(iter.GetText(PageIteratorLevel.Word).Any(char.IsLetter)))
                        {
                            continue;
                        }
                        Graphics g = Graphics.FromImage(obraz);
                        Brush whiteBrush = new SolidBrush(Color.White);
                        g.FillRectangle(whiteBrush, bounds.X1, bounds.Y1, bounds.Width, bounds.Height);
                    }
                }
            } while (iter.Next(PageIteratorLevel.Word));
            pictureBox2.Image = obraz;
            return page.GetText();
        }
        SaveSettingsWS saveSettingsWS = new SaveSettingsWS();
        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = saveSettingsWS.betterFolderSelection();
            saveSettingsWS.SettingsSave(panel1.Controls);
        }

        string sQpathFolder = "C:\\Users\\kaktu\\Desktop\\Azk";
        int imR = 0;

        void LoadQ(string sQpathFolder, int qInt)
        {
            StreamReader mugin = new StreamReader(sQpathFolder+ "\\qdtb.txt");
            string nnn = mugin.ReadToEnd();
            nnn = sQpathFolder + "\\" + nnn.Split('\n')[0].Split('_')[0];
            Console.WriteLine("neco :  "+nnn);
            pictureBox1.Image = new Bitmap(nnn); 
        }
        void SaveQ(Bitmap bmp, List<string> deleni)
        {
            if (bmp == null) { return; }
            sQpathFolder =  textBox2.Text;
            // fotka, deleni1, deleni2 atd
            if (imR == 0) { imR =foundNMIm(); }
            string cestaKIm = $"{imR:0000}.jpg";
            imR++;
            bmp.Save(sQpathFolder + "\\" + cestaKIm);

            

            string zapKQ = $"{cestaKIm}_";
            foreach (string s in deleni)
            {
                zapKQ += $"{s}_";
            }
            zapKQ = zapKQ.Substring(0, zapKQ.Length - 1);
            StreamWriter hugin = new StreamWriter(sQpathFolder + "\\qdtb.txt", true);
            hugin.WriteLine(zapKQ);
            hugin.Close();

            
            int foundNMIm()
            {
                List<int> jpgFiles = Directory.GetFiles(sQpathFolder, "*.jpg")
                .Select(Path.GetFileNameWithoutExtension) // Get the file names without extensions
                .Where(name => int.TryParse(name, out _)) // Filter out non-numeric names
                .Select(name => int.Parse(name)) // Convert the file names to integers
                .ToList();

                if (jpgFiles.Any())
                {
                   return jpgFiles.Max(); // Find the highest number 
                }
                return 0;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> deleni = new List<string>();
            deleni.Add("01Kosti");
            SaveQ((Bitmap)pictureBox1.Image,deleni);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadQ(sQpathFolder, 0);
        }
    }
}
