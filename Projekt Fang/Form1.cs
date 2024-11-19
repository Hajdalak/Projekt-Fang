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
using System.Text.Json;
using System.Text.Json.Serialization;

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
            getAllF(textBox2.Text);
        }
        void getAllF(string VetsiFolderPath)
        {
           string[] slozky = Directory.GetDirectories(VetsiFolderPath);
            foreach (string s in slozky) 
            { 
                comboBox3.Items.Add(s.Replace(VetsiFolderPath,""));
                comboBox2.Items.Add(s.Replace(VetsiFolderPath, ""));
                comboBox2.Text = comboBox2.Items[0].ToString();
            }
        }
        public class Otazka
        {
            public int Spatne { get; set; }
            public int Spravne { get; set; }
            public int Celkem { get; set; }
            public string Subtrida { get; set; }
            public string Path { get; set; }
            [JsonConstructor]
            public Otazka(string Path, string Subtrida, int Celkem, int Spatne, int Spravne)

            {
                this.Spatne = Spatne;
                this.Spravne = Spravne;
                this.Celkem = Celkem;
                this.Path = Path;
                this.Subtrida = Subtrida;
            }
            public Otazka(string Path, string Subtrida)

            {
                this.Spatne = 0;
                this.Spravne = 0;
                this.Celkem = 0;
                this.Path = Path;
                this.Subtrida = Subtrida;
            }
        }
        List<Otazka> Otazky = new List<Otazka>();
        string ScanObr(string folderPath)
        {
            string[] temp  = Directory.GetFiles(folderPath, "*.jpg");
            string x = folderPath;
            x = x.Substring(x.LastIndexOf("\\") , x.Length - x.LastIndexOf("\\")  );
            Otazka tmp = null;
            List<Otazka> ul = new List<Otazka>();

            foreach (string radek in temp)
            {
                tmp = Otazky.FirstOrDefault(otazka => otazka.Path == radek);

                if (tmp == null)
                {
                    ul.Add(new Otazka(radek, x));
                }
                else { ul.Add(tmp); Console.WriteLine("pridano"); }
            }
            string cesta = $"{folderPath}{x}.json";
            Console.WriteLine("cesta: "+ cesta);
            //if (File.Exists(cesta)) { File.Delete(cesta);  }
            StreamWriter mugin = new StreamWriter(cesta);
            mugin.WriteLine(JsonSerializer.Serialize(ul));
            mugin.Close();
            return cesta;
        }
        void LoadObr(string folderPath,bool pridat)
        {
            string 
            n = Directory.GetFiles(folderPath, "*.json").FirstOrDefault();
            Console.WriteLine("NNNN: " + n);
            if(n == null) { n = ScanObr(folderPath); }

            StreamReader mugin = new StreamReader(n);
            string precteno = mugin.ReadToEnd();
            mugin.Close();

            if (pridat) { Otazky.AddRange(JsonSerializer.Deserialize<List<Otazka>>(precteno)); }
            else { Otazky = JsonSerializer.Deserialize<List<Otazka>>(precteno); }
            
        }
        private void button10_Click(object sender, EventArgs e)
        {
            ScanObr(textBox2.Text);
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


        int imR = 0;

        void SaveQ(Bitmap bmp , string path)
        {
            if (bmp == null) { return; }
            string sQpathFolder = path;
            // fotka, deleni1, deleni2 atd
            if (imR == 0) { imR = foundNMIm(); }
            string cestaKIm = $"{imR:0000}.jpg";
            imR++;
            bmp.Save(sQpathFolder + "\\" + cestaKIm);

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

        Otazka[] obrazky = { };
        bool showHide = false;
        int kolikaty = 0;
        bool killThread = false;
        bool start = true;
        private void button5_Click(object sender, EventArgs e)
        {
            switch (Convert.ToInt32(new string((sender as Button).Name.Where(char.IsDigit).ToArray())))
            {
                case 1: SaveQ((Bitmap)pictureBox1.Image, textBox2.Text + comboBox3.Text); break;
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
                        pictureBox3.Image = new Bitmap(obrazky[kolikaty].Path);
                        (sender as Button).Text = "Hide";
                    }
                    else
                    {
                        pictureBox3.Image = imToTxt(new Bitmap(obrazky[kolikaty].Path));
                        (sender as Button).Text = "Show";
                    }
                    break;

                case 4:
                    if (start)
                    {
                        Console.WriteLine(textBox2.Text  + comboBox2.Text);
                        LoadObr((textBox2.Text +comboBox2.Text), false);
                        obrazky = Otazky.ToArray();
                        if (obrazky.Count() == 0) { }
                        obrazky = chaosPole(obrazky);
                        pictureBox3.Image = imToTxt(new Bitmap(obrazky[0].Path));
                        label1.Text = $"{kolikaty + 1} : {obrazky.Count()}";
                        start = false;
                        kill = false;
                        Thread SD = new Thread(KeyDet);
                        SD.IsBackground = true;
                        SD.Start();
                        (sender as Button).Text = "End";
                        button11.Text = $"Wrong: {obrazky[kolikaty].Spatne}";
                        button12.Text = $"Right: {obrazky[kolikaty].Spravne}";
                    }
                    else
                    {
                        Otazky.Clear();
                        Otazky.AddRange( obrazky.ToList().OrderBy(x => x.Path));
                        ScanObr(textBox2.Text + comboBox2.Text);
                        start = true;
                        kolikaty = 0;
                        pictureBox3.Image.Dispose();
                        pictureBox3.Image = null;
                        kill = true;
                        (sender as Button).Text = "Start";
                        button11.Text = $"Wrong: ";
                        button12.Text = $"Right: ";
                        label1.Text = "0 : 0";
                    }
                    
                    break;
                case 7:
                    if (killThread) { killThread = false; (sender as Button).Text = "Start"; }
                    else { imageClipboard(); (sender as Button).Text = "Stop"; }
                    break;
                case 11:
                    obrazky[kolikaty].Spatne += 1;
                    (sender as Button).Text = $"Wrong: {obrazky[kolikaty].Spatne}";
                    break;
                case 12:
                    obrazky[kolikaty].Spravne += 1;
                    (sender as Button).Text = $"Right: {obrazky[kolikaty].Spravne}";
                    break;
            }
            
            void posun()
            {
                if(pictureBox3 != null) { pictureBox3.Image.Dispose(); pictureBox3.Image = null; }
                Bitmap x = new Bitmap(obrazky[kolikaty].Path);
                pictureBox3.Image = imToTxt(x); 
                label1.Text = $"{kolikaty + 1} : {obrazky.Count()}";
                button11.Text = $"Wrong: {obrazky[kolikaty].Spatne}";
                button12.Text = $"Right: {obrazky[kolikaty].Spravne}";
                button6.Text = "Show";
                showHide = false;
            }
            Otazka[] chaosPole(Otazka[] pole)
            {
                Otazka[] chaos = pole;
                Random random = new Random();
                for (int i = chaos.Count() - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);

                    Otazka temp = chaos[i];
                    chaos[i] = chaos[j];
                    chaos[j] = temp;
                }
                chaos = chaos.OrderBy(x => (x.Spravne - x.Spatne)).ToArray();
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
                resize(panel1.Controls);
                resize(tabPage1.Controls);
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
