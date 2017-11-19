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
        /// Экспортировать данные фрагментами
        /// </summary>
        public bool ExportFragments { get; set; }

        /// <summary>
        /// Размер фрагмента для выгрузки
        /// </summary>
        public int FragmentSize { get; set; }

        /// <summary>
        /// Граница выгрузки предыдущего фрагмента при фрагментной выгрузке.
        /// </summary>
        public int PreviousFragment { get; set; }

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
        /// <param name="__exportFragments">Фрагментный экспорт</param>
        /// <param name="__fragmentSize">Размер фрагмента</param>
        /// <param name="__previousFragment">Номер строки предыдущего фрагмента</param>
        public async void ExportData(string __filePath, bool __exportFragments, int __fragmentSize, int __previousFragment)
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

                // Подготовка параметров фрагментной выгрузки
                ExportFragments = __exportFragments;
                FragmentSize = __fragmentSize;
                PreviousFragment = __previousFragment;

                // Запуск процедуры экспорта
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
                    string logMessage = @"{0} {1} из {2}";
                    string logCaption;

                    while (rdrInvent.Read() && IsActive && (!ExportFragments || exportedItems < PreviousFragment + FragmentSize))
                    {
                        List<ImageResource> imgList = GetImagesList((int)rdrInvent["Id"]);

                        exportRow = new FileRow();
                        exportRow.XmlId = (int)rdrInvent["Id"];
                        exportRow.Name = rdrInvent["Name"].ToString().Replace(";", " ");
                        exportRow.PreviewText = rdrInvent["ShortDescr"].ToString().Replace(";", " ").Replace("\n", " ").Replace("\r", " ");
                        exportRow.DetailText = rdrInvent["Descr"].ToString().Replace(";", " ").Replace("\n", " ").Replace("\r", " ");
                        exportRow.ArtNumber = @"SMR-" + rdrInvent["Id"].ToString().Trim();
                        exportRow.Price = (decimal)rdrInvent["Price"];

                        FillOptions(ref exportRow);

                        FillGroups(ref exportRow, (int)rdrInvent["ParentId"]);

                        if (!ExportFragments || exportedItems >= PreviousFragment)
                        {
                            // Выгрузка с учётом изображений
                            if (imgList.Count > 0)
                            {
                                foreach (ImageResource img in imgList)
                                {
                                    if (img.LocalImageFile != null && img.LocalImageFile.Exists)
                                    {
                                        //string remoteFileName = @"invent_export/" + img.LocalImageFile.Name;
                                        string remoteFileName = img.LocalImageFile.Name;
                                        exportRow.MorePhoto = remoteFileName;
                                        ExportFile.FileWriter.WriteLine(exportRow.ToString());
                                    }
                                }
                            }
                            else
                                ExportFile.FileWriter.WriteLine(exportRow.ToString());

                            logCaption = @"Экспортировано";
                        }
                        else
                            logCaption = @"Пропущено";

                        // Формирование сообщения пользователю
                        exportedItems++;
                        if (exportedItems % 100 == 0)
                        {
                            Log = string.Format(logMessage, logCaption, exportedItems, itemsCount);
                        }
                        
                    }

                    // Фиксируем значение границы выгрузки
                    PreviousFragment = exportedItems;
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
        /// Заполнение списка характеристик товара
        /// </summary>
        /// <param name="__row"></param>
        private void FillOptions(ref FileRow __row)
        {
            dsParserTableAdapters.tInventOptionsTableAdapter ta = new dsParserTableAdapters.tInventOptionsTableAdapter();
            dsParser.tInventOptionsDataTable tbl = ta.GetDataByInventItemId(__row.XmlId);

            if (tbl.Rows.Count > 0)
            {
                // Блок характеристик добавляется только при условии наличия характеристик
                string srcCode = "";
                foreach (dsParser.tInventOptionsRow row in tbl)
                    if (row.Name.Trim() != string.Empty && row.Value.Trim() != string.Empty)
                        srcCode += row.Name.Trim().Replace(";", " ").Replace("\n", " ").Replace("\r", " ") + 
                            @": " + 
                            row.Value.Trim().Replace(";", " ").Replace("\n", " ").Replace("\r", " ") + 
                            @", ";

                if (srcCode.Length > 0)
                {
                    __row.DetailText += @" Характеристики: " + srcCode;
                }
                        
            }
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
