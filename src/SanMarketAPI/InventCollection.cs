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
    /// Список товаров
    /// </summary>
    public class InventCollection : SitePage, IEnumerable<InventItem>
    {
        /// <summary>
        /// Список товаров
        /// </summary>
        public List<InventItem> Items { get; set; }

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        public InventCollection() : base()
        {
            Items = new List<InventItem>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<InventCollection> GetInventCollection(GroupsCollection __groupsCollection)
        {
            InventCollection inventCollection = new InventCollection();

            // Под выборку попадают только группы без подчинённых групп
            foreach (Group group in __groupsCollection)
            {
                if (__groupsCollection.GetChildAmount(group) == 0)
                {

                    // Перебираем страницы с товарами категории
                    string nextUrl = group.Url;
                    while (nextUrl.Length > 0)
                    {
                        if (!SiteWorker.CURRENT_INSTANCE.IsActive)
                            return inventCollection;

                        if (await group.Loader.LoadPageAsync(nextUrl))
                        {
                            // Перебор товарных позиций
                            var catalogItemsList = group.Loader.Document.QuerySelectorAll("div")
                                .Where(item => item.ClassName != null
                                && item.ClassName.Contains("bx_catalog_item"));
                            foreach (IHtmlDivElement catalogItem in catalogItemsList)
                            {
                                if (!SiteWorker.CURRENT_INSTANCE.IsActive)
                                    return inventCollection;

                                // Определение ссылки на товар
                                IHtmlAnchorElement catalogItemAnchor = (IHtmlAnchorElement)catalogItem.QuerySelectorAll("a")
                                    .Where(item => item.ClassName != null
                                    && item.ClassName.Contains("img-cont"))
                                    .FirstOrDefault();
                                if (catalogItemAnchor != null)
                                {
                                    string catalogItemUrl = Site.PrepareUrl(catalogItemAnchor.Href.Trim());
                                    InventItem inventItem = new InventItem(group, catalogItemUrl);
                                    if (await inventItem.FillByUrl())
                                    {
                                        if (inventCollection.Items.Count > 200)
                                            inventCollection = new InventCollection();
                                        inventCollection.Items.Add(inventItem);
                                    }
                                }
                            }

                            // Поиск перехода к следующей странице
                            IHtmlAnchorElement nextAnchor = (IHtmlAnchorElement)group.Loader.Document.QuerySelectorAll("a")
                                .Where(item => item.ClassName != null
                                && item.ClassName.Contains("modern-page-next")
                                && item.TextContent.Trim() == @"След.")
                                .FirstOrDefault();
                            if (nextAnchor != null)
                                nextUrl = Site.PrepareUrl(nextAnchor.Href.Trim());
                            else
                                nextUrl = "";
                        }
                        else
                        {
                            string logMessage = @"Ошибка при загрузке страницы: {0}";
                            SiteWorker.CURRENT_INSTANCE.Log = String.Format(logMessage, nextUrl);
                            SiteWorker.CURRENT_INSTANCE.Stop();
                            nextUrl = "";
                        }
                    }

                }
            }

            return inventCollection;
        }

        #region Реализация интерфейса списка
        public IEnumerator<InventItem> GetEnumerator()
        {
            return ((IEnumerable<InventItem>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<InventItem>)Items).GetEnumerator();
        }
        #endregion 
    }
}
