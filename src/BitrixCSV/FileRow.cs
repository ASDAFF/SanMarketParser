using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixCSV
{
    /// <summary>
    /// Строка файла экспорта данных
    /// </summary>
    public class FileRow
    {
        /// <summary>
        /// Внешний код (уникальный идентификатор)
        /// (B_IBLOCK_ELEMENT.XML_ID)
        /// </summary>
        public int XmlId { get; set; }

        /// <summary>
        /// Название
        /// (B_IBLOCK_ELEMENT.NAME)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание для анонса (списка)
        /// (B_IBLOCK_ELEMENT.PREVIEW_TEXT)
        /// </summary>
        public string PreviewText { get; set; }

        /// <summary>
        /// Детальное описание
        /// (B_IBLOCK_ELEMENT.DETAIL_TEXT)
        /// </summary>
        public string DetailText { get; set; }

        /// <summary>
        /// Свойство "Артикул"
        /// [ARTNUMBER]
        /// </summary>
        public string ArtNumber { get; set; }

        /// <summary>
        /// Свойство "Картинки галереи"
        /// [MORE_PHOTO]
        /// </summary>
        public string MorePhoto { get; set; }

        /// <summary>
        /// Раздел уровня 1
        /// Название группы (B_IBLOCK_SECTION.NAME)
        /// </summary>
        public string Group_0 { get; set; }

        /// <summary>
        /// Раздел уровня 2
        /// Название группы (B_IBLOCK_SECTION.NAME)
        /// </summary>
        public string Group_1 { get; set; }

        /// <summary>
        /// Раздел уровня 3
        /// Название группы (B_IBLOCK_SECTION.NAME)
        /// </summary>
        public string Group_2 { get; set; }

        /// <summary>
        /// Тип цен "Розничная цена" (BASE): Цена
        /// (B_CATALOG_PRICE.PRICE)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Тип цен "Розничная цена" (BASE): Валюта
        /// (B_CATALOG_PRICE.CURRENCY)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Разделитель полей
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Базовый конструктор с инициализацией базовых значений
        /// </summary>
        public FileRow()
        {
            Delimiter = @";";

            XmlId = 0;
            Name = "";
            PreviewText = "";
            DetailText = "";
            ArtNumber = "";
            MorePhoto = "";
            Group_0 = "";
            Group_1 = "";
            Group_2 = "";
            Price = 0;
            Currency = @"RUB";
        }

        /// <summary>
        /// Приведение структуры к текстовому представлению строки файла экспорта
        /// </summary>
        /// <returns>Строка данных файла экспорта</returns>
        public override string ToString()
        {
            string res = XmlId.ToString() + Delimiter +
                Name.Trim() + Delimiter +
                PreviewText.Trim() + Delimiter +
                DetailText.Trim() + Delimiter +
                ArtNumber.Trim().ToLower() + Delimiter +
                ArtNumber.Trim() + Delimiter +
                MorePhoto + Delimiter +
                MorePhoto + Delimiter +
                MorePhoto + Delimiter +
                Group_0.Trim() + Delimiter +
                Group_1.Trim() + Delimiter +
                Group_2.Trim() + Delimiter +
                Price.ToString().Replace(",", ".") + Delimiter +
                Currency.Trim();

            return res;
        }

        /// <summary>
        /// Строка с заголовками полей файла экспорта
        /// с разделителем полей по-умолчанию
        /// </summary>
        /// <returns></returns>
        public static string FileHeaderString()
        {
            return FileHeaderString(@";");
        }

        /// <summary>
        /// Строка с заголовками полей файла экспорта
        /// </summary>
        /// <param name="__delimiter">Разделитель полей</param>
        /// <returns></returns>
        public static string FileHeaderString(string __delimiter)
        {
            string res = @"IE_XML_ID;IE_NAME;IE_PREVIEW_TEXT;IE_DETAIL_TEXT;IE_CODE;ARTNUMBER;IE_PREVIEW_PICTURE;IE_DETAIL_PICTURE;MORE_PHOTO;IC_GROUP0;IC_GROUP1;IC_GROUP2;CV_PRICE_1;CV_CURRENCY_1";
            return res.Replace(@";", __delimiter);
        }
    }
}
