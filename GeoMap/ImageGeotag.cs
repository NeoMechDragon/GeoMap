using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;


namespace GeoMap
{
    class ImageGeotag
    {
        public void LoadImage(String path) {

            try
            {
                FileStream Foto = File.Open(path, FileMode.Open, FileAccess.Read); // открыли файл по адресу s для чтения
                BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                BitmapMetadata TmpImgEXIF = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные
                ulong[] t = { rational(50), rational(30), rational(12.345) };
       


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

        private ulong rational(double a) //
        {
            uint denom = 1000;
            uint num = (uint)(a * denom);
            ulong tmp;
            tmp = (ulong)denom << 32;
            tmp |= (ulong)num;
            return tmp;
        }

        public String GetDataFromImage(String path)
        {
            try
            {
                String result = "";
                FileStream Foto1 = File.Open(path, FileMode.Open, FileAccess.Read); // открыли файл по адресу s для чтения
                BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto1, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                BitmapMetadata TmpImgEXIF2 = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные            
                result = (String)TmpImgEXIF2.GetQuery("/app1/ifd/gps/{ushort=2}");
                return result;
            }

            catch (Exception ex)
            {
                return "Error";
            }
        }

       public static double ToDegrees(ulong[] coord)
        {
            return coord[0] + coord[1] / 60.0 + coord[2] / (60.0 * 60.0);
        }

    }

    

        
    }



