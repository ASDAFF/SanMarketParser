using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;

namespace SanMarketAPI
{
    /// <summary>
    /// Группа товаров
    /// </summary>
    public class Group : SitePage
    {
        /// <summary>
        /// Наименование группы товаров
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Родительская группа товаров
        /// </summary>
        public Group Parent { get; set; }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        public Group() : base()
        {
            Name = "";
            Parent = null;
        }

        public Group(string __name, string __url) : this()
        {
            Name = __name;
            Url = __url;
        }

        public Group(string __name, string __url, Group __parent) : this(__name, __url)
        {
            Parent = __parent;
        }

        /// <summary>
        /// Возвращает дочерние группы товаров
        /// </summary>
        /// <returns>Коллекция подгрупп текущей группы товаров</returns>
        public async Task<GroupsCollection> GetSubgroupsCollection()
        {
            GroupsCollection groupsCollection = new GroupsCollection();

            if (await Loader.LoadPageAsync(Url))
            {
                // Для разбора наименования группы
                string pattern = @"^(?<name>[^<]+)";
                Regex regex = new Regex(pattern);
                Match match;
                string subUrl, categoryName;

                // Получение коллекций списков субкатегорий
                var subcategoryListsCollection = Loader.Document.QuerySelectorAll("ul")
                    .Where(item => item.ClassName != null
                    && item.ClassName.Contains("scroll-pane"));
                foreach (IHtmlUnorderedListElement subcategoryList in subcategoryListsCollection)
                {
                    // Перебор подкатегорий
                    var anchorsList = subcategoryList.QuerySelectorAll("a");
                    foreach(IHtmlAnchorElement anchorItem in anchorsList)
                    {
                        subUrl = Site.BASE_URL + anchorItem.PathName;
                        if (subUrl.Length > Url.Length && Url == subUrl.Substring(0, Url.Length))
                        {
                            match = regex.Match(anchorItem.InnerHtml);
                            if (match.Success)
                            {
                                categoryName = match.Groups["name"].Value.Trim();

                                Group groupItem = new Group(categoryName, subUrl, this);
                                groupsCollection.Groups.Add(groupItem);
                            }
                        }
                    }
                }
            }

            return groupsCollection;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
