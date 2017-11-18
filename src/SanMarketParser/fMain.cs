using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SanMarketAPI;

namespace SanMarketParser
{
    public partial class fMain : Form
    {
        /// <summary>
        /// Текущие настройки
        /// </summary>
        public ParserSettings CurrentSettings { get; set; }

        /// <summary>
        /// Класс управления парсингом
        /// </summary>
        public SiteWorker Worker { get { return SiteWorker.CURRENT_INSTANCE; } }

        /// <summary>
        /// Класс управления экспортом данных
        /// </summary>
        public InventExport ExportWorker { get { return InventExport.INSTANCE; } }

        /// <summary>
        /// Конструктор формы
        /// </summary>
        public fMain()
        {
            InitializeComponent();

            // Инициализация журнала событий
            tbEvents.Text = "";
            Log(@"Инициализация");

            // Инициализация настроек
            CurrentSettings = ParserSettings.GetSettings();

            // Установка параметров из настроек
            tbExportPath.Text = CurrentSettings.ExportPath;

            // Подключение событий парсера
            Worker.OnParsingCompleted += Worker_OnParsingCompleted;
            Worker.OnLogChanged += Worker_OnLogChanged;

            // Подключение событий экспортера данных
            ExportWorker.OnExportCompleted += ExportWorker_OnExportCompleted;
            ExportWorker.OnLogChanged += ExportWorker_OnLogChanged;
        }

        /// <summary>
        /// Обработка события завершения экспорта товаров
        /// </summary>
        /// <param name="__sender"></param>
        private void ExportWorker_OnExportCompleted(InventExport __sender)
        {
            bImport.Enabled = true;
            bExport.Enabled = true;
            bAll.Enabled = true;
            bCancel.Enabled = false;

            Log(@"Экспорт товаров завершён");
        }

        /// <summary>
        /// Обработка изменений в журнале объекта управления экспортом товаров
        /// </summary>
        /// <param name="__sender"></param>
        /// <param name="__logMessage"></param>
        private void ExportWorker_OnLogChanged(WorkerBase __sender, string __logMessage)
        {
            Log(__logMessage);
        }

        /// <summary>
        /// Обработка изменений в журнале объекта управления парсингом
        /// </summary>
        /// <param name="__sender"></param>
        /// <param name="__logMessage"></param>
        private void Worker_OnLogChanged(WorkerBase __sender, string __logMessage)
        {
            Log(__logMessage);
        }

        /// <summary>
        /// Обработка события завершения парсинга
        /// </summary>
        /// <param name="obj"></param>
        private void Worker_OnParsingCompleted(SiteWorker obj)
        {
            bImport.Enabled = true;
            bExport.Enabled = true;
            bAll.Enabled = true;
            bCancel.Enabled = false;

            Log(@"Парсинг товаров завершён");
        }

        /// <summary>
        /// Обработка закрытия формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender != null && sender.GetType().Name == @"fMain")
            {
                fMain senderForm = (fMain)sender;

                // Обновление настроек в классе конфигурации
                senderForm.CurrentSettings.ExportPath = tbExportPath.Text;

                // Сохранение настроек
                senderForm.CurrentSettings.SaveSettings();
            }
        }

        /// <summary>
        /// Запуск процедуры парсинга с сайта источника
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bImport_Click(object sender, EventArgs e)
        {
            if (!Worker.IsActive)
            {
                bImport.Enabled = false;
                bExport.Enabled = false;
                bAll.Enabled = false;
                bCancel.Enabled = true;

                Worker.Start();
            }
        }

        /// <summary>
        /// Запись событий в журнал
        /// </summary>
        /// <param name="__messageText">Текст описания события</param>
        private void Log(string __messageText)
        {
            if (tbEvents.Text.Length > 8192)
                tbEvents.Text = "";

            tbEvents.Text = DateTime.Now.ToLocalTime() + @" - " +
                __messageText +
                Environment.NewLine +
                tbEvents.Text;
        }

        /// <summary>
        /// Обработка нажатия кнопки отмены
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bCancel_Click(object sender, EventArgs e)
        {
            if (Worker.IsActive)
                Worker.Stop();
        }

        /// <summary>
        /// Запуск процедуры экспорта товаров
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bExport_Click(object sender, EventArgs e)
        {
            if (!ExportWorker.IsActive)
            {
                bImport.Enabled = false;
                bExport.Enabled = false;
                bAll.Enabled = false;
                bCancel.Enabled = true;

                ExportWorker.ExportData(tbExportPath.Text);
            }
        }
    }
}
