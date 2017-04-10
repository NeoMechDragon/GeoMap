GeoMap - программа для отображения ваших фотографий с геометками на Google карте.
Как пользоваться программой:
Чтобы выбрать папку, из которой нужно нанести изображения на карте, нажмите "Выбрать папку", если вы не хотите загружать изображения из подпапок, то уберите галочку с "Добавить подпапки".
Кнопка "Очистить данные" удаляет фотографии с карты.
Кнопка "Обновить карту" обновляет карту, если обновление по какой-то причине не произошло само.
При нажатии правой кнопкой мыши по карте фиксируются координаты того место, куда вы кликнули, это нужно для нанесения геоданных на фотографии.
При нажатии на кнопку "Выбрать фото" вам предложат выбрать фото, на которое нужно нанести геоданные. После нажатиия на кнопку "Вставить геотеги" вы можете выбрать, записать геоданные в исходный файл или создать новое фото с геоданными(которые были зафиксированы при нажатии правой кнопкой мыши по карте). У нового фото будет имя как у старого фото + "_GeoTag".
При нажатии на фотографию на карте есть возможность удалить фото из карты или изменить координаты.

Разбор кода.

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
// Здесь происходит инциализация втроенного браузера и переход на страницу map.html:
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
        //Сохранение координат при нажатии правой кнопкой мыши по карте:
            geckoWebBrowser.AddMessageEventListener("externAlert", s =>
            {
                Gecko.GeckoHtmlElement lat,lng;
                lat = geckoWebBrowser.Document.GetHtmlElementById("coord1");
                lng = geckoWebBrowser.Document.GetHtmlElementById("coord2");
                Dlat.Text = lat.TextContent;
                Dlng.Text = lng.TextContent;
                
            });
            //Удаление изображения с карты:
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
            //Изменени координат фотографии на карте:
            geckoWebBrowser.AddMessageEventListener("updateAlert", s =>
            {
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
        //Обработка указанной для загрузки папки:
        public void LoadImages(string path, string oldpath, bool sub)

        {
            var dirInfo = new DirectoryInfo(path);
            foreach (var fileInfo in dirInfo.GetFiles("*.jpg"))
            {
                geotag.GetDataFromImage(path + "\\" + fileInfo.ToString(), oldpath);
            }
            if (sub == true) // если нужно загружать подпапки
                foreach (var fileInfo in dirInfo.GetDirectories())
                {
                    LoadImages(path + "\\" + fileInfo.ToString(), oldpath, sub);
                }
        }
        //Кнопка "Выбрать папку"
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

        //Кнопка "Обновить карту"
        private void button3_Click(object sender, EventArgs e)
        {
            string filename = @"map.html";
            geckoWebBrowser.Navigate(Path.Combine(folderpath, filename));
        }
        //Кнопка "Очистить данные", удаляем все созданные маленькие копии фотографий и очищаем json файл.
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
        
        //Кнопка "Выбрать фото":
        private void button6_Click(object sender, EventArgs e)
        {
            FileDialog fbd = new OpenFileDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Data2.Text = fbd.FileName;             
            }
        }
        //Кнопка "Вставить геотеги":
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

namespace GeoMap
{
    class ImageGeotag
    {
    //Функция нанесения геотегов на изображение:
        public void LoadImage(string path, double lat, double lng, bool usl)
        {
            using (var Foto1 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
            {
                Image original = Image.FromStream(Foto1);
                const short ExifTypeByte = 1;
                const short ExifTypeAscii = 2;
                const short ExifTypeRational = 5;

                const int ExifTagGPSVersionID = 0x0000;
                const int ExifTagGPSLatitudeRef = 0x0001;
                const int ExifTagGPSLatitude = 0x0002;
                const int ExifTagGPSLongitudeRef = 0x0003;
                const int ExifTagGPSLongitude = 0x0004;

                char latHemisphere = 'N';
                if (lat < 0)
                {
                    latHemisphere = 'S';
                    lat = -lat;
                }
                char lngHemisphere = 'E';
                if (lng < 0)
                {
                    lngHemisphere = 'W';
                    lng = -lng;
                }

                MemoryStream ms = new MemoryStream();
                original.Save(ms, ImageFormat.Jpeg);
                ms.Seek(0, SeekOrigin.Begin);

                Image img = Image.FromStream(ms);
                AddProperty(img, ExifTagGPSVersionID, ExifTypeByte, new byte[] { 2, 3, 0, 0 });
                AddProperty(img, ExifTagGPSLatitudeRef, ExifTypeAscii, new byte[] { (byte)latHemisphere, 0 });
                AddProperty(img, ExifTagGPSLatitude, ExifTypeRational, ConvertToRationalTriplet(lat));
                AddProperty(img, ExifTagGPSLongitudeRef, ExifTypeAscii, new byte[] { (byte)lngHemisphere, 0 });
                AddProperty(img, ExifTagGPSLongitude, ExifTypeRational, ConvertToRationalTriplet(lng));
                Foto1.Close();
                if (usl == true)
                {
                    string message = "Записать геоданные в указанный файл? (В случае отказа будет создан новый файл, который будет иметь имя исходного файла + '_GeoTag')";
                    string caption = "Запись геоданных";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult result;
                    result = MessageBox.Show(message, caption, buttons);

                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        img.Save(path);
                    }
                    else
                        img.Save(path.Substring(0, path.Length - 4) + "_GeoTag.jpg");
                }
                else
                    img.Save(path);
            }
        }
         static void AddProperty(Image img, int id, short type, byte[] value)
        {
            PropertyItem pi = img.PropertyItems[0];
            pi.Id = id;
            pi.Type = type;
            pi.Len = value.Length;
            pi.Value = value;
            img.SetPropertyItem(pi);
        }
        //Функция перевода из ulong в rational:
        static byte[] ConvertToRationalTriplet(double value)
        {
            int degrees = (int)Math.Floor(value);
            value = (value - degrees) * 60;
            int minutes = (int)Math.Floor(value);
            value = (value - minutes) * 60 * 100;
            int seconds = (int)Math.Round(value);
            byte[] bytes = new byte[3 * 2 * 4]; // Degrees, minutes, and seconds, each with a numerator and a denominator, each composed of 4 bytes
            int i = 0;
            Array.Copy(BitConverter.GetBytes(degrees), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(1), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(minutes), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(1), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(seconds), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(100), 0, bytes, i, 4);
            return bytes;
        }
        //Функция уменьшения изображения:
        public Image ResizeImg(Image b, int nWidth, int nHeight)
        {
            Image result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
                g.Dispose();
            }
            return result;
        }
        //Функция перевода из rational в ulong:
        private double obr(ulong a)
        {
            while (a < 1000000000000)
                a *= 10;
            ulong tmp = (ulong)1000 << 32;
            ulong num = a ^ tmp;
            double b = (double)num / 1000;
            return b;

        }
        //Функция чтения геотегов из фото:
        public void GetDataFromImage(String path, string oldpath)
        {
            string folderpath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;
            string filename = "document.json";
            JObject rss = JObject.Parse(File.ReadAllText(Path.Combine(folderpath, filename)));
            JObject channel = (JObject)rss["photolist"];
            JArray or = (JArray)channel["photoor"];
            for (int i = 0; i < or.Count; i++)
                if (or[i].ToString() == ("file:\\" + path))
                    return;
            try
            {
                using (var Foto1 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
                {
                    BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto1, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                    BitmapMetadata TmpImgEXIF2 = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные   
                    ulong[] a = ((ulong[])(TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=2}")));
                    ulong[] b = ((ulong[])(TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=4}")));
                    double aa = obr(a[0]) + obr(a[1]) / 60 + obr(a[2]) / 3600;
                    double bb = obr(b[0]) + obr(b[1]) / 60 + obr(b[2]) / 3600;
                    string path2 = Path.Combine(folderpath, "photosm");
                    Image im = Image.FromStream(Foto1);
                    im = ResizeImg(im, 60, 60 * im.Height / im.Width);
                    path2 = Path.Combine(path2, GetRightPartOfPath(path, Path.GetFileName(oldpath)));
                    Directory.CreateDirectory(path2);
                    Bitmap bitmap = new Bitmap(im);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawRectangle(new Pen(Brushes.Yellow, 3), new Rectangle(0, 0, bitmap.Width-1, bitmap.Height-1));
                    }
                    string fname = Path.GetFileName(path);
                    fname = fname.Substring(0, fname.Length - 4) + "_small.jpg";
                    bitmap.Save(path2 + "\\" + fname);
                    JArray sm = (JArray)channel["photosm"];
                    JArray tag1 = (JArray)channel["geotag1"];
                    JArray tag2 = (JArray)channel["geotag2"];
                    JArray h = (JArray)channel["height"];
                    JArray w = (JArray)channel["width"];
                    or.Add("file:\\" + path);
                    sm.Add("file:\\" + path2 + "\\" + fname);
                    tag1.Add(aa);
                    tag2.Add(bb);
                    h.Add(650 * im.Height / im.Width);
                    w.Add(650);
                    File.WriteAllText((Path.Combine(folderpath, filename)), rss.ToString());
                }
            }

            catch (Exception ex)
            {
            }
        }
        //Функция вычисления абсолютного пути нового файла (отнимает из одного пути другой путь):
        private static string GetRightPartOfPath(string path, string startAfterPart)
        {
            // use the correct seperator for the environment
            var pathParts = path.Split(Path.DirectorySeparatorChar);

            // this assumes a case sensitive check. If you don't want this, you may want to loop through the pathParts looking
            // for your "startAfterPath" with a StringComparison.OrdinalIgnoreCase check instead
            int startAfter = Array.IndexOf(pathParts, startAfterPart);

            if (startAfter == -1)
            {
                // path path not found
                return null;
            }

            // try and work out if last part was a directory - if not, drop the last part as we don't want the filename
            var lastPartWasDirectory = pathParts[pathParts.Length - 1].EndsWith(Path.DirectorySeparatorChar.ToString());
            return string.Join(
                Path.DirectorySeparatorChar.ToString(),
                pathParts, startAfter,
                pathParts.Length - startAfter - (lastPartWasDirectory ? 0 : 1));
        }

    }
}

map.html

<!DOCTYPE html>
<html>
  <head>
      <script src="https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/markerclusterer.js"></script>
      <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.6.2/jquery.min.js" type="text/javascript"></script>
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no">
    <meta charset="utf-8">
    <title>Simple markers</title>
    <style>
      html, body {
        height: 100%;
        margin: 0;
        padding: 0;
      }
      #map {
        height: 100%;
      }
    </style>
  </head>
  <body>
      <div id='coord1' style="display:none;">0</div> 
      <div id='coord2' style="display:none;">0</div> 
      <div id='curphoto' style="display:none;">-1</div> 
    <div id="map"></div>
      <script>

       var json = (function () {
       var json = null;
       $.ajax({   // открываем json файл
          'async': false,
           'global': false,
           'url': 'document.json',
           'dataType': "json",
            'success': function (data) {
                json = data;
               }
            });
              return json;
       })();
       function fireEvent() { // функция фиксации координат
           var event = new MessageEvent('externAlert', { 'view': window, 'bubbles': true, 'cancelable': false, 'data': 'data' });
           document.dispatchEvent(event);
       }
function initMap() { // загрузка карты
    var myLatLng = { lat: 50.363, lng: 50.044 };
  var map = new google.maps.Map(document.getElementById('map'), {
    zoom: 4,
    center: myLatLng
  });
  map.addListener('rightclick', function (event) {
      displayCoordinates(event.latLng);
  });
  function displayCoordinates(pnt) { // фиксация координат на правую кнопку мыши
      var lat = pnt.lat();
      lat = lat.toFixed(4);
      var lng = pnt.lng();
      lng = lng.toFixed(4);
      document.getElementById("coord1").innerHTML = lat;
      document.getElementById("coord2").innerHTML = lng;
      fireEvent()

  }
  var beachMarker = [];  // отрисовка маркеров
  for (i = 0; i < json.photolist.photoor.length; i++) {
      var image = json.photolist.photosm[i];
      beachMarker[i] = new google.maps.Marker({
          position: { lat: json.photolist.geotag1[i], lng: json.photolist.geotag2[i] },
          map: map,
          icon: image 
      });
      var name = i;
      attachSecretMessage(beachMarker[i], '<div id="content">' + '<img src="' + json.photolist.photoor[i] + '" height="' + json.photolist.height[i] + '" width="' + json.photolist.width[i] + '" >' + '</div>' + "<button onclick=del(\"" + name + "\");>Удалить</button> &nbsp;&nbsp;&nbsp;" + "<button onclick=upd(\"" + name + "\");>Изменить координаты</button>");
  }
 // var mcOptions = { gridSize: 50, maxZoom: 15, imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m' };
  var mcOptions = { maxZoom: 15, imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m' };
  var markerCluster = new MarkerClusterer(map, beachMarker, mcOptions);
}
function attachSecretMessage(marker, secretMessage) { // отрисовка окошек с ориганалми фотографий
    var infowindow = new google.maps.InfoWindow({
        content: secretMessage
    });
    marker.addListener('click', function () { // слушатель на нажатие по маркерам
        infowindow.open(marker.get('map'), marker);
    });
}
function del(i) { // удаление маркера
    document.getElementById("curphoto").innerHTML = i;
    var event = new MessageEvent('deleteAlert', { 'view': window, 'bubbles': true, 'cancelable': false, 'data': 'data' });
    document.dispatchEvent(event);
    alert("Фотография удалена с карты.")
    location.reload();
}
function upd(i) { // изменение координат маркера
    document.getElementById("curphoto").innerHTML = i;
    var event = new MessageEvent('updateAlert', { 'view': window, 'bubbles': true, 'cancelable': false, 'data': 'data' });
    document.dispatchEvent(event);
    alert("Координаты фотографии изменены.")
    location.reload();
}


      </script>
    <script src="https://maps.googleapis.com/maps/api/js?key=AIzaSyCWjPK1-v1cIjJK18qcZKwTH94pPtGveZ8&callback=initMap"
        async defer></script>
  </body>
</html>
