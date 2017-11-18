using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

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

        /// <summary>
        /// Обработка ссылки к глобальному представлению
        /// </summary>
        /// <param name="__url"></param>
        /// <returns>Глобальный текст ссылки</returns>
        public static string PrepareUrl(string __url)
        {
            string pattern = @"^about\:\/\/\/";
            Regex regex = new Regex(pattern);

            string res = regex.Replace(__url, BASE_URL + @"/");

            return res;
        }

        /// <summary>
        /// Создаёт от открывает новое соединение с БД
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetDatabaseConnection()
        {
            SqlConnection cnt = null;

            try
            {
                dsParserTableAdapters.tInventItemsTableAdapter ta = new dsParserTableAdapters.tInventItemsTableAdapter();
                string connectionString = ta.Connection.ConnectionString;
                cnt = new SqlConnection(connectionString);
                cnt.Open();
            }
            catch { cnt = null; }

            return cnt;
        }
    }
}
