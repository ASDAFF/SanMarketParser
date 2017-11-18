using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitrixCSV;

namespace SanMarketAPI
{
    /// <summary>
    /// Класс управления экспортом товаров в файл CSV для 1С-Битрикс
    /// </summary>
    public class InventExport : WorkerBase
    {
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

                OnExportCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Выполнение процедуры экспорта данных
        /// </summary>
        private void StartExport()
        {
            
        }

    }
}
