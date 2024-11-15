using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Tesseract;

namespace Projekt_Fang
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            oldSize = base.Size;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            comboBox1.Items.Add("eng");
            comboBox1.Items.Add("ces");
            comboBox1.Items.Add("lat");
            comboBox1.Text = comboBox1.Items[1].ToString();

            saveSettingsWS.SettingsInitilize(panel1.Controls);
            //imageClipboard();
            Thread SD = new Thread(KeyDet);
            SD.IsBackground = true;
            SD.Start();
        }

        void imageClipboard()
        {
            killThread = true;
            Clipboard.Clear();
            Thread BD = new Thread(prtsc);
            BD.IsBackground = true;
            BD.Start();
            void prtsc()
            {
                CheckForIllegalCrossThreadCalls = false;
                while (killThread)
                {
                    Thread thread = new Thread(ccc);
                    thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                    thread.Start();
                    thread.Join();
                    Thread.Sleep(1000);
                }


                void ccc()
                {
                    if (Clipboard.ContainsImage())
                    {
                        pictureBox1.Image = Clipboard.GetImage();
                        imageChanged(pictureBox2);
                        Clipboard.Clear();
                    }
                }
            }
        }

        void imageChanged(PictureBox pictureBox)
        {
            if (!Clipboard.ContainsImage()) { return; }

            //if (pictureBox.SizeMode != PictureBoxSizeMode.Zoom) { pictureBox.SizeMode = PictureBoxSizeMode.Zoom; }
            Bitmap scImage = (Bitmap)Clipboard.GetImage();
            pictureBox.Image = imToTxt((Bitmap)scImage);
            scImage.Dispose();
        }
        Bitmap imToTxt(Bitmap bmp)
        {
            Bitmap obraz = (Bitmap)bmp.Clone();
            using (TesseractEngine engine = new TesseractEngine(@"./tessdata", comboBox1.Text, EngineMode.Default))
            using (Pix img = PixConverter.ToPix(obraz))
            using (Page page = engine.Process(img))
            {
                ResultIterator iter = page.GetIterator();
                iter.Begin();

                do
                {
                    if (iter.IsAtBeginningOf(PageIteratorLevel.Word))
                    {
                        Rect bounds;
                        if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out bounds))
                        {
                            //Console.WriteLine($"Word: {iter.GetText(PageIteratorLevel.Word)}");
                            //Console.WriteLine($"BoundingBox: X={bounds.X1}, Y={bounds.Y1}, Width={bounds.Width}, Height={bounds.Height}");

                            if (!iter.GetText(PageIteratorLevel.Word).Any(char.IsLetter))
                            {
                                continue;
                            }
                            using (Graphics g = Graphics.FromImage(obraz))
                            {
                                using (Brush whiteBrush = new SolidBrush(Color.White))
                                {
                                    g.FillRectangle(whiteBrush, bounds.X1, bounds.Y1, bounds.Width, bounds.Height);
                                }
                            }

                        }
                    }
                } while (iter.Next(PageIteratorLevel.Word));
            }

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



           /* string zapKQ = $"{cestaKIm}_";
            foreach (string s in deleni)
            {
                zapKQ += $"{s}_";
            }
            zapKQ = zapKQ.Substring(0, zapKQ.Length - 1);
            StreamWriter hugin = new StreamWriter(sQpathFolder + "\\qdtb.txt", true);
            hugin.WriteLine(zapKQ);
            hugin.Close();*/


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
        // json
        //string jsonString = JsonSerializer.Serialize(data);
        /*
         string jsonString = JsonSerializer.Serialize(data);
         
         
         */
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
        bool kill = false;
        Keys kNext = Keys.N;
        Keys kPrev = Keys.J;
        Keys kShowHide = Keys.M;
        void KeyDet()
        {
            while (kill == false)
            {
                if (GetAsyncKeyState(kNext) < 0)
                {
                    button5_Click(button5, null); 
                    Thread.Sleep(100);
                }
                else if (GetAsyncKeyState(kShowHide) < 0)
                {
                    button5_Click(button6, null);
                    Thread.Sleep(100);
                }
                else if (GetAsyncKeyState(kPrev) < 0)
                {
                    button5_Click(button3, null);
                    Thread.Sleep(100);
                }


                    Thread.Sleep(70);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> deleni = new List<string>();
            deleni.Add("01Kosti");
            SaveQ((Bitmap)pictureBox1.Image, deleni);
        }

        string[] obrazky = { };
        bool showHide = false;
        int kolikaty = 0;
        bool killThread = false;
        private void button5_Click(object sender, EventArgs e)
        {
            switch (Convert.ToInt32(new string((sender as Button).Name.Where(char.IsDigit).ToArray())))
            {
                case 5:

                    if(kolikaty != obrazky.Count()-1) { kolikaty++;}
                    else { kolikaty = 0; }

                    posun();

                    break;
                case 3:

                    if (kolikaty != 0) { kolikaty--; }
                    else { kolikaty = obrazky.Count() - 1; }

                    posun();

                    break;
                case 6:
                    showHide = !showHide;

                    if (showHide)
                    {
                        pictureBox3.Image = new Bitmap(obrazky[kolikaty]);
                        (sender as Button).Text = "Hide";
                    }
                    else
                    {
                        pictureBox3.Image = imToTxt(new Bitmap(obrazky[kolikaty]));
                        (sender as Button).Text = "Show";
                    }
                    break;

                case 4:
                    obrazky = Directory.GetFiles(textBox2.Text);
                    obrazky = chaosPole(obrazky);
                    pictureBox3.Image = imToTxt(new Bitmap(obrazky[0]));
                    label1.Text = $"{kolikaty + 1} : {obrazky.Count()}";
                    break;
                case 7:
                    if (killThread) { killThread = false; (sender as Button).Text = "Start"; }
                    else { imageClipboard(); (sender as Button).Text = "Stop"; }
                    break;
            }
            
            void posun()
            {
                if(pictureBox3 != null) { pictureBox3.Image.Dispose(); pictureBox3.Image = null; }
                Bitmap x = new Bitmap(obrazky[kolikaty]);
                pictureBox3.Image = imToTxt(x); 
                label1.Text = $"{kolikaty + 1} : {obrazky.Count()}";
                button6.Text = "Show";
                showHide = false;
            }
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
        bool c = false;
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized && c)
            {
                resize(panel2.Controls);
                resize(tabPage2.Controls);
                oldSize = base.Size;
            }
            c = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = checkBox1.Checked;
        }
    }
}
