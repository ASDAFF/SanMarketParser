using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace SanMarketParser
{
    /// <summary>
    /// Параметры парсера
    /// </summary>
    [DataContract]
    public class ParserSettings
    {
        /// <summary>
        /// Имя файла конфигурации
        /// </summary>
        public const string FILE_NAME = @"SanMarketParser.xml";

        /// <summary>
        /// Полный путь к конфигурационному файлу
        /// </summary>
        public static string CONFIG_PATH
        {
            get
            {
                string configPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                    @"\" + ParserSettings.FILE_NAME;
                return configPath;
            }
        }

        /// <summary>
        /// Путь к пакетам экспорта для 1С-Битрикс
        /// </summary>
        [DataMember]
        public string ExportPath { get; set; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public ParserSettings()
        {
            // Инициализация переменных
            ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                @"\SanMarketParser";
        }

        /// <summary>
        /// Загрузка данных из файла конфигруации
        /// </summary>
        /// <returns></returns>
        public static ParserSettings GetSettings()
        {
            ParserSettings settings = new ParserSettings();

            try
            {
                if (File.Exists(ParserSettings.CONFIG_PATH))
                {
                    var configSerializer = new DataContractSerializer(typeof(ParserSettings));
                    using (Stream s = File.OpenRead(ParserSettings.CONFIG_PATH))
                        settings = (ParserSettings)configSerializer.ReadObject(s);
                }
            } catch { }

            return settings;
        }

        /// <summary>
        /// Сохранение параметров в конфигурационном файле
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var configSerializer = new DataContractSerializer(typeof(ParserSettings));
                using (Stream s = File.Create(ParserSettings.CONFIG_PATH))
                    configSerializer.WriteObject(s, this);
            }
            catch { }
        }
    }
}
