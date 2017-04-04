using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.JScript;
using Newtonsoft.Json.Linq;
using System.Drawing.Drawing2D;

namespace GeoMap
{
    class ImageGeotag
    {
        public void LoadImage(String path) {

            try
            {

                FileStream Foto = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Inheritable); // открыли файл по адресу s для чтения
                BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                BitmapMetadata TmpImgEXIF = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные
                ulong[] t = { rational(50), rational(130), rational(12.345) };
                TmpImgEXIF.SetQuery("/app1/ifd/gps/{ushort=2}", t);
                ulong[] t2 = { rational(100), rational(130), rational(12.345) };
                TmpImgEXIF.SetQuery("/app1/ifd/gps/{ushort=4}", t2);

                JpegBitmapEncoder Encoder = new JpegBitmapEncoder();//создали новый энкодер для Jpeg
                Encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0], decoder.Frames[0].Thumbnail, TmpImgEXIF, decoder.Frames[0].ColorContexts)); //добавили в энкодер новый кадр(он там всего один) с указанными параметрами

                string NewFileName = path + "+GeoTag.jpg";//имя исходного файла +GeoTag.jpg
                using (Stream jpegStreamOut = File.Open(NewFileName, FileMode.CreateNew, FileAccess.ReadWrite))//создали новый файл
                    Encoder.Save(jpegStreamOut);//сохранили новый файл
                Foto.Close();//и закрыли исходный файл
            }
            catch (Exception ex)
            {

            }
        }
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
        private ulong rational(double a) //
        {
            uint denom = 1000;
            uint num = (uint)(a * denom);
            ulong tmp;
            tmp = (ulong)denom << 32;
            tmp |= (ulong)num;
            ulong tmp2;
            return tmp;

            
        }
        private double obr(ulong a)
        {
            while (a < 1000000000000)
                a *= 10;
            ulong tmp = (ulong)1000 << 32;
            ulong num = a ^ tmp;
            double b= (double)num / 1000;
            return b;

        }

        public void GetDataFromImage(String path)
        {
            try
            {
                using (var Foto1 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
                {
                    BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto1, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                    BitmapMetadata TmpImgEXIF2 = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные            
                                                                                                     //result = (String)TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=2}");
                    ulong[] a = ((ulong[])(TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=2}")));
                    ulong[] b = ((ulong[])(TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=4}")));
                    double aa = obr(a[0]) + obr(a[1]) / 60 + obr(a[2]) / 3600;
                    double bb = obr(b[0]) + obr(b[1]) / 60 + obr(b[2]) / 3600;
                    string folderpath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;
                    string path2 = Path.Combine(folderpath, "photosm");
                    Image im = Image.FromStream(Foto1);

                    im = ResizeImg(im, 60, 60*im.Height/im.Width);
                    im.Save(path2 + "\\" + Path.GetFileName(path) + "small.jpg");
                    string filename = "document.json";
                    JObject rss = JObject.Parse(File.ReadAllText(Path.Combine(folderpath, filename)));  // Считываем json файл в объект rss
                    JObject channel = (JObject)rss["photolist"];
                    JArray or = (JArray)channel["photoor"];
                    JArray sm = (JArray)channel["photosm"];
                    JArray tag1 = (JArray)channel["geotag1"];
                    JArray tag2 = (JArray)channel["geotag2"];
                    JArray h = (JArray)channel["height"];
                    JArray w = (JArray)channel["width"];
                    or.Add("file:\\" + path);
                    sm.Add("file:\\" + path2 + "\\" + Path.GetFileName(path) + "small.jpg");
                    tag1.Add(aa);
                    tag2.Add(bb);
                    h.Add(650*im.Height / im.Width);
                    w.Add(650);
                    File.WriteAllText((Path.Combine(folderpath, filename)), rss.ToString());
                }
            }

            catch (Exception ex)
            {
            }
        }

       public static double ToDegrees(ulong[] coord)
        {
            return coord[0] + coord[1] / 60.0 + coord[2] / (60.0 * 60.0);
        }

    }

    

        
    }



