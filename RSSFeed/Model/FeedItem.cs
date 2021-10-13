using System;
using System.Net.NetworkInformation;

namespace RSSFeed.Model
{
    class FeedItem
    {
        public string FeedName { get; set; }
        public string Title{ get; set; }
        public string Link { get; set; }
        public DateTime PubDate { get; set; }
        public string DescriptionWithoutTags { get; set; }
        public string DescriptionWithTags { get; set; }
        public bool EnableTags { get; set; }
    }
}
