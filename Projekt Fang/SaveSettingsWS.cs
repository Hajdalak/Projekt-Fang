using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static string nameTxt01 = "Settings.txt";
        public void SettingsInitilize(Control.ControlCollection control)
        {
            if (!Directory.Exists(settingFilePath)) { Directory.CreateDirectory(settingFilePath); }
            if (!File.Exists(settingFilePath + "\\" + nameTxt01)) { File.Create(settingFilePath + "\\" + nameTxt01); }
            else { SettingsLoad(control);  }
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
