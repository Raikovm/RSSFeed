using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using RSSFeed.Model;


namespace RSSFeed.ViewModel
{
    class MainViewModel
    {
        public ObservableCollection<Feed> Feeds { get; set; }
        public ObservableCollection<FeedItem> FeedItems { get; set; }
        public ObservableCollection<string> LinksArray { get; set; }

        private string _links;
        public string Links
        {
            get => _links;
            set
            {
                _links = value;
                LinksArray = new ObservableCollection<string>(value.Split('\n'));

                //Валидация введенных ссылок
                var i = 0;
                while (i < LinksArray.Count)
                {
                    if (UrlValidation(LinksArray[i]))
                        i++;
                    else
                    {
                        MessageBox.Show($"\"{LinksArray[i]}\" is not a valid HTTP URL!");
                        LinksArray.RemoveAt(i);
                    }
                }
            }
        }

        public bool TagsFormating { get; set; }

        private CancellationTokenSource _tokenSource;

        private int _refreshTime;
        public int RefreshTime
        {
            //Перевод из секунд в миллисекунды
            get => _refreshTime / 1000;
            set
            {
                //При изменении времени автообновления, процесс автообновления перезапускается 
                if (!_tokenSource.IsCancellationRequested)
                    _tokenSource.Cancel();
                _tokenSource.Dispose();

                _tokenSource = new CancellationTokenSource();
                CancellationToken token = _tokenSource.Token;
                new Task(AutoRefresh).Start();


                _refreshTime = value * 1000;
            }
        }

        private bool _autoRefreshIsOn;
        public bool AutoRefreshIsOn
        {
            get => _autoRefreshIsOn;
            set
            {
                _autoRefreshIsOn = value;
                _tokenSource = new CancellationTokenSource();
                CancellationToken token = _tokenSource.Token;
                //Запуск нового процесса автообновления
                if (value)
                {
                    new Task(AutoRefresh).Start();
                }
                //Отмена процесса автообновления
                else
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                }
            }
        }




        public MainViewModel()
        {
            Feeds = new ObservableCollection<Feed>();
            FeedItems = new ObservableCollection<FeedItem>();

            //Загрузка настроек из конфига 
            if (ConfigurationManager.AppSettings != null)
            {
                AutoRefreshIsOn = bool.Parse(ConfigurationManager.AppSettings?.Get("autoRefresh") ?? string.Empty);
                LinksArray = new ObservableCollection<string>(ConfigurationManager.AppSettings["links"]?.Split('|') ?? Array.Empty<string>());
                _links = ConfigurationManager.AppSettings.Get("links")?.Replace('|','\n');
                _refreshTime = int.Parse(ConfigurationManager.AppSettings.Get("refreshTime") ?? string.Empty);
            }

            RefreshFeeds();
        }

        //Команды для кнопки "Refresh feeds"
        private ICommand _refreshButtonClick;
        public ICommand RefreshButtonClick
        {
            get
            {
                return _refreshButtonClick ??= new CommandHandler(RefreshFeeds, () => true);
            }
        }

        //Обновление RSS лент
        private void RefreshFeeds()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                //Очистка и обновление коллекции лент
                Feeds.Clear();
                foreach (var s in LinksArray)
                    Feeds.Add(new Feed(s));

                //Очистка и обновление коллекции элементов лент, отображаемой на основном окне 
                FeedItems.Clear();
                foreach (var f in Feeds)
                {
                    f.Refresh(TagsFormating);
                    if (f.Items != null)
                        foreach (var fi in f.Items)
                        {
                            FeedItems.Add(fi);
                        }

                }
                //Сортировка новостей по дате 
                FeedItems = new ObservableCollection<FeedItem>(FeedItems.OrderByDescending(i => i.PubDate));

            });
        }


        private async void AutoRefresh()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(_refreshTime, _tokenSource.Token);
                }
                catch (Exception)
                {
                    return;
                }

                if (!_tokenSource.IsCancellationRequested)
                    RefreshFeeds();
                else
                    return;
            }
        }

        //Команды для кнопки "Save settings"
        private ICommand _saveSettingsButtonClick;
        public ICommand SaveSettingsButtonClick
        {
            get
            {
                return _saveSettingsButtonClick ??= new CommandHandler(SaveSettings, () => true);
            }
        }

        //Сохранение настроек в конфиг 
        private void SaveSettings()
        {
            UpdateConfig("links", _links.Replace('\n', '|'));
            UpdateConfig("refreshTime", _refreshTime.ToString());
            UpdateConfig("autoRefresh", AutoRefreshIsOn.ToString());
        }

        //Метод для упрощения взаимодействия с конфигом 
        private static void UpdateConfig(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configFile.AppSettings.Settings[key].Value = value;

            configFile.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }

        //Проверка валидности Url ссылок 
        private static bool UrlValidation(string s)
        {
            return Uri.TryCreate(s, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }



    }
}
