using System;
using System.Text;
using System.Security.Cryptography;

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
        public string Url {
            get { return _url; }
            set
            {
                _url = value.Trim();

                if (_url.Length > 0)
                {
                    // Установка ХЭШа
                    HashAlgorithm md5 = new MD5CryptoServiceProvider();
                    byte[] res = md5.ComputeHash(Encoding.Default.GetBytes(_url));
                    _hash = BitConverter.ToString(res).Replace("-", string.Empty);
                }
                else
                    _hash = "";
            }
        }
        protected string _url;

        protected string _hash;
        public string Hash { get { return _hash; } }

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
