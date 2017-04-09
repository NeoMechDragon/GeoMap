using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Drawing.Drawing2D;


namespace GeoMap
{
    class ImageGeotag
    {
        public void LoadImage(String path, double lat, double lng, bool usl)
        {

            try
            {
                double latDegree = Math.Floor(lat);
                double latMinute = Math.Floor(((lat - Math.Floor(lat)) * 60.0));
                double latSecond = (((lat - Math.Floor(lat)) * 60.0) - Math.Floor(((lat - Math.Floor(lat)) * 60.0))) * 60;
                double lngDegree = Math.Floor(lng);
                double lngMinute = Math.Floor(((lng - Math.Floor(lng)) * 60.0));
                double lngSecond = (((lng - Math.Floor(lng)) * 60.0) - Math.Floor(((lng - Math.Floor(lng)) * 60.0))) * 60;
                using (var Foto = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
                {
                    BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                    BitmapMetadata TmpImgEXIF = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные
                    ulong[] t = { rational(latDegree), rational(latMinute), rational(latSecond) };
                    TmpImgEXIF.SetQuery("/app1/ifd/gps/{ushort=2}", t);
                    ulong[] t2 = { rational(lngDegree), rational(lngMinute), rational(lngSecond) };
                    TmpImgEXIF.SetQuery("/app1/ifd/gps/{ushort=4}", t2);
                    JpegBitmapEncoder Encoder = new JpegBitmapEncoder();//создали новый энкодер для Jpeg
                    Encoder.QualityLevel = 100;
                    Encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0], decoder.Frames[0].Thumbnail, TmpImgEXIF, decoder.Frames[0].ColorContexts)); //добавили в энкодер новый кадр(он там всего один) с указанными параметрами
                    string NewFileName = path.Substring(0, path.Length - 4) + "_GeoTag.jpg";//имя исходного файла + _GeoTag.jpg
                    if (File.Exists(NewFileName))
                        File.Delete(NewFileName);
                    using (Stream jpegStreamOut = File.Open(NewFileName, FileMode.CreateNew, FileAccess.ReadWrite))//создали новый файл
                    {
                        Encoder.Save(jpegStreamOut);
                    }//сохранили новый файл 
                    Foto.Close();
                }
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
            double b = (double)num / 1000;
            return b;

        }

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
                                                                                                     //result = (String)TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=2}");
                    ulong[] a = ((ulong[])(TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=2}")));
                    ulong[] b = ((ulong[])(TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=4}")));
                    double aa = obr(a[0]) + obr(a[1]) / 60 + obr(a[2]) / 3600;
                    double bb = obr(b[0]) + obr(b[1]) / 60 + obr(b[2]) / 3600;
                    string path2 = Path.Combine(folderpath, "photosm");
                    Image im = Image.FromStream(Foto1);
                    im = ResizeImg(im, 60, 60 * im.Height / im.Width);
                    path2 = Path.Combine(path2, GetRightPartOfPath(path, Path.GetFileName(oldpath)));
                    Directory.CreateDirectory(path2);
               //     im.Save(path2 + "\\" + Path.GetFileName(path) + "small.jpg");
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


