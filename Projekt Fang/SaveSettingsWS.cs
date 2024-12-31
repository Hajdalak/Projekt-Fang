using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
//using IWshRuntimeLibrary;


namespace Projekt_Fang
{
    internal class SaveSettingsWS
    {

        public class CntData
        {
            public string nameOfControl { get; set; }
            public string typeOfControl { get; set; }
            public string dataFromControl { get; set; }
            public CntData(string TypeOfControl, string NameOfControl, string DataFromControl)
            {
                this.nameOfControl = NameOfControl;
                this.typeOfControl = TypeOfControl;
                this.dataFromControl = DataFromControl;
            }
        }

        private string settingFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\lsekaiMan" + "\\" + Assembly.GetExecutingAssembly().GetName().Name;
        private static string nameTxt01 = "Settings.txt";
        private Size oldSize;
        List<Control> allCon = new List<Control>();
        // resi zda existuje slozka a soubor nastaveni
        public void SettingsInitilize(Control TopControlKolekce, Size FormSize)
        {
            if (!Directory.Exists(settingFilePath)) { Directory.CreateDirectory(settingFilePath); }
            if (!File.Exists(settingFilePath + "\\" + nameTxt01)) { File.Create(settingFilePath + "\\" + nameTxt01); }
            else { SizeInitilize(); SettingsLoad();  }

            // ziska vsechny polozky ui a ulozi je do listu a ulozi velikost formu
            void SizeInitilize()
            {
                allCon.Clear();
                allCon.AddRange(GetAllCon(TopControlKolekce));
                oldSize = FormSize;
            }
        }
        
        // ziska vsechny polozky ui //idk mozna lepsi zpusob
        public List<Control> GetAllCon(Control coll)
        {
            List<Control> list = new List<Control>();
            List<Control> list2 = new List<Control>();
            List<Control> list3 = new List<Control>();
            foreach (Control cnt in coll.Controls) { list.Add(cnt); list3.Add(cnt); }

            while (true)
            {
                foreach (Control www in list3)
                {
                    nnn(www);
                }
                if (list2.Count == 0) { break; }
                list3.Clear();
                list3.AddRange(list2);
                list2.Clear();
            }
            return list;

            void nnn(Control bbb)
            {
                foreach (Control cnt in bbb.Controls)
                {
                    list.Add(cnt);
                    list2.Add(cnt);
                }
            }
        }
        //prejmuto ze stack overflow skaluje velikost aplikace
        public void ResizeAll(Size newSize)
        {
            foreach (Control control in allCon)
            {
                int width = newSize.Width - oldSize.Width;
                control.Left += (control.Left * width) / oldSize.Width;
                control.Width += (control.Width * width) / oldSize.Width;

                int height = newSize.Height - oldSize.Height;
                control.Top += (control.Top * height) / oldSize.Height;
                control.Height += (control.Height * height) / oldSize.Height;
            }
            oldSize = newSize;
        }
        // ukladani nejakych hodnot ui do .txt napr. checkboxy true/false atd.
        public void SettingsSave(Control.ControlCollection control)
        {
            // veci oddeluje "\n" a jmena od hodnot a hodnoty od sebe "_"
            string CW = control.Owner.Name + "\n";
            foreach (Control cnt in control)
            {
                if (cnt is CheckBox chk)
                {
                    Console.WriteLine(cnt.Name);
                    CW += cnt.GetType() + "_" + cnt.Name + "_" + chk.Checked + "\n";
                }
                else if (cnt is TextBox tx)
                {
                    CW += cnt.GetType() + "_" + cnt.Name + "_" + tx.Text + "\n";
                }
            }
            CW += "\n";
            Console.WriteLine(CW);
            StreamWriter mugin = new StreamWriter(settingFilePath + "\\" + nameTxt01);
            mugin.Write(CW);
            mugin.Close();
        }
        // nacte hodnoty ui podle .txt ktery vygeneruje predchozi void
        public string SettingsLoad()
        {

            StreamReader hugin = new StreamReader(settingFilePath + "\\" + nameTxt01);
            string vse = hugin.ReadToEnd();
            hugin.Close();
            string[] komponenty = vse.Split('\n');

            for (int i = 1; i < (komponenty.Count() - 2); i++)
            {
                CntData cntData = new CntData(komponenty[i].Split('_')[0], komponenty[i].Split('_')[1], komponenty[i].Split('_')[2]);
                Control cntNal = allCon.FirstOrDefault(x => x.Name == cntData.nameOfControl);
                Console.WriteLine(cntNal.Name);
                Console.WriteLine(cntData.typeOfControl + "_" + cntData.nameOfControl + "_" + cntData.dataFromControl);
                if (cntData.typeOfControl.Contains("CheckBox"))
                {
                    (cntNal as CheckBox).Checked = (cntData.dataFromControl == "True");
                }
                else if (cntData.typeOfControl.Contains("TextBox"))
                {
                    (cntNal as TextBox).Text = cntData.dataFromControl;
                }
            }
            return vse;
        }
        // otevre slozku ulozeneho nastaveni
        public void OpenFolder() { Process.Start(settingFilePath); }
        // chatgpt nebo stackoverflow vytvori zastupce aplikace kteryho nakopiruje ho do slozky ve ktery
        // se spousti aplikace po zapnuti a nbeo ho tam smaze
        public void MakeAppStartWithPC(bool make_delete)
        {
            //using IWshRuntimeLibrary; // > Ref > COM > Windows Script Host Object  
            string link = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
                            + Path.DirectorySeparatorChar + Application.ProductName + ".lnk";

            bool start_ups = System.IO.File.Exists(link);

            if (make_delete && !start_ups)
            {
                CreateShortcut();

                void CreateShortcut()
                {

                    var shell = new IWshRuntimeLibrary.WshShell();
                    var shortcut = shell.CreateShortcut(link) as IWshRuntimeLibrary.IWshShortcut;
                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.WorkingDirectory = Application.StartupPath;
                    shortcut.Save();
                }
            }
            else if (!make_delete && start_ups) { System.IO.File.Delete(link); }
        }
        // vygeneroval chatgpt na lepsi vybyrani slozek
        public string betterFolderSelection()
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection";
            if (folderBrowser.ShowDialog() == DialogResult.OK) 
            { return System.IO.Path.GetDirectoryName(folderBrowser.FileName) + "\\"; }

            return "";
        }
    }
}
