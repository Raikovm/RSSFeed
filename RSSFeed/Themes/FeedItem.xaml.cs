using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RSSFeed.Themes
{
    public partial class FeedItem : ResourceDictionary
    {
        public FeedItem()
        {
            InitializeComponent();
        }

        //Метод для открытия статьи в браузере
        private void Title_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = ((TextBlock) sender).Tag.ToString();
            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        //Назначение элементу WebBrowser текста описания статьи 
        private void WebBrowserInitialized(object sender, EventArgs e)
        {
            if (sender is WebBrowser wb)
                wb.NavigateToString("<meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>" +  //исправление кодировки 
                                    wb.Tag.ToString());
        }

        //Метод для открытия ссылок в элементе WebBrowser в браузере по умолчанию
        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri == null)
                return;
            try
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {e.Uri.AbsoluteUri}") { CreateNoWindow = true });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            e.Cancel = true;
        }

        //Ворматированиен отображения элемента WebBrowser (автоматическое масштабирование, сокрытие скроллбара)
        private void WebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var script = "document.body.style.overflow ='hidden'";
            if (sender is not WebBrowser wb) 
                return;
            wb.Height = (int) (wb.Document as dynamic).body.scrollHeight + 20;
            wb.InvokeScript("execScript", new object[] {script, "JavaScript"});
        }
    }
}
