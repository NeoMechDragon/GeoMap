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
            geckoWebBrowser = new Gecko.GeckoWebBrowser { };
            geckoWebBrowser.Parent = this.panel1;
            geckoWebBrowser.Dock = DockStyle.Fill;
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));

        }


        private void showMessage(string s)
        {
            MessageBox.Show(s);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            geckoWebBrowser.AddMessageEventListener("externAlert", s =>
            {
                Gecko.GeckoHtmlElement lat,lng;
                lat = geckoWebBrowser.Document.GetHtmlElementById("coord1");
                lng = geckoWebBrowser.Document.GetHtmlElementById("coord2");
                Dlat.Text = lat.TextContent;
                Dlng.Text = lng.TextContent;
                
            });
        }

        public void LoadImages(string path, string oldpath)

        {
            var dirInfo = new DirectoryInfo(path);
            foreach (var fileInfo in dirInfo.GetFiles("*.jpg"))
            {
                geotag.GetDataFromImage(path + "\\" + fileInfo.ToString(), oldpath);
            }
            foreach (var fileInfo in dirInfo.GetDirectories())
            {
                LoadImages(path + "\\" + fileInfo.ToString(), oldpath);
            }
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
                LoadImages(fbd.SelectedPath, fbd.SelectedPath);
            }
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Data.Text = "";
            DirectoryInfo dirInfo = new DirectoryInfo(folderpath + "//photosm");
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (var folder in dirInfo.GetDirectories())
            {
                Directory.Delete(folderpath + "//photosm//" + folder.ToString(),true);
            }
            JObject rss = new JObject(
            new JProperty("photolist",
            new JObject(
            new JProperty("photoor", new JArray()),
            new JProperty("photosm", new JArray()),
            new JProperty("geotag1", new JArray()),
            new JProperty("geotag2" ,new JArray()),
            new JProperty("height", new JArray()),
            new JProperty("width", new JArray())
                        )
                         )          );
            File.WriteAllText((Path.Combine(folderpath, "document.json")), rss.ToString());
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Gecko.GeckoHtmlElement ele;
            ele = geckoWebBrowser.Document.GetHtmlElementById("coord");
            Data.Text = ele.TextContent;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FileDialog fbd = new OpenFileDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Data2.Text = fbd.FileName;             
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string slat = Dlat.Text;
            string slng = Dlng.Text;
            slat = slat.Replace(".", ",");
            slng = slng.Replace(".", ",");
            double lat = Convert.ToDouble(slat);
            double lng = Convert.ToDouble(slng);
            geotag.LoadImage(Data2.Text, lat, lng);
        }
    }
}
