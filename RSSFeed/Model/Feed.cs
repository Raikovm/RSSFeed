using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace RSSFeed.Model
{
    class Feed
    {
        public Feed(string feedUrl)
        {
            Url = feedUrl;
            if (!Refresh(false))
            {
                Items = null;
                Name = null;
                Source = null;
            }
        }

        public string Url { get; set; }
        public string Name { get; set; }
        public IEnumerable<FeedItem> Items { get; set; }
        public XDocument Source { get; set; }


        public bool Refresh(bool tagsEnabled)
        {
            try
            {
                //Получение XML файла 
                Source = XDocument.Load(Url);
                //Получение названия источника 
                Name = (from x in Source.Descendants("title") select x).FirstOrDefault()?.Value;
                //Получение элементов ленты 
                Items = (from x in Source.Descendants("item")
                         select new FeedItem
                         {
                             FeedName = Name,
                             Title = x.Element("title")?.Value,
                             Link = x.Element("link")?.Value,
                             PubDate = DateTime.Parse(x.Element("pubDate")?.Value),
                             DescriptionWithTags = x.Element("description")?.Value,
                             DescriptionWithoutTags = FormatDescription(x.Element("description")?.Value),
                             EnableTags = tagsEnabled
                         });

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading data!\n" + ex.Message);
                Items = null;
                return false;
            }
            return true;
        }

        //Очистка описания от HTML тегов 
        private static string FormatDescription(string desc)
        {
            var result = Regex.Replace(desc, @"\<.*?\>", "");
            result = Regex.Replace(result, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
            return result;
        }

    }
}
