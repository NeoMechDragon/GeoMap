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
        ImageGeotag geotag = new ImageGeotag();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string filename = @"map.html";
            string dirpath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;
            String strMapPath = Path.Combine(dirpath,filename);
            Browser.ScriptErrorsSuppressed = true;
            Browser.Navigate(new Uri(strMapPath));
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
            JObject rss = JObject.Parse(File.ReadAllText(@"D:\Documents\Visual Studio 2015\Projects\GeoMap\GeoMap\document.json"));
            JObject channel = (JObject)rss["photolist"];
        //    channel["title"] = ((string)channel["title"]).ToUpper();
        //    channel["description"] = ((string)channel["description"]).ToUpper();
            JArray photopath = (JArray)channel["path"];
            photopath.Add("Item 1");
            photopath.Add("Item 2");
            File.WriteAllText(@"D:\Documents\Visual Studio 2015\Projects\GeoMap\GeoMap\document.json", rss.ToString());
        }

        private void Data_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            FileDialog fbd = new OpenFileDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.FileName;
                Data.Text = geotag.GetDataFromImage(path);
            }
            else
            { Data.Text = fbd.FileName; }
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

    }
}
