using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SanMarketAPI
{
    /// <summary>
    /// Управляющий класс процессом парсинга
    /// </summary>
    public class SiteWorker
    {
        /// <summary>
        /// Вызывается при любом виде завершения процесса парсинга
        /// </summary>
        public event Action<SiteWorker> OnParsingCompleted;

        #region Параметры и События журнала
        /// <summary>
        /// Событие изменения записи в журнале
        /// </summary>
        public event Action<SiteWorker, string> OnLogChanged;

        /// <summary>
        /// Сообщение журнала
        /// </summary>
        private string _log;

        /// <summary>
        /// Сообщение журнала
        /// </summary>
        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                OnLogChanged?.Invoke(this, value);
            }
        }
        #endregion

        /// <summary>
        /// Признак активности процесса парсинга
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Признак активности процесса парсинга
        /// </summary>
        public bool IsActive { get { return _isActive; } }

        /// <summary>
        /// Список категорий товаров
        /// </summary>
        public GroupsCollection Groups { get; set; }

        /// <summary>
        /// Список товаров
        /// </summary>
        public InventCollection InventItems { get; set; }

        /// <summary>
        /// Единый объект управления парсингом
        /// </summary>
        private static SiteWorker _currentInstance = null;

        /// <summary>
        /// Единый объект управления парсингом
        /// </summary>
        public static SiteWorker CURRENT_INSTANCE
        {
            get
            {
                if (SiteWorker._currentInstance == null)
                    SiteWorker._currentInstance = new SiteWorker();
                return SiteWorker._currentInstance;
            }
        }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        public SiteWorker()
        {
            _isActive = false;

            Groups = null;
            InventItems = null;
        }

        /// <summary>
        /// Запуск процедуры парсинга товаров
        /// </summary>
        public void Start()
        {
            if (!IsActive)
            {
                _isActive = true;
                Log = @"Начало парсинга";

                Worker();
            }
        }

        /// <summary>
        /// Рабочий процесс
        /// </summary>
        private async void Worker()
        {
            Groups = await GroupsCollection.GetGroupsCollection();
            InventItems = await InventCollection.GetInventCollection(Groups);

            OnParsingCompleted?.Invoke(this);
        }

        /// <summary>
        /// Остановка процедуры парсинга товаров
        /// </summary>
        public void Stop()
        {
            if (IsActive)
            {
                _isActive = false;
                Log = @"Запрос на остановку парсинга";
            }
        }
    }
}
