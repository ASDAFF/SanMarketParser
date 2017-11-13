using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
