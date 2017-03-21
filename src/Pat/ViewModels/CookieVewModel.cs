using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Windows.Input;
using Pat.Cookies;
using Pat.IO;

namespace Pat.ViewModels
{
    public class CookieVewModel : PropertyChangedBase, ICanSave, ICanLoad
    {
        private readonly FileManager _fileManager = new FileManager();
        public ObservableCollection<Cookie> Cookies { get; } = new ObservableCollection<Cookie>();

        public CookieContainer Container { get; private set; }
        public ICommand Clear { get; }
        public ICommand Refresh { get; }

        public CookieVewModel()
        {
            Container = new CookieContainer();
            Clear = new DelegateCommand(DoClearCookies);
            Refresh = new DelegateCommand(Update);
            Cookies.CollectionChanged += OnCookiesChanged;
        }

        private void OnCookiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (Cookie cookie in e.OldItems)
                    {
                        cookie.Expires = new DateTime(1970, 01, 01, 0, 0, 0);
                        Container.Add(cookie);
                    }
                    break;
            }
        }

        private void DoClearCookies()
        {
            Container = new CookieContainer();
            Cookies.Clear();
        }

        public void Update()
        {
            Cookies.Clear();
            foreach (var cookie in Container.GetAllCookies().OrderBy(c => c.Domain))
            {
                Cookies.Add(cookie);
            }
        }

        public void Save()
        {
            _fileManager.SaveBinary(Container);
        }

        public void Load()
        {
            Container = _fileManager.LoadBinaryOrDefault<CookieContainer>() ?? new CookieContainer();
            Update();
        }
    }
}