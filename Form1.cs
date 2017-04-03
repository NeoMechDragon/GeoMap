using System;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using Geotagging;
using Newtonsoft.Json.Linq;

namespace GeoMap
{
    public partial class Form1 : Form
    {
        string folderpath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;  // ПУТЬ К ПАПКЕ С ПРОЕКТОМ
        ImageGeotag geotag = new ImageGeotag();
        Gecko.GeckoWebBrowser geckoWebBrowser;
        public Form1()
        {
            InitializeComponent();
            Gecko.Xpcom.EnableProfileMonitoring = false;
            Gecko.Xpcom.Initialize("Firefox");
            geckoWebBrowser = new Gecko.GeckoWebBrowser {  };
            geckoWebBrowser.Parent = this.panel1;
            geckoWebBrowser.Dock = DockStyle.Fill;
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = "";
            FileDialog fbd = new OpenFileDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.FileName;
                geotag.LoadImage(path);
                Data.Text = fbd.FileName;
            }
            else
            { Data.Text = fbd.FileName; }
        /*    string filename = "document.json";
            JObject rss = JObject.Parse(File.ReadAllText(Path.Combine(folderpath, filename)));  // Считываем json файл в объект rss
            JObject channel = (JObject)rss["photolist"];
            JArray or = (JArray)channel["photoor"];
            JArray sm = (JArray)channel["photosm"];
            JArray tag1 = (JArray)channel["geotag1"];
            JArray tag2 = (JArray)channel["geotag2"];
            or.Add("Эта херня");
            sm.Add("Реально");
            tag1.Add("Работает");
            tag2.Add("Проверь");
            File.WriteAllText((Path.Combine(folderpath, filename)), rss.ToString()); */
        }

        private void Data_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Выберите папку";
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = folderpath + "\\photos";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Data.Text = fbd.SelectedPath + "\\";

                var dirInfo = new DirectoryInfo(fbd.SelectedPath);
                foreach (var fileInfo in dirInfo.GetFiles("*.jpg"))
                {
                    Data.Text = (fbd.SelectedPath + "\\"+ fileInfo.ToString());
                    Data.Text = geotag.GetDataFromImage(fbd.SelectedPath + "\\" + fileInfo.ToString());
                }
            }

            /*   if (fbd.ShowDialog() == DialogResult.OK)
               {
                   path = fbd.FileName;
                   Data.Text = geotag.GetDataFromImage(path);
                   string filename = @"map.html";
                   geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
                   geckoWebBrowser.Reload();
                   geckoWebBrowser.Refresh();
                   geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
                   geckoWebBrowser.Reload();
                   geckoWebBrowser.Refresh();

               }
               else
               { Data.Text = fbd.FileName; } */
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

    }
}
