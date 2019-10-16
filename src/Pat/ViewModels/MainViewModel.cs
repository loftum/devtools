using System;
using System.Net;
using System.Windows.Input;
using Pat.IO;
using Pat.Models;

namespace Pat.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private readonly VmFactory _vmFactory = new VmFactory();
        private bool _isNotBusy;
        public IGlobalSettings GlobalSettings { get; }
        public IInput Inputs { get; }
        public Request Request { get; set; }
        public Response Response { get; }
        public ResponseContent ResponseContent { get; }
        public CookieVewModel Cookies { get; }
        public ICommand Execute { get; }

        public bool IsNotBusy
        {
            get { return _isNotBusy; }
            set
            {
                _isNotBusy = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            var fileManager = new FileManager();
            
            GlobalSettings = _vmFactory.Build<IGlobalSettings>()
                .WithTarget(fileManager.LoadJsonOrDefault<GlobalSettings>() ?? new GlobalSettings())
                .WithPropertyChangeNotifier()
                .Persistent(fileManager.SaveJson)
                .Create();
            Inputs = _vmFactory.Build<IInput>()
                .WithTarget(fileManager.LoadJsonOrDefault<Input>() ?? new Input())
                .WithPropertyChangeNotifier()
                .Persistent(fileManager.SaveJson)
                .Create();
            Request = _vmFactory.Build<Request>().Create();
            Response = _vmFactory.Build<Response>().Create();
            ResponseContent = _vmFactory.Build<ResponseContent>().Create();
            Cookies = new CookieVewModel();
            Execute = new DelegateCommand(DoExecute);
            IsNotBusy = true;
        }

        public async void DoExecute()
        {
            try
            {
                IsNotBusy = false;
                ServicePointManager.SecurityProtocol = GlobalSettings.TLSVersion;
                ServicePointManager.Expect100Continue = GlobalSettings.Expect100Continue;
                ServicePointManager.CheckCertificateRevocationList = GlobalSettings.CheckCertificateRevocationList;
                
                var request = Inputs.CreateRequest();
                
                request.CookieContainer = Cookies.Container;
                try
                {
                    ResponseContent.Body = null;
                    using (var response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        Response.Update(response);
                        await ResponseContent.Update(response);
                    }
                }
                finally
                {
                    Request.Update(request);
                    Cookies.Update();
                }
                
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response)
                {
                   Response.Update(response);
                }
                else
                {
                    Response.Clear();
                    ResponseContent.Body = ex.ToString();
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                ResponseContent.Body = ex.ToString();
            }
            finally
            {
                IsNotBusy = true;
            }
        }

        public void Save()
        {
            _vmFactory.Save();
            Cookies.Save();
        }

        public void Load()
        {
            Cookies.Load();
        }
    }
}