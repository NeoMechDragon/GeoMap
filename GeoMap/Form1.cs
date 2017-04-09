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
            geckoWebBrowser.AddMessageEventListener("deleteAlert", s =>
            {
                Gecko.GeckoHtmlElement photo;
                photo = geckoWebBrowser.Document.GetHtmlElementById("curphoto");
                CurPhoto.Text = photo.TextContent;
                int i = Convert.ToInt16(CurPhoto.Text);
                string filename = "document.json";
                JObject rss = JObject.Parse(File.ReadAllText(Path.Combine(folderpath, filename)));  // Считываем json файл в объект rss
                JObject channel = (JObject)rss["photolist"];
                JArray or = (JArray)channel["photoor"];
                JArray sm = (JArray)channel["photosm"];
                JArray tag1 = (JArray)channel["geotag1"];
                JArray tag2 = (JArray)channel["geotag2"];
                JArray h = (JArray)channel["height"];
                JArray w = (JArray)channel["width"];
                string photopath = sm[i].ToString();
                photopath = photopath.Substring(6, photopath.Length-6);
                or[i].Remove();
                sm[i].Remove();
                tag1[i].Remove();
                tag2[i].Remove();
                h[i].Remove();
                w[i].Remove();
                File.WriteAllText((Path.Combine(folderpath, filename)), rss.ToString());
                File.Delete(photopath);
            });
            geckoWebBrowser.AddMessageEventListener("updateAlert", s =>
            {
                Gecko.GeckoHtmlElement photo;
                photo = geckoWebBrowser.Document.GetHtmlElementById("curphoto");
                CurPhoto.Text = photo.TextContent;
                int i = Convert.ToInt16(CurPhoto.Text);
                string slat = Dlat.Text;
                string slng = Dlng.Text;
                slat = slat.Replace(".", ",");
                slng = slng.Replace(".", ",");
                double lat = Convert.ToDouble(slat);
                double lng = Convert.ToDouble(slng);
                string filename = "document.json";
                JObject rss = JObject.Parse(File.ReadAllText(Path.Combine(folderpath, filename)));  // Считываем json файл в объект rss
                JObject channel = (JObject)rss["photolist"];
                JArray tag1 = (JArray)channel["geotag1"];
                JArray tag2 = (JArray)channel["geotag2"];
                tag1[i] = lat;
                tag2[i] = lng;
                File.WriteAllText((Path.Combine(folderpath, filename)), rss.ToString());
                string message = "Обновить геоданные в исходной фотографии? (В случае отказа координаты фотографии на карте изменятся, но геоданные в исходной фотографии не будут изменены)";
                string caption = "Изменение координат";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    JArray photoor = (JArray)channel["photoor"];
                    string temp = photoor[i].ToString();
                    temp = temp.Substring(6, temp.Length - 6);
                    geotag.LoadImage(temp, lat, lng, false);
                }
                else
                { }
            });
        }

        public void LoadImages(string path, string oldpath, bool sub)

        {
            var dirInfo = new DirectoryInfo(path);
            foreach (var fileInfo in dirInfo.GetFiles("*.jpg"))
            {
                geotag.GetDataFromImage(path + "\\" + fileInfo.ToString(), oldpath);
            }
            if (sub == true)
                foreach (var fileInfo in dirInfo.GetDirectories())
                {
                    LoadImages(path + "\\" + fileInfo.ToString(), oldpath, sub);
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Выберите папку";
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = folderpath + "\\photos";
            bool sub = false;
            if (checkSub.Checked)
                sub = true;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Data.Text = fbd.SelectedPath + "\\";
                LoadImages(fbd.SelectedPath, fbd.SelectedPath, sub);
            }
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
        }


        private void button3_Click(object sender, EventArgs e)
        {
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Data.Text = "";
            if (Directory.Exists(folderpath + "//photosm"))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderpath + "//photosm");
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (var folder in dirInfo.GetDirectories())
                {
                    Directory.Delete(folderpath + "//photosm//" + folder.ToString(), true);
                }
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
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
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
            geotag.LoadImage(Data2.Text, lat, lng, true);
        }
    }
}
