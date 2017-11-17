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

        public InventItemOption(string __name, string __value)
        {
            Name = __name;
            Value = __value;
        }

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}
