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
        public string Name
        {
            get { return _name; }

            set { _name = value.Trim().Replace(@"&nbsp;", " "); }
        }
        public string _name;

        /// <summary>
        /// Краткое описание
        /// </summary>
        public string ShortDescr { get; set; }

        /// <summary>
        /// Полное описание
        /// </summary>
        public string Descr
        {
            get { return _descr; }

            set { _descr = value.Trim().Replace(@"&nbsp;", " "); }
        }
        public string _descr;

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

                // Получение цены товара
                IHtmlSpanElement mainPrice = (IHtmlSpanElement)Loader.Document.QuerySelectorAll("span")
                    .Where(item => item.Id != null
                    && item.Id.Trim() == @"main-price")
                    .FirstOrDefault();
                if (mainPrice != null)
                {
                    int mainPriceValue = 0;
                    if (int.TryParse(mainPrice.TextContent.Trim().Replace(" ", string.Empty), out mainPriceValue))
                        Price = mainPriceValue;
                }

                // Получение старой цены товара
                IHtmlDivElement oldPriceElement = (IHtmlDivElement)Loader.Document.QuerySelectorAll("div")
                    .Where(item => item.ClassName != null
                    && item.ClassName.Contains(@"old-price"))
                    .FirstOrDefault();
                if (oldPriceElement != null)
                {
                    Regex rgOldPrice = new Regex(@"^(?<value>\d+)");
                    Match mtOldPrice = rgOldPrice.Match(oldPriceElement.TextContent.Trim().Replace(" ", string.Empty));
                    if (mtOldPrice.Success)
                    {
                        int oldPriceValue = 0;
                        if (int.TryParse(mtOldPrice.Groups["value"].ToString(), out oldPriceValue))
                            OldPrice = oldPriceValue;
                    }
                }

                // Загрузка списка технических характеристик
                IHtmlElement techSpecSection = (IHtmlElement)Loader.Document.QuerySelectorAll("section")
                    .Where(item => item.ClassName != null
                    && item.ClassName.Contains(@"tech-specs"))
                    .FirstOrDefault();
                if (techSpecSection != null)
                {
                    var techSpecRowsCollection = techSpecSection.QuerySelectorAll("div")
                        .Where(item => item.ClassName != null
                        && item.ClassName.Contains(@"row"));
                    foreach (IHtmlDivElement techSpecRow in techSpecRowsCollection)
                    {
                        IHtmlSpanElement techSpecName = (IHtmlSpanElement)techSpecRow.QuerySelectorAll("span")
                            .FirstOrDefault();
                        IHtmlParagraphElement techSpecValue = (IHtmlParagraphElement)techSpecRow.QuerySelectorAll("p")
                            .Where(item => item.ClassName != null
                            && item.ClassName.Contains(@"p-style2"))
                            .FirstOrDefault();
                        if (techSpecName != null 
                            && techSpecValue != null 
                            && techSpecName.TextContent.Trim().Length > 0
                            && techSpecValue.TextContent.Trim().Length > 0)
                        {
                            Options.Add(new InventItemOption(techSpecName.TextContent.Trim(), techSpecValue.TextContent.Trim()));
                        }
                    }
                }

                // Загрузка описания товара
                IHtmlDivElement descrElement = (IHtmlDivElement)Loader.Document.QuerySelectorAll("div")
                    .Where(item => item.Id != null
                    && item.Id.Trim() == @"descr")
                    .FirstOrDefault();
                if (descrElement != null)
                    Descr = descrElement.TextContent.Trim();

                // Формирование списка изображений товара
                IHtmlUnorderedListElement imagesListElement = (IHtmlUnorderedListElement)Loader.Document.QuerySelectorAll("ul")
                    .Where(item => item.ClassName != null
                    && item.ClassName.Contains(@"pagination"))
                    .FirstOrDefault();
                if (imagesListElement != null)
                {
                    var imagesListCallection = imagesListElement.QuerySelectorAll("img");
                    foreach (IHtmlImageElement imageItem in imagesListCallection)
                        if (imageItem.Source != null && imageItem.Source.Length > 0)
                        {
                            Images.Add(Site.PrepareUrl(imageItem.Source));
                        }
                }

                res = true;
            }

            RegisterInventItem();

            return res;
        }

        /// <summary>
        /// Записывает данные товара в БД
        /// </summary>
        public void RegisterInventItem()
        {
            if (Url.Length > 0 && Parent != null)
            {
                dsParserTableAdapters.tInventItemsTableAdapter taInventItems = new dsParserTableAdapters.tInventItemsTableAdapter();
                dsParser.tInventItemsDataTable tblInventItems = taInventItems.GetDataByUrl(Url);
                dsParser.tInventItemsRow rowInventItems;

                if (tblInventItems.Rows.Count > 0)
                    rowInventItems = (dsParser.tInventItemsRow)tblInventItems.Rows[0];
                else
                {
                    rowInventItems = tblInventItems.NewtInventItemsRow();
                    rowInventItems.Url = Url;
                }

                // Обновление данных товара
                rowInventItems.ParentId = Parent.Id;
                rowInventItems.Name = Name;
                rowInventItems.ShortDescr = ShortDescr;
                rowInventItems.Descr = Descr;
                rowInventItems.OldPrice = (decimal)OldPrice;
                rowInventItems.Price = (decimal)Price;

                if (rowInventItems.RowState == System.Data.DataRowState.Detached)
                    tblInventItems.Rows.Add(rowInventItems);

                taInventItems.Update(tblInventItems);


                // Сохранение данных о картинках
                dsParserTableAdapters.tInventImagesTableAdapter taImages = new dsParserTableAdapters.tInventImagesTableAdapter();
                dsParser.tInventImagesDataTable tblImages;
                dsParser.tInventImagesRow rowImages;
                foreach (string imageUrl in Images)
                {
                    tblImages = taImages.GetDataByUrl(rowInventItems.Id, imageUrl);
                    if (tblImages.Rows.Count == 0)
                    {
                        rowImages = tblImages.NewtInventImagesRow();
                        rowImages.InventItemId = rowInventItems.Id;
                        rowImages.Url = imageUrl;
                        tblImages.Rows.Add(rowImages);
                        taImages.Update(tblImages);
                    }
                }


                // Сохранение данных о характеристиках
                dsParserTableAdapters.tInventOptionsTableAdapter taOptions = new dsParserTableAdapters.tInventOptionsTableAdapter();
                dsParser.tInventOptionsDataTable tblOptions;
                dsParser.tInventOptionsRow rowOptions;
                foreach (InventItemOption optionItem in Options)
                {
                    tblOptions = taOptions.GetDataByName(rowInventItems.Id, optionItem.Name);

                    if (tblOptions.Rows.Count > 0)
                        rowOptions = (dsParser.tInventOptionsRow)tblOptions.Rows[0];
                    else
                    {
                        rowOptions = tblOptions.NewtInventOptionsRow();
                        rowOptions.InventItemId = rowInventItems.Id;
                        rowOptions.Name = optionItem.Name;
                    }

                    rowOptions.Value = optionItem.Value;

                    if (rowOptions.RowState == System.Data.DataRowState.Detached)
                        tblOptions.Rows.Add(rowOptions);

                    taOptions.Update(tblOptions);
                }
            }
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
