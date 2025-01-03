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
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            comboBox1.Items.Add("eng");
            comboBox1.Items.Add("ces");
            comboBox1.Items.Add("lat");
            comboBox1.Text = comboBox1.Items[1].ToString();
            saveSettingsWS.SettingsInitilize(tabControl1, base.Size);
            //imageClipboard();
            getAllF(textBox2.Text);
            //panel4.Hide();
            panel5.Show();
            saveLoadKeybind(false);
            KeyName();
        }

        // .cs ktere pouzivam mezi projekty na caste veci
        SaveSettingsWS saveSettingsWS = new SaveSettingsWS();

        // udela z 2 bitmap 1
        Bitmap mergeBitmapy(Bitmap bmp1, Bitmap bmp2)
        {
            int delka = bmp1.Width;
            int vyska = bmp1.Height;
            bool VyskaNadDelka = false;

            if (bmp1.Height < bmp2.Height) { vyska = bmp2.Height; }
            if (bmp1.Width < bmp2.Width) { delka = bmp2.Width; }
            if (Math.Abs(bmp1.Height - bmp2.Height) > Math.Abs(bmp1.Width - bmp2.Width))
            { VyskaNadDelka = true; }

            Bitmap retbmp = null;// new Bitmap(delka, vyska);
            if (!VyskaNadDelka) { retbmp = new Bitmap(delka, bmp1.Height + bmp2.Height); }
            else { retbmp = new Bitmap(bmp1.Width + bmp2.Width, vyska); }
            if(checkBox7.Checked) {VyskaNadDelka = !VyskaNadDelka; }

            using (Graphics g = Graphics.FromImage(retbmp))
            {
                if (VyskaNadDelka)
                {
                    g.DrawImage(bmp1, 0, 0);
                    g.DrawImage(bmp2, bmp1.Width, 0);
                }
                else
                {
                    g.DrawImage(bmp1, 0, 0);
                    g.DrawImage(bmp2, 0, bmp1.Height);
                }
            }

            return retbmp;
        }
        //najde vsechny slozky a vypise je do comboboxu
        void getAllF(string VetsiFolderPath)
        {
            if (!Directory.Exists(VetsiFolderPath)) { return; }

            string[] slozky = Directory.GetDirectories(VetsiFolderPath);
            if (slozky.Count() == 0) {  return; }

            foreach (string s in slozky)
            {
                comboBox3.Items.Add(s.Replace(VetsiFolderPath, ""));
                comboBox2.Items.Add(s.Replace(VetsiFolderPath, ""));
                comboBox2.Text = comboBox2.Items[0].ToString();
                comboBox3.Text = comboBox3.Items[0].ToString();
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

        //vytvori json z otazek a souboru kde otazky z pole maji prednost pred souborami
        string ScanObr(string folderPath)
        {
            string[] temp = Directory.GetFiles(folderPath, "*.jpg");
            string x = folderPath;
            x = x.Substring(x.LastIndexOf("\\"), x.Length - x.LastIndexOf("\\"));
            Otazka tmp = null;
            List<Otazka> ul = new List<Otazka>();

            foreach (string radek in temp)
            {
                tmp = Otazky.FirstOrDefault(otazka => otazka.Path.Contains(Path.GetFileName(radek)));

                if (tmp == null)
                {
                    ul.Add(new Otazka(radek, x));
                }
                else 
                {
                    tmp.Path = radek;
                    ul.Add(tmp); 
                    //Console.WriteLine("pridano"); 
                }
            }
            string cesta = $"{folderPath}{x}.json";
            Console.WriteLine("cesta: " + cesta);
            //if (File.Exists(cesta)) { File.Delete(cesta);  }
            StreamWriter mugin = new StreamWriter(cesta);
            mugin.WriteLine(JsonSerializer.Serialize(ul));
            mugin.Close();
            return cesta;
        }
        //najde jsony dle pozadavku a nacte z nich otazky
        void LoadObr(string folderPath, bool pridat)
        {
            string n = Directory.GetFiles(folderPath, "*.json").FirstOrDefault();
            if (n == null) { n = ScanObr(folderPath); }

            StreamReader mugin = new StreamReader(n);
            string precteno = mugin.ReadToEnd();
            mugin.Close();
            List<Otazka> tmp = JsonSerializer.Deserialize<List<Otazka>>(precteno);

            if (!tmp[0].Path.Contains(folderPath)) { tmp = ChangePath(folderPath); }

            if (pridat) { Otazky.AddRange(tmp); }
            else { Otazky = tmp; }
        }
        //kdyz neni stejna cesta jsonu a obrazku opravy ji na cestu souboru
        List<Otazka> ChangePath(string folderPath)
        {
            List<Otazka> tmp = new List<Otazka>();
            string n = Directory.GetFiles(folderPath, "*.json").FirstOrDefault();
            StreamReader mugin = new StreamReader(n);
            string precteno = mugin.ReadToEnd();
            mugin.Close();

            tmp = JsonSerializer.Deserialize<List<Otazka>>(precteno);
            foreach(Otazka zm in tmp)
            {
                zm.Path = folderPath + $"\\{Path.GetFileName(zm.Path)}";
            }
            StreamWriter hugin = new StreamWriter(n);
            hugin.WriteLine(JsonSerializer.Serialize(tmp));
            hugin.Close();

            return tmp;
        }
        
        Bitmap x1 = null;
        Bitmap x2 = null;
        bool drz = true;
        // zapina a vypina periodicke cteni schranky
        bool killThread = true; //false pada? otestovat
        void imageClipboard()
        {
            killThread = true;
            drz = !drz;
            Clipboard.Clear();
            Thread BD = new Thread(prtsc);
            BD.IsBackground = true;
            BD.Start();
            if (drz) { killThread = false; button7.Text = "StartThread"; }
            else { button7.Text = "Stop"; }
            // kouka na jinych threadach zda je ve schrance obrazek
            void prtsc()
            {
                CheckForIllegalCrossThreadCalls = false;
                while (killThread)
                {
                    Thread thread = new Thread(ccc);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    Thread.Sleep(1000);
                }
                x1 = x2 = null;
                // podle podminek vybira co se bude delat s bitmapou 
                void ccc()
                {
                    if (Clipboard.ContainsImage())
                    {
                        if (checkBox2.Checked && !checkBox3.Checked) { x2 = null; x1 = new Bitmap(Clipboard.GetImage()); }
                        else if (checkBox3.Checked && checkBox2.Checked) { x2 = new Bitmap(Clipboard.GetImage()); }
                        else if (checkBox3.Checked && !checkBox2.Checked) { x1 = new Bitmap(Clipboard.GetImage()); }
                        else { x1 = new Bitmap(Clipboard.GetImage()); }


                        if (x1 != null && x2 != null)
                        {
                            pictureBox1.Image = mergeBitmapy(x1, x2);
                            pictureBox2.Image = imToTxt(mergeBitmapy(x1, x2));
                        }
                        else
                        {
                            pictureBox1.Image = x1;
                            pictureBox2.Image = imToTxt(x1);
                        }
                        Clipboard.Clear();
                    }
                }
            }
        }
        //chatgpt + upravy
        //vyuzuva knihovnu tesseract ktera umi OCR pouzivam ji at mi da souradnice textu kde pak na
        // bitmapu namaluju bily obdelnik na souradnice textu diky tomu "vytvorim karticku"
        //celou praci s knihovnou vygeneroval chatgpt ja jen to predelal at mi to dava souradnice
        //namaluje bilej obdelnik a opravil memory leak
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
            return obraz;//page.GetText();
        }


        // ukladani bitmap a generovani jejich jmena
        int imR = 0;
        void SaveQ(Bitmap bmp, string path, bool prepsat = false)
        {
            if (bmp == null) { return; }
            if(tempPathProZmenu != "") { bmp.Save(tempPathProZmenu); tempPathProZmenu = ""; }
            string sQpathFolder = path;
            string cestaKIm = "";
            if (!prepsat) 
            {
                if (imR == 0) { imR = foundNMIm() + 1; }
                cestaKIm = $"{imR:0000}.jpg";
                imR++;
                sQpathFolder += "\\" + cestaKIm;
            }
            else { File.Delete(sQpathFolder); }
            
            bmp.Save(sQpathFolder);

            int foundNMIm()
            {
                List<string> jpgFiles = Directory.GetFiles(sQpathFolder, "*.jpg")
                .Select(Path.GetFileNameWithoutExtension).ToList();
                jpgFiles.Sort();
                jpgFiles.Reverse();
                string n = jpgFiles.FirstOrDefault(x => int.TryParse(x, out int cislo) == true);

                if (n != null){ return Convert.ToInt32(n); }
                return 0;
            }

        }
        
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
        bool kill = false;
        Keys kNext = Keys.N;
        Keys kPrev = Keys.J;
        Keys kShowHide = Keys.M;
        // snima keystrouky i bez fokusu na aplikaci
        void KeyDet()
        {
            while (kill == false)
            {
                Thread.Sleep(70);
                if (checkBox8.Checked && Form.ActiveForm != this) { Thread.Sleep(1000); continue; }

                if (GetAsyncKeyState(kNext) < 0)
                {
                    buttonKlik(button5, null);
                    Thread.Sleep(100);
                }
                else if (GetAsyncKeyState(kShowHide) < 0)
                {
                    buttonKlik(button6, null);
                    Thread.Sleep(100);
                }
                else if (GetAsyncKeyState(kPrev) < 0)
                {
                    buttonKlik(button3, null);
                    Thread.Sleep(100);
                }
                
            }
        }
        void KeyName()
        {
            button8.Text = $"Next: {Enum.GetName(typeof(Keys), kNext)}";
            button9.Text = $"Prev: {Enum.GetName(typeof(Keys), kPrev)}";
            button10.Text = $"Show: {Enum.GetName(typeof(Keys), kShowHide)}";
        }
        void KeyBind(object sender, EventArgs e)
        {
            Keys temp = Keys.None;
            while(temp == Keys.None)
            {
                temp = getKey();
            }

            switch (Convert.ToInt32(new string((sender as Button).Name.Where(char.IsDigit).ToArray())))
            {
                case 8:
                    kNext = temp;
                    break;

                case 9: 
                    kPrev = temp;
                    break;

                case 10: 
                    kShowHide = temp;
                    break;
            }
            KeyName();


            Keys getKey()
            {
                for (int key = 0; key < 256; key++)
                {
                    short keyState = GetAsyncKeyState((Keys)key);

                    if ((keyState & 0x8000) != 0 && (Keys)key != Keys.End && (Keys)key != Keys.None)
                    {
                        return (Keys)key;
                    }
                }
                return Keys.None;
            }
        }
        void saveLoadKeybind(bool saveLoad)
        {
            string tcesta = saveSettingsWS.settingFilePath + "\\KeyBinds.txt";

            if (saveLoad) 
            {
                StreamWriter hugin = new StreamWriter(tcesta);
                string c = 
                    $"{Enum.GetName(typeof(Keys), kShowHide)}_" +
                    $"{Enum.GetName(typeof(Keys), kPrev)}_" +
                    $"{Enum.GetName(typeof(Keys), kNext)}";

                hugin.WriteLine(c);
                hugin.Close();
            }
            else 
            {
                if (!File.Exists(tcesta)) { return; }

                StreamReader mugin = new StreamReader(tcesta);
                string soub = mugin.ReadToEnd().Replace("\n","");
                mugin.Close();

                string[] radky = soub.Split('_');
                kShowHide = (Keys)Enum.Parse(typeof(Keys), radky[0], true);
                kPrev = (Keys)Enum.Parse(typeof(Keys), radky[1], true);
                kNext = (Keys)Enum.Parse(typeof(Keys), radky[2], true);
            }
        }
        Otazka[] obrazky = { };
        bool showHide = false;
        int kolikaty = 0;
        
        bool start = true;
        bool ochranaPosunu = false;
        string tempPathProZmenu = "";
        // aby nebylo nekolik vojdu na butonclick tak jsem udelal jednu fce ktera podle jmena odesilatele to rozdeli
        private void buttonKlik(object sender, EventArgs e)
        {
            switch (Convert.ToInt32(new string((sender as Button).Name.Where(char.IsDigit).ToArray())))
            {
                case 1: SaveQ((Bitmap)pictureBox1.Image, textBox2.Text + comboBox3.Text); break;
                case 2:
                    textBox2.Text = saveSettingsWS.betterFolderSelection();
                    //saveSettingsWS.SettingsSave(panel3.Controls);
                    getAllF(textBox2.Text);
                    break;
                case 3:

                    if (kolikaty != 0) { kolikaty--; }
                    else { kolikaty = obrazky.Count() - 1; }

                    posun();

                    break;
                case 4:
                    startEnd();

                    break;
                case 5:

                    if (kolikaty != obrazky.Count() - 1) { kolikaty++; }
                    else { kolikaty = 0; }

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
                case 7:
                    imageClipboard();
                    break;
                case 11:
                    obrazky[kolikaty].Spatne += 1;
                    (sender as Button).Text = $"Wrong: {obrazky[kolikaty].Spatne}";
                    break;
                case 12:
                    obrazky[kolikaty].Spravne += 1;
                    (sender as Button).Text = $"Right: {obrazky[kolikaty].Spravne}";
                    break;
                case 13: saveSettingsWS.OpenFolder(); break;
                case 14:
                    tabControl1.SelectedTab = tabPage3;
                    killThread = true;
                    Thread.Sleep(70);
                    killThread = false;
                    imageClipboard();
                    tempPathProZmenu = obrazky[kolikaty].Path;
                    Clipboard.SetImage(new Bitmap(tempPathProZmenu));
                    //pictureBox1.Image = new Bitmap(obrazky[kolikaty].Path);
                    break;
                case 15:
                    saveSettingsWS.SettingsSave(panel3.Controls);
                    saveLoadKeybind(true);
                    break;
            }
            // resi jak upravit ui pri startu ci vypnuti daneho okruhu kde start resi cteni souboru
            // a generovani pole otazek dle podminek a end uklada zmenu otazek jako napr zmenu wrong nebo right
            void startEnd()
            {
                if(ochranaPosunu) { return; }
                if (start)
                {
                    Console.WriteLine(textBox2.Text + comboBox2.Text);
                    if (checkBox4.Checked)
                    {
                        foreach (string radek in Directory.GetDirectories(textBox2.Text))
                        {
                            LoadObr((radek), true);
                        }
                    }
                    else { LoadObr((textBox2.Text + comboBox2.Text), false); }

                    obrazky = Otazky.ToArray();
                    // idk proc to tu bylo if (obrazky.Count() == 0) { }
                    obrazky = chaosPole(obrazky, checkBox5.Checked, checkBox6.Checked);
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
                    panel4.Show();
                    panel5.Hide();

                }
                else
                {
                    Otazky.Clear();
                    if (checkBox4.Checked)
                    {
                        Otazky.AddRange(obrazky.ToList().OrderBy(x => x.Subtrida));
                        foreach (string radek in Directory.GetDirectories(textBox2.Text).Select(Path.GetFileName))
                        {
                            obrazky.ToList().Where(x => x.Subtrida == radek);
                            ScanObr(textBox2.Text + radek);
                        }
                    }
                    else
                    {
                        Otazky.AddRange(obrazky.ToList().OrderBy(x => x.Path));
                        ScanObr(textBox2.Text + comboBox2.Text);
                    }

                    start = true;
                    kolikaty = 0;
                    pictureBox3.Image.Dispose();
                    pictureBox3.Image = null;
                    kill = true;
                    (sender as Button).Text = "Start";
                    button11.Text = $"Wrong: ";
                    button12.Text = $"Right: ";
                    label1.Text = "0 : 0";
                    panel4.Hide();
                    panel5.Show();

                }
            }
            // resi zmenu ui pri zmene vybrane otazky
            void posun()
            {
                ochranaPosunu = true;
                if (pictureBox3 != null) { pictureBox3.Image.Dispose(); pictureBox3.Image = null; }
                using (Bitmap x = new Bitmap(obrazky[kolikaty].Path))
                    pictureBox3.Image = imToTxt(x);
                label1.Text = $"{kolikaty + 1} : {obrazky.Count()}";
                button11.Text = $"Wrong: {obrazky[kolikaty].Spatne}";
                button12.Text = $"Right: {obrazky[kolikaty].Spravne}";
                button6.Text = "Show";
                showHide = false;
                ochranaPosunu = false;
            }
            //nahodne prohazi pole a pak srovna dle podminek
            Otazka[] chaosPole(Otazka[] pole, bool odSpatDoDob, bool pouzePoc)
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
                if (odSpatDoDob) { chaos = chaos.OrderBy(x => (x.Spravne - x.Spatne)).ToArray(); }
                if (pouzePoc) { chaos = chaos.Take((int)numericUpDown1.Value).ToArray(); }
                return chaos;
            }

        }

        //resi maximalizaci aplikace
        private void Form1_ResizeEnd(object sender, EventArgs e) { saveSettingsWS.ResizeAll(base.Size); }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                saveSettingsWS.ResizeAll(base.Size);
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e) 
        { TopMost = (sender as CheckBox).Checked; }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            saveSettingsWS.MakeAppStartWithPC(checkBox9.Checked);
        }
    }
}
