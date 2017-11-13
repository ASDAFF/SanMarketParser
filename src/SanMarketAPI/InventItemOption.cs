namespace SanMarketAPI
{
    /// <summary>
    /// Характеристика товара
    /// </summary>
    public struct InventItemOption
    {
        /// <summary>
        /// Наименование характеристики
        /// </summary>
        public string Name;

        /// <summary>
        /// Значение характеристики
        /// </summary>
        public string Value;

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}
