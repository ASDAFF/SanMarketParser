using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitrixCSV;
using System.Data.SqlClient;
using System.Threading;

namespace SanMarketAPI
{
    /// <summary>
    /// Класс управления экспортом товаров в файл CSV для 1С-Битрикс
    /// </summary>
    public class InventExport : WorkerBase
    {
        public static readonly object _locker = new object();

        /// <summary>
        /// Вызывается при любом виде завершения процесса экспорта товаров
        /// </summary>
        public event Action<InventExport> OnExportCompleted;

        /// <summary>
        /// Признак активности процесса парсинга
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Признак активности процесса парсинга
        /// </summary>
        public bool IsActive { get { return _isActive; } }

        /// <summary>
        /// Файл экспорта данных
        /// </summary>
        public FileCSV ExportFile { get; set; }

        /// <summary>
        /// Глобальный экземпляр текущего класса
        /// </summary>
        public static InventExport INSTANCE
        {
            get
            {
                if (_instance == null)
                    _instance = new InventExport();
                return _instance;
            }
        }
        protected static InventExport _instance = null;

        /// <summary>
        /// Базовый конструктор класса
        /// </summary>
        public InventExport()
        {
            ExportFile = null;
            _isActive = false;
        }

        /// <summary>
        /// Выполнение процедуры экспорта данных
        /// </summary>
        /// <param name="__filePath">Каталог с файлами обмена данными</param>
        public async void ExportData(string __filePath)
        {
            // Защита от повторного запуска
            if (!IsActive)
            {
                _isActive = true;
                ExportFile = new FileCSV();
                if (ExportFile.InitFile(__filePath))
                    Log = string.Format(@"Инициализация файла успешно: {0}", ExportFile.FileInstance.FullName);
                else
                    Log = string.Format(@"Ошибка инициализации файла в каталоге: {0}", __filePath);

                Log = @"Начало экспорта товаров";

                await Task.Run(() => StartExport());

                ExportFile.CloseFile();

                _isActive = false;

                OnExportCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Выполнение процедуры экспорта данных
        /// </summary>
        private void StartExport()
        {

            SqlConnection cnt = Site.GetDatabaseConnection();

            if (cnt != null && cnt.State == System.Data.ConnectionState.Open)
            {
                dsParserTableAdapters.tInventItemsTableAdapter taItems = new dsParserTableAdapters.tInventItemsTableAdapter();
                int itemsCount;
                try { itemsCount = (int)taItems.ItemsCount(); }
                catch { itemsCount = 0; }

                // Работаем с товарами через SqlDataReader, поскольку в выборке очень много записей
                string query = @"SELECT * FROM [dbo].[tInventItems]";
                SqlCommand cndInvent = new SqlCommand(query, cnt);
                SqlDataReader rdrInvent = cndInvent.ExecuteReader();

                if (rdrInvent.HasRows)
                {
                    FileRow exportRow;
                    int exportedItems = 0;
                    while (rdrInvent.Read() && IsActive)
                    {
                        List<ImageResource> imgList = GetImagesList((int)rdrInvent["Id"]);

                        exportRow = new FileRow();
                        exportRow.XmlId = (int)rdrInvent["Id"];
                        exportRow.Name = rdrInvent["Name"].ToString();
                        exportRow.PreviewText = rdrInvent["ShortDescr"].ToString();
                        exportRow.DetailText = rdrInvent["Descr"].ToString();
                        exportRow.ArtNumber = @"SMR-" + rdrInvent["Id"].ToString().Trim();
                        exportRow.Price = (decimal)rdrInvent["Price"];

                        FillGroups(ref exportRow, (int)rdrInvent["ParentId"]);

                        // Выгрузка с учётом изображений
                        if (imgList.Count > 0)
                            foreach (ImageResource img in imgList)
                                if (img.LocalImageFile != null && img.LocalImageFile.Exists)
                                {
                                    //string remoteFileName = @"invent_export/" + img.LocalImageFile.Name;
                                    string remoteFileName = img.LocalImageFile.Name;
                                    exportRow.MorePhoto = remoteFileName;
                                    ExportFile.FileWriter.WriteLine(exportRow.ToString());
                                }
                        else
                            ExportFile.FileWriter.WriteLine(exportRow.ToString());

                        // Формирование сообщения пользователю
                        exportedItems++;
                        if (exportedItems % 100 == 0)
                        {
                            string logMessage = @"Экспортировано {0} из {1}";
                            Log = string.Format(logMessage, exportedItems, itemsCount);
                        }
                        
                    }
                }

                rdrInvent.Close();
                cnt.Close();
            }

        }

        /// <summary>
        /// Формирование списка изображений
        /// </summary>
        /// <param name="__id"></param>
        /// <returns></returns>
        private List<ImageResource> GetImagesList(int __id)
        {
            List<ImageResource> imgList = new List<ImageResource>();

            // Извлекаем данные из БД
            dsParserTableAdapters.tInventImagesTableAdapter ta = new dsParserTableAdapters.tInventImagesTableAdapter();
            dsParser.tInventImagesDataTable tbl = ta.GetDataByInventItemId(__id);

            foreach (dsParser.tInventImagesRow row in tbl)
            {
                imgList.Add(new ImageResource(__id, row.Url, ExportFile.FilePath.FullName));
            }

            return imgList;
        }

        /// <summary>
        /// Заполнение значений полей групп родителей
        /// </summary>
        /// <param name="__row"></param>
        /// <param name="__parent"></param>
        private void FillGroups(ref FileRow __row, int __parent)
        {
            Group parentGroup = new Group(__parent);

            if (parentGroup.Id > -1)
            {
                List<string> parentNames = new List<string>();
                FillParentName(ref parentNames, parentGroup);
                parentNames.Reverse();

                // Установка значений полей
                if (parentNames.Count > 0)
                    __row.Group_0 = parentNames[0];
                if (parentNames.Count > 1)
                    __row.Group_1 = parentNames[1];
                if (parentNames.Count > 2)
                    __row.Group_2 = parentNames[2];
            }
        }

        /// <summary>
        /// Рекурсивное формирование групп согласно подчинённости
        /// </summary>
        /// <param name="__parentNames"></param>
        /// <param name="__parentGroup"></param>
        private void FillParentName(ref List<string> __parentNames, Group __parentGroup)
        {
            __parentNames.Add(__parentGroup.Name);
            if (__parentGroup.Parent != null && __parentGroup.Parent.Name.Trim() != string.Empty)
                FillParentName(ref __parentNames, __parentGroup.Parent);
        }

        /// <summary>
        /// Остановка процедуры парсинга товаров
        /// </summary>
        public void Stop()
        {
            if (IsActive)
            {
                _isActive = false;
                Log = @"Запрос на остановку экспорта товаров";
            }
        }

    }
}
