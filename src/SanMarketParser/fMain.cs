using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanMarketParser
{
    public partial class fMain : Form
    {
        /// <summary>
        /// Текущие настройки
        /// </summary>
        public ParserSettings CurrentSettings { get; set; }

        /// <summary>
        /// Конструктор формы
        /// </summary>
        public fMain()
        {
            InitializeComponent();

            // Инициализация настроек
            CurrentSettings = ParserSettings.GetSettings();

            // Установка параметров из настроек
            tbExportPath.Text = CurrentSettings.ExportPath;
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
    }
}
