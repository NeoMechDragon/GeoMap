using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoMap
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            String strMapPath = @"D:\Documents\Visual Studio 2015\Projects\GeoMap\map.html";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(new Uri(strMapPath));
        }     
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Выберите папку";
            fbd.ShowNewFolderButton = false;
            if(fbd.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath+"\\";
            }
        }
    }
}
