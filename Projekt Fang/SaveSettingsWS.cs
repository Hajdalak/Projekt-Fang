using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


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

        // udelej default nacteni napr. settingFilePath a cesta epubu

        private string settingFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\lsekaiMan" + "\\" + Assembly.GetExecutingAssembly().GetName().Name;
        private static string nameTxt01 = "Settings.txt";
        private Size oldSize;
        List<Control> allCon = new List<Control>();
        public void SettingsInitilize(Control.ControlCollection control)
        {
            if (!Directory.Exists(settingFilePath)) { Directory.CreateDirectory(settingFilePath); }
            if (!File.Exists(settingFilePath + "\\" + nameTxt01)) { File.Create(settingFilePath + "\\" + nameTxt01); }
            else { SettingsLoad(control);  }
        }
        public void SizeInitilize(Control TopControl, Size FormSize)
        {
            allCon.Clear();
            allCon.AddRange(GetAllCon(TopControl));
            oldSize = FormSize;
        }
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
        public string SettingsLoad(Control.ControlCollection control)
        {

            StreamReader hugin = new StreamReader(settingFilePath + "\\" + nameTxt01);
            string vse = hugin.ReadToEnd();
            hugin.Close();
            string[] komponenty = vse.Split('\n');

            if (control.Owner.Name != komponenty[0]) { return ""; }

            for (int i = 1; i < (komponenty.Count() - 2); i++)
            {
                CntData cntData = new CntData(komponenty[i].Split('_')[0], komponenty[i].Split('_')[1], komponenty[i].Split('_')[2]);
                Console.WriteLine(cntData.typeOfControl + "_" + cntData.nameOfControl + "_" + cntData.dataFromControl);
                if (cntData.typeOfControl.Contains("CheckBox"))
                {
                    Control[] foundControls = control.Find(cntData.nameOfControl, true); // The second argument (true) searches recursively
                    (foundControls[0] as CheckBox).Checked = (cntData.dataFromControl == "True");
                }
                else if (cntData.typeOfControl.Contains("TextBox"))
                {
                    
                    Control[] foundControls = control.Find(cntData.nameOfControl, true);
                    (foundControls[0] as TextBox).Text = cntData.dataFromControl;
                }
            }
            return vse;
        }
        public void OpenFolder() { Process.Start(settingFilePath); }
        public string ChooseFolder(bool viaText_Dialog, string viText)
        {
            if (!viaText_Dialog) { settingFilePath = betterFolderSelection(); }
            else { settingFilePath = viText; }

            return settingFilePath + "\\" + nameTxt01;
        }
        public string pathToSettings() { return settingFilePath; /*+ "\\"+ nameTxt01; */}
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
