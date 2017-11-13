using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanMarketAPI
{
    /// <summary>
    /// Обработчик загрузки страниц
    /// </summary>
    public class PageLoader
    {
        /// <summary>
        /// Максимальное количество попыток загрузки страницы.
        /// Множественные попытки необходимы в связи с тем, что сайт источник
        /// работает отвратительно и регулярно падает в ошибки.
        /// </summary>
        public const int MAX_ATTEMPTS = 10;
    }
}
