using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BitrixCSV
{
    /// <summary>
    /// Класс для формирования файла обмена товарами с сайтом
    /// под управлением CMS 1C-Bitrix
    /// </summary>
    public class FileCSV
    {
        /// <summary>
        /// Имя файла обмена данными
        /// </summary>
        public const string FILE_NAME = @"invent_export.csv";

        /// <summary>
        /// Каталог с файлом и ресурсами обмена данными
        /// </summary>
        public DirectoryInfo FilePath { get; set; }

        /// <summary>
        /// Данные файла обмена товарами
        /// </summary>
        public FileInfo FileInstance { get; set; }

        /// <summary>
        /// Глобальный экземпляр доступа к файлу обмена данными
        /// </summary>
        public static FileCSV INSTANCE
        {
            get
            {
                if (_instance == null)
                    _instance = new FileCSV();
                return _instance;
            }
        }
        protected static FileCSV _instance = null;

        /// <summary>
        /// Инициализация файла обмена данными
        /// </summary>
        /// <param name="__filePath">Путь к каталогу с файлами обмена и ресурсами</param>
        /// <returns>Признак успешной инициализации файла</returns>
        public bool InitFile(string __filePath)
        {
            bool res = false;

            try
            {
                FilePath = new DirectoryInfo(__filePath);

                // Проверка существования каталога
                if (!FilePath.Exists)
                    FilePath.Create();

                // Инициализация самого файла
                FileInstance = new FileInfo(FilePath.FullName + @"\" + FILE_NAME);
                if (FileInstance.Exists)
                    FileInstance.Delete();

                res = true;

            } catch { }

            return res;
        }

        
    }
}
