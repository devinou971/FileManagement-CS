using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32;

using MessageBox = System.Windows.MessageBox;

namespace FileManagement
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String folderPath; 
        private FolderBrowserDialog folderDialog; 
        private Dictionary<String, List<String>> types; 

        public MainWindow()
        {
            InitializeComponent(); 

            PathArea.DataContext = this; 
            folderDialog = new FolderBrowserDialog();

            this.FolderPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();


            this.PathArea.DataContext = this;


            ChargerTypes();
        }

        public string FolderPath
        {
            get
            {
                return this.folderPath;
            }

            set
            {
                this.folderPath = value;
            }
        }

        public Dictionary<string, List<string>> Types
        {
            get
            {
                return this.types;
            }

            set
            {
                this.types = value;
            }
        }

        private void ChoosePath(object sender, RoutedEventArgs e)
        {
            folderDialog.ShowDialog();
            this.FolderPath = folderDialog.SelectedPath;
            this.PathTextBox.Text = this.FolderPath;
        }

        public void ChargerTypes()
        {
            try
            {
                this.Types = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("./types.json"));
            }
            catch(Exception e)
            {
                MessageBox.Show("Lecture du fichier impossible, l'application va se fermer : \n" + e.Message);
                Environment.Exit(0);
            }
        }

        private void SortFiles(object sender, RoutedEventArgs e)
        {
            
            List<String> files = new List<string> ();

            try
            {
                files = Directory.GetFiles(this.FolderPath).ToList<String>();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Impossible d'obtenir de lire le dossier : " + this.FolderPath + "\n" + ex.Message );
                Environment.Exit(1);
            }
            int nbFiles = files.Count;
            int nbFilesMoved = 0;

            foreach (String file in files)
            {
                string ext = file.Split('.').Last().ToLower();
                foreach(String typeFichier in this.Types.Keys)
                {
                    if (this.Types[typeFichier].Contains(ext))
                    {
                        String pathToGo = this.FolderPath + @"\" + typeFichier + @"\" + ext + @"\" ;
                        if(!Directory.Exists(pathToGo))
                        {
                            Directory.CreateDirectory(pathToGo);
                        }

                        if (!File.Exists(pathToGo + file.Split(char.Parse(@"\")).Last()))
                        {
                            File.Move(file, pathToGo + file.Split(char.Parse(@"\")).Last());
                        }
                        else
                        {
                            string nomFichier = file.Split('.')[0].Split(char.Parse(@"\")).Last();
                            List<String> doubles = Directory.GetFiles(pathToGo).ToList<String>();
                            doubles = doubles.FindAll(x => x.Split(char.Parse(@"\")).Last().Contains(nomFichier));
                            int nbDoubles = doubles.Count;
                            string nouveauNom = nomFichier + "_" + nbDoubles + "." + ext;
                            nouveauNom = nouveauNom.Split( char.Parse(@"\")).Last();
                            File.Move(file, pathToGo + nouveauNom);
                        }
                    }
                }
                nbFilesMoved++;
                Progression.Value = ((double)nbFilesMoved / (double)nbFiles) * 100;
            }
            MessageBox.Show("Finished !");
        }
    }
}
