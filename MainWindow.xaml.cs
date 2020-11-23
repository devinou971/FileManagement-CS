using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.ComponentModel;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32;

using MessageBox = System.Windows.MessageBox;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FileManagement
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String folderPath; // Cette variable sert à contenir le chemin vers le dossier que l'on veut trier
        private FolderBrowserDialog folderDialog; // Ceci est un dialogue pour récupérer le dossier que l'on souhaite trier
        private Dictionary<String, List<String>> types; // Cette variable récupère tout les types de fichier

        public MainWindow()
        {
            InitializeComponent(); // Initialise les composants

            PathArea.DataContext = this; // On défini le datacontext
            folderDialog = new FolderBrowserDialog();


            this.FolderPath = "";
            
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
            //MessageBox.Show(this.FolderPath);
            
        }

        public void ChargerTypes()
        {
            try
            {
                this.Types = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText("./types.json"));
            }
            catch(Exception e)
            {
                //MessageBox.Show("Lecture du fichier impossible, l'application va se fermer : \n" + e.Message);
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
            //MessageBox.Show(files.Count + "");

            foreach (String file in files)
            {
                string ext = file.Split('.').Last().ToLower();
                //MessageBox.Show("L'extention du fichier : " + file + " est " + ext );
                foreach(String typeFichier in this.Types.Keys)
                {
                    if (this.Types[typeFichier].Contains(ext))
                    {
                        String pathToGo = this.FolderPath + @"\" + typeFichier + @"\" + ext + @"\" ;
                        //MessageBox.Show("Le fichier " + file + " ira ici : " + pathToGo + file.Split(char.Parse(@"\")).Last());
                        if(!Directory.Exists(pathToGo))
                        {
                            Directory.CreateDirectory(pathToGo);
                            //MessageBox.Show("Create : " + pathToGo);
                        }

                        if (!File.Exists(pathToGo + file.Split(char.Parse(@"\")).Last()))
                        {
                            //MessageBox.Show("Fichier bougé !");
                            File.Move(file, pathToGo + file.Split(char.Parse(@"\")).Last());
                        }
                        else
                        {
                            string nomFichier = file.Split('.')[0].Split(char.Parse(@"\")).Last();
                            List<String> doubles = Directory.GetFiles(pathToGo).ToList<String>();
                            //MessageBox.Show(nomFichier + " " + doubles[0]);
                            doubles = doubles.FindAll(x => x.Split(char.Parse(@"\")).Last().Contains(nomFichier));
                            //MessageBox.Show("" + doubles.Count);
                            int nbDoubles = doubles.Count;
                            string nouveauNom = nomFichier + "_" + nbDoubles + "." + ext;
                            nouveauNom = nouveauNom.Split( char.Parse(@"\")).Last();
                            File.Move(file, pathToGo + nouveauNom);
                            //MessageBox.Show("Fichier renommé : " + nouveauNom +" bougé !");


                        }
                    }
                }
                nbFilesMoved++;
                Progression.Value = ((double)nbFilesMoved / (double)nbFiles) * 100;
            }
            MessageBox.Show("Fini !");
        }
    }
}
