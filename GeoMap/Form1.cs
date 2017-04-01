using System;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;

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
            // TODO: данная строка кода позволяет загрузить данные в таблицу "sampleDatabaseDataSet.Table1". При необходимости она может быть перемещена или удалена.
            this.table1TableAdapter.Fill(this.sampleDatabaseDataSet.Table1);
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
            string conStr = Geotagging.Properties.Settings.Default.SampleDatabaseConnectionString;  // Строка соединения с бд
            string sql = "INSERT INTO Table1(text) values(2)";
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = new SqlCommand(sql, con);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void table1BindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {

        }

        private void bindingNavigatorMovePreviousItem_Click(object sender, EventArgs e)
        {

        }

        private void table1BindingNavigatorSaveItem_Click_1(object sender, EventArgs e)
        {
            this.Validate();
            this.table1BindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.sampleDatabaseDataSet);

        }
    }
}
