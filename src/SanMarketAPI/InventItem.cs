using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;

namespace SanMarketAPI
{
    /// <summary>
    /// Товар
    /// </summary>
    public class InventItem : SitePage
    {
        /// <summary>
        /// Наименование товара
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Краткое описание
        /// </summary>
        public string ShortDescr { get; set; }

        /// <summary>
        /// Полное описание
        /// </summary>
        public string Descr { get; set; }

        /// <summary>
        /// Список характеристик
        /// </summary>
        public List<InventItemOption> Options { get; set; }

        /// <summary>
        /// Список ссылок на изображения
        /// </summary>
        public List<string> Images { get; set; }

        /// <summary>
        /// Старая цена
        /// </summary>
        public double OldPrice { get; set; }

        /// <summary>
        /// Актуальная цена
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Группа, в которую входит товар
        /// </summary>
        public Group Parent { get; set; }

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        public InventItem() : base()
        {
            Name = "";
            ShortDescr = "";
            Descr = "";
            Options = new List<InventItemOption>();
            Images = new List<string>();
            OldPrice = 0;
            Price = 0;
            Parent = null;
        }

        /// <summary>
        /// Создание класса товара по ссылке и родителю
        /// </summary>
        /// <param name="__parent">Группа родитель</param>
        /// <param name="__url">Ссылка на страницу товара</param>
        public InventItem(Group __parent, string __url) : this()
        {
            Parent = __parent;
            Url = __url;
        }

        /// <summary>
        /// Заполнение данных о товаре с сайта поставщика
        /// </summary>
        public async Task<bool> FillByUrl()
        {
            bool res = false;

            if (await Loader.LoadPageAsync(Url))
            {
                // Получение наименования товара
                IHtmlHeadingElement pageHeader = (IHtmlHeadingElement)Loader.Document.QuerySelectorAll("h1")
                    .Where(item => item.TextContent.Trim().Length > 0)
                    .FirstOrDefault();
                if (pageHeader != null)
                    Name = pageHeader.TextContent.Trim();

                res = true;
            }

            return res;
        }

        /// <summary>
        /// Преобразование к строковому представлению товара
        /// </summary>
        /// <returns>Строковое представление товара</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
