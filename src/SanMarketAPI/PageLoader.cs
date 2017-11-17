using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

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
        public const int MAX_ATTEMPTS = 25;

        /// <summary>
        /// Адрес, из которого должна быть выполнена загрузка
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Исходный код загруженной страницы
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Разборщик структуры документа
        /// </summary>
        public HtmlParser DomParser { get; set; }

        /// <summary>
        /// Признак удачной загрузки документа
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Объект документа
        /// </summary>
        public IHtmlDocument Document { get; set; }

        /// <summary>
        /// Сброс значений всех параметров
        /// </summary>
        private void ResetData()
        {
            Url = "";
            Source = "";
            Success = false;
            Document = null;
        }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        public PageLoader()
        {
            DomParser = new HtmlParser();
            ResetData();
        }

        /// <summary>
        /// Загрузка страницы в асинхронном режиме
        /// </summary>
        /// <param name="__url">Адрес страницы</param>
        /// <returns>Признак успешной загрузки документа</returns>
        public async Task<bool> LoadPageAsync(string __url)
        {
            int attemptsRemain = MAX_ATTEMPTS;
            int attemptsAmount;
            string logMessage;

            ResetData();

            Url = __url;

            // Подготовка к загрузке
            CookieContainer cookie = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = cookie,
                UseCookies = true,
                AllowAutoRedirect = false
            };

            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { MaxAge = TimeSpan.Zero };
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            HttpResponseMessage response;

            // Загрузка выполняется до успешного результата или окончания числа попыток
            while (attemptsRemain > 0 && !Success)
            {
                // Пауза при неудачной попытке
                if (attemptsRemain < MAX_ATTEMPTS)
                {
                    attemptsAmount = MAX_ATTEMPTS - attemptsRemain;
                    logMessage = @"Повторная загрузка страницы {0} - {1}/{2}: Error";
                    SiteWorker.CURRENT_INSTANCE.Log = String.Format(logMessage,
                        Url,
                        attemptsAmount.ToString(),
                        MAX_ATTEMPTS.ToString());
                    await Task.Delay(15000);
                }
                attemptsRemain--;

                // Загрузка
                try
                {
                    response = await client.GetAsync(Url);
                    if (response != null)
                    {
                        attemptsAmount = MAX_ATTEMPTS - attemptsRemain;
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                Source = await response.Content.ReadAsStringAsync();
                                Document = await DomParser.ParseAsync(Source);
                                Success = true;

                                logMessage = @"Загрузка страницы {0} - {1}/{2}: Ok";
                                SiteWorker.CURRENT_INSTANCE.Log = String.Format(logMessage,
                                    Url,
                                    attemptsAmount.ToString(),
                                    MAX_ATTEMPTS.ToString());
                                break;

                            case HttpStatusCode.Moved:
                                attemptsRemain = MAX_ATTEMPTS;
                                Url = response.Headers.Location.AbsoluteUri;
                                logMessage = @"Перемещена страница {0} - {1}/{2}: Moved";
                                SiteWorker.CURRENT_INSTANCE.Log = String.Format(logMessage,
                                    Url,
                                    attemptsAmount.ToString(),
                                    MAX_ATTEMPTS.ToString());
                                break;
                        }
                    }
                } catch { }
            }

            return Success;
        }
    }
}
