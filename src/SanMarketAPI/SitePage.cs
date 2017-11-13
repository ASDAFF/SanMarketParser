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
        protected PageLoader _loader;

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
            _loader = new PageLoader();
        }

    }
}
