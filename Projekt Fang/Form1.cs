using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tesseract;

namespace Projekt_Fang
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            comboBox1.Items.Add("eng");
            comboBox1.Items.Add("ces");
            comboBox1.Items.Add("lat");
            comboBox1.Text = comboBox1.Items[1].ToString();

            saveSettingsWS.SettingsInitilize(panel1.Controls);
            imageClipboard();
        }
        bool das = false;
        void imageClipboard()
        {
            das = true;
            Thread BD = new Thread(prtsc);
            BD.IsBackground = true;
            BD.Start();
            void prtsc()
            {
                while (das)
                {
                    if (Clipboard.ContainsImage())
                    {
                        List<string> list = new List<string>();
                        list.Add(textBox1.Text);
                        pictureBox1.Image = (Bitmap)Clipboard.GetImage();
                        imToTxt((Bitmap)pictureBox1.Image.Clone());
                    }
                }

            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage()) { return; }
            if (!(sender is PictureBox)) { return; }

            PictureBox pictureBox = (sender as PictureBox);

            if (pictureBox.SizeMode != PictureBoxSizeMode.Zoom) { pictureBox.SizeMode = PictureBoxSizeMode.Zoom; }
            Bitmap scImage = (Bitmap)Clipboard.GetImage();
            pictureBox.Image = scImage;
            //Clipboard.Clear();
            /*string radek = imToTxt((Bitmap)scImage.Clone()).Replace("\n", " ");
            radek = radek.Replace("  ", "\n");
            radek = radek.Replace("\n\n", "\n");
            richTextBox1.Text = radek;*/
            pictureBox2.Image = imToTxt((Bitmap)scImage);
        }

        Bitmap imToTxt(Bitmap bmp)
        {
            Bitmap obraz = (Bitmap)bmp.Clone();
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
            //pictureBox2.Image = obraz;
            return obraz;//page.GetText();
        }
        SaveSettingsWS saveSettingsWS = new SaveSettingsWS();
        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = saveSettingsWS.betterFolderSelection();
            saveSettingsWS.SettingsSave(panel1.Controls);
        }

        string sQpathFolder = "C:\\Users\\kaktu\\Desktop\\Azk";
        int imR = 0;

        void SaveQ(Bitmap bmp, List<string> deleni)
        {
            if (bmp == null) { return; }
            sQpathFolder = textBox2.Text;
            // fotka, deleni1, deleni2 atd
            if (imR == 0) { imR = foundNMIm(); }
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
            SaveQ((Bitmap)pictureBox1.Image, deleni);
        }

        string[] obrazky = { };

        int kolikaty = 0;
        private void button5_Click(object sender, EventArgs e)
        {
            kolikaty++;
            pictureBox3.Image = imToTxt(new Bitmap(obrazky[kolikaty]));
            button6.Text = "SHOW";
        }
        bool showHide = false;
        private void button6_Click(object sender, EventArgs e)
        {
            showHide = !showHide;

            if (showHide) 
            { 
                pictureBox3.Image = new Bitmap(obrazky[kolikaty]); 
                (sender as Button).Text = "HIDE"; 
            }
            else 
            {
                pictureBox3.Image = imToTxt(new Bitmap(obrazky[kolikaty]));
                (sender as Button).Text = "SHOW";
            }
        }
        private void button4_Click_1(object sender, EventArgs e)
        {
            obrazky = Directory.GetFiles("C:\\Users\\kaktu\\Desktop\\Azk");
            obrazky = chaosPole(obrazky);
            pictureBox3.Image = imToTxt(new Bitmap(obrazky[0]));
            string[] chaosPole(string[] pole)
            {
                string[] chaos = pole;
                Random random = new Random();
                for (int i = chaos.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);

                    string temp = chaos[i];
                    chaos[i] = chaos[j];
                    chaos[j] = temp;
                }
                return chaos;
            }
        }
        // ze stack overflow + upravy
        private Size oldSize;

        private void Form1_Load(object sender, EventArgs e)
        => oldSize = base.Size;
        private void ResizeAll(Control control, Size newSize)
        {
            int width = newSize.Width - oldSize.Width;
            control.Left += (control.Left * width) / oldSize.Width;
            control.Width += (control.Width * width) / oldSize.Width;

            int height = newSize.Height - oldSize.Height;
            control.Top += (control.Top * height) / oldSize.Height;
            control.Height += (control.Height * height) / oldSize.Height;
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            resize(panel2.Controls);
            resize(tabPage2.Controls);
            oldSize = base.Size;
        }
        void resize(Control.ControlCollection nnn)
        {
            ResizeAll(nnn.Owner, base.Size);
            foreach (Control cnt in nnn)
            {
                ResizeAll(cnt, base.Size);
            }
        }
    }
}
