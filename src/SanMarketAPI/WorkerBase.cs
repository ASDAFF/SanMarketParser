using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanMarketAPI
{
    /// <summary>
    /// Базовый класс для классов обработки данных
    /// </summary>
    public abstract class WorkerBase
    {
        #region Параметры и События журнала
        /// <summary>
        /// Событие изменения записи в журнале
        /// </summary>
        public event Action<WorkerBase, string> OnLogChanged;

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
    }
}
