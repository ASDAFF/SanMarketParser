using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Net;

namespace BitrixCSV
{
    /// <summary>
    /// Класс для обработки ресурсов изображений
    /// </summary>
    public class ImageResource
    {
        /// <summary>
        /// Адрес изображения на сайте поставщика
        /// </summary>
        public string VendorImageUrl
        {
            get { return _vendorImageUrl; }

            set
            {
                _vendorImageUrl = value;

                HashAlgorithm md5 = new MD5CryptoServiceProvider();
                byte[] res = md5.ComputeHash(Encoding.Default.GetBytes(value));
                _hash = BitConverter.ToString(res).Replace("-", string.Empty).ToLower();
            }
        }
        private string _vendorImageUrl;

        /// <summary>
        /// Контрольная сумма ссылки на файл поставщика
        /// </summary>
        private string _hash;

        /// <summary>
        /// Идентификатор товара, которому принадлежит изображение
        /// </summary>
        public int InventId { get; set; }

        /// <summary>
        /// Путь к локальному ресурсу в каталоге изображений
        /// </summary>
        public string LocalImageFileName
        {
            get
            {
                string res = "";

                if (InventId > -1 && VendorImageUrl.Length > 0 && _hash.Length > 0)
                {
                    // Получаем расширение из файла поставщика
                    Regex rg = new Regex(@"(?<ext>\w+)$");
                    Match extMatch = rg.Match(VendorImageUrl);
                    if (extMatch.Success)
                    {
                        res = extMatch.Groups["ext"].ToString();

                        // Формирование основной части имени
                        res = @"i_" + InventId.ToString() + @"-" + _hash + @"." + res;
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Каталог с изображениями
        /// </summary>
        public DirectoryInfo ResourcesFolder { get; set; }

        /// <summary>
        /// Локльный файл изображения
        /// </summary>
        public FileInfo LocalImageFile { get; set; }

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        public ImageResource()
        {
            InventId = -1;
            VendorImageUrl = "";
            _hash = "";
            ResourcesFolder = null;
            LocalImageFile = null;
        }

        /// <summary>
        /// Конструктор с заполнением параметров класса
        /// </summary>
        /// <param name="__inventId">ИД товара владельци изображения</param>
        /// <param name="__vendorUrl">Ссылка на изображение поставщика</param>
        /// <param name="__exportPath">Каталог с файлом экспорта</param>
        public ImageResource(int __inventId, string __vendorUrl, string __exportPath) : this()
        {
            if (__inventId > -1 && __vendorUrl.Length > 0 && __exportPath.Length > 0)
            {
                InventId = __inventId;
                VendorImageUrl = __vendorUrl;

                // Создание каталога ресурса
                ResourcesFolder = new DirectoryInfo(__exportPath + @"/invent_export");
                if (!ResourcesFolder.Exists)
                    ResourcesFolder.Create();

                // Иницализация информации о файле изображения
                LocalImageFile = new FileInfo(ResourcesFolder.FullName + @"\" + LocalImageFileName);
                if (!LocalImageFile.Exists)
                {
                    DownloadVendorFile();
                    LocalImageFile = new FileInfo(LocalImageFile.FullName);
                }
            }
        }

        /// <summary>
        /// Скачивает файл изображения с сайта поставщика
        /// </summary>
        public void DownloadVendorFile()
        {
            if (LocalImageFile != null && VendorImageUrl.Length > 0)
            {
                try
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(VendorImageUrl, LocalImageFile.FullName);
                } catch { }
            }
        }
        
    }
}
