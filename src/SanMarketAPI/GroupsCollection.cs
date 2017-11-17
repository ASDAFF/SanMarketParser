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
    /// Список групп товаров
    /// </summary>
    public class GroupsCollection : SitePage, IEnumerable<Group>
    {
        /// <summary>
        /// Список групп в коллекции
        /// </summary>
        public List<Group> Groups { get; set; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public GroupsCollection(): base()
        {
            // Инициализация коллекции
            Groups = new List<Group>();
            Url = Site.BASE_URL + @"/katalog-santehniki/";
        }

        /// <summary>
        /// Вычисляет количество дочерних групп для заданной
        /// </summary>
        /// <param name="__group">Группа для анализа</param>
        /// <returns>Количество дочерних групп</returns>
        public int GetChildAmount(Group __group)
        {
            int res = 0;

            foreach (Group item in this)
                if (item.Parent != null && item.Parent.Equals(__group))
                    res++;

            return res;
        }

        /// <summary>
        /// Загрузка всей структуры групп товаров
        /// </summary>
        /// <returns>Список групп товаров</returns>
        public static async Task<GroupsCollection> GetGroupsCollection()
        {
            GroupsCollection groupsCollection = new GroupsCollection();

            if (await groupsCollection.Loader.LoadPageAsync(groupsCollection.Url))
            {
                // Для разбора наименования группы
                string pattern = @"^(?<name>[^<]+)";
                Regex regex = new Regex(pattern);
                Match match;
                string categoryName;

                var catalogBlockList = groupsCollection.Loader.Document.QuerySelectorAll("div")
                    .Where(item => item.ClassName != null
                    && item.ClassName.Contains("category"));
                foreach (IHtmlDivElement catalogBlock in catalogBlockList)
                {
                    if (!SiteWorker.CURRENT_INSTANCE.IsActive)
                        return groupsCollection;

                    IHtmlAnchorElement catalogAnchor = (IHtmlAnchorElement)catalogBlock.QuerySelectorAll("a").FirstOrDefault();
                    if (catalogAnchor != null)
                    {
                        IHtmlParagraphElement catalogLabel = (IHtmlParagraphElement)catalogAnchor.QuerySelectorAll("p").FirstOrDefault();
                        if (catalogLabel != null)
                        {
                            match = regex.Match(catalogLabel.InnerHtml);
                            if (match.Success)
                            {
                                categoryName = match.Groups["name"].Value.Trim();

                                Group groupItem = new Group(categoryName, Site.PrepareUrl(catalogAnchor.Href.Trim()));
                                groupsCollection.Groups.Add(groupItem); // Добавление корневой группы
                                groupItem.RegisterGroup();

                                // Загрузка дочерних групп
                                GroupsCollection subgroupsCollection = await groupItem.GetSubgroupsCollection();
                                groupsCollection.Groups.AddRange(subgroupsCollection);
                            }
                        }
                    }
                }

                // Обновление уровней подчинённости в таблице групп
                groupsCollection.UpdateDBParents();
            }
            else
            {
                string logMessage = @"Ошибка при загрузке страницы: {0}";
                SiteWorker.CURRENT_INSTANCE.Log = String.Format(logMessage, groupsCollection.Url);
                SiteWorker.CURRENT_INSTANCE.Stop();
            }

            return groupsCollection;
        }

        /// <summary>
        /// Обновление уровней подчинённости в таблице групп
        /// </summary>
        public void UpdateDBParents()
        {
            if (Groups.Count > 0)
            {
                dsParserTableAdapters.tGroupsTableAdapter ta = new dsParserTableAdapters.tGroupsTableAdapter();
                dsParser.tGroupsDataTable tbl;
                dsParser.tGroupsRow row;

                foreach (Group item in this)
                {
                    tbl = ta.GetDataByUrl(item.Url);
                    if (tbl.Rows.Count > 0)
                    {
                        row = (dsParser.tGroupsRow)tbl.Rows[0];

                        // Определение значения идентификатора группы родителя
                        int newParent = item.ParentId;

                        // Установка ИД. родительской группы
                        if (row.ParentId != newParent)
                        {
                            row.ParentId = (int)newParent;
                            ta.Update(tbl);
                        }
                    }
                }
            }
        }

#region Реализация Интерфейса Коллекции
        public IEnumerator<Group> GetEnumerator()
        {
            foreach (Group item in Groups)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
#endregion

    }
}
