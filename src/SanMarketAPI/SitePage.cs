using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanMarketAPI
{
    /// <summary>
    /// Класс страницы сайта
    /// </summary>
    public abstract class SitePage
    {
        /// <summary>
        /// Загрузчик страницы
        /// </summary>
        public PageLoader Loader { get; set; }

        /// <summary>
        /// Ссылка на страницу текущего объекта
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Базовый конструктор класса
        /// </summary>
        public SitePage()
        {
            Url = "";

            // Инициализация загрузчика станицы
            Loader = new PageLoader();
        }

    }
}
