using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanMarketAPI
{
    /// <summary>
    /// Базовый класс доступа к сайту
    /// </summary>
    public class Site
    {
        /// <summary>
        /// Адрес сайта
        /// </summary>
        public const string BASE_URL = @"http://www.sanmarket.ru";

        public static string PrepareUrl(string __url)
        {
            string pattern = @"^about\:\/\/\/";
            Regex regex = new Regex(pattern);

            string res = regex.Replace(__url, BASE_URL + @"/");

            return res;
        }
    }
}
