using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TapTap.Bootstrap;
using TapTap.Common;
using XD.Intl.Common;

namespace XD.Intl.Account
{
    public class XDGAccountImpl
    {
        private XDGAccountImpl()
        {
            EngineBridge.GetInstance()
                .Register(XDGUnityBridge.ACCOUNT_SERVICE_NAME, XDGUnityBridge.ACCOUNT_SERVICE_IMPL);
        }

        private readonly string XDG_ACCOUNT_SERVICE = "XDGLoginService"; //注意要和iOS本地的桥接文件名一样！ 
        private static volatile XDGAccountImpl _instance;
        private static readonly object Locker = new object();

        public static XDGAccountImpl GetInstance()
        {
            lock (Locker)
            {
                if (_instance == null)
                {
                    _instance = new XDGAccountImpl();
                }
            }

            return _instance;
        }

        public  void Login(Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            try
            {
                var command = new Command(XDG_ACCOUNT_SERVICE, "login", true, null);
                 EngineBridge.GetInstance().CallHandler(command, result => {
                     XDGTool.Log("Login 方法结果: " + result.ToJSON());
                     if (!XDGTool.checkResultSuccess(result))
                     {
                         errorCallback(new XDGError(result.code, result.message));
                         return;
                     }

                     XDGUserWrapper userWrapper = new XDGUserWrapper(result.content);
                     if (userWrapper.error != null)
                     {
                         errorCallback(userWrapper.error);
                         return;
                     }

                     callback(userWrapper.user);
                     
                     ActiveLearnCloudToken();
                     XDGTool.Log("login block end");
                 });
               
            }
            catch (Exception e){
                XDGTool.LogError("Login 报错");
                Console.WriteLine(e);
            }
        }
        
        public void ActiveLearnCloudToken(){
            try{
                XDGTool.Log("LoginSync 开始执行  ActiveLearnCloudToken");
                var command = new Command(XDG_ACCOUNT_SERVICE, "loginSync", true, null);
                EngineBridge.GetInstance().CallHandler(command, (async result => {
                    XDGTool.Log("LoginSync 方法结果: " + result.ToJSON());
                    if (!XDGTool.checkResultSuccess(result)){
                        return;
                    }

                    Dictionary<string, object> contentDic = Json.Deserialize(result.content) as Dictionary<string, object>;
                    string token = SafeDictionary.GetValue<string>(contentDic, "sessionToken");
                    await TDSUser.BecomeWithSessionToken(token);
                    XDGTool.Log("LoginSync  BecomeWithSessionToken 执行完毕");
                }));
                
            }catch (Exception e){
                XDGTool.LogError("LoginSync 报错");
                Console.WriteLine(e);
            }
        }

        public void Logout(){
            var command = new Command(XDG_ACCOUNT_SERVICE, "logout", false, null);
            EngineBridge.GetInstance().CallHandler(command);
            TDSUser.Logout();  //退出LC
        }

        public void AddUserStatusChangeCallback(Action<int, string> callback)
        {
            var command = new Command(XDG_ACCOUNT_SERVICE, "addUserStatusChangeCallback", true,
                null);
            EngineBridge.GetInstance().CallHandler(command, (result) =>
            {
                XDGTool.Log("AddUserStatusChangeCallback 方法结果: " + result.ToJSON());
                if (!XDGTool.checkResultSuccess(result))
                {
                    return;
                }

                XDGUserStatusChangeWrapper wrapper = new XDGUserStatusChangeWrapper(result.content);
                callback(wrapper.code, wrapper.message);
            });
        }

        public void GetUser(Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            var command = new Command(XDG_ACCOUNT_SERVICE, "getUser", true, null);
            EngineBridge.GetInstance().CallHandler(command, result =>
            {
                XDGTool.Log("GetUser 方法结果: " + result.ToJSON());
                if (!XDGTool.checkResultSuccess(result))
                {
                    errorCallback(new XDGError(result.code, result.message));
                    return;
                }

                XDGUserWrapper userWrapper = new XDGUserWrapper(result.content);
                if (userWrapper.error != null)
                {
                    errorCallback(userWrapper.error);
                    return;
                }

                callback(userWrapper.user);
            });
        }

        public void OpenUserCenter()
        {
            var command = new Command(XDG_ACCOUNT_SERVICE, "openUserCenter", false, null);
            EngineBridge.GetInstance().CallHandler(command);
        }

        public void LoginByType(LoginType loginType, Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            var command = new Command.Builder()
                .Service(XDG_ACCOUNT_SERVICE)
                .Method("loginByType")
                .Args("loginType", XDGUser.GetLoginTypeString(loginType)) //和app交互用的是字符串，如TapTap 
                .Callback(true)
                .CommandBuilder();
            
            XDGTool.Log("调用方法：loginByType ");
            EngineBridge.GetInstance().CallHandler(command, result =>
            {
                XDGTool.Log("LoginByType 方法结果: " + result.ToJSON());
                if (!XDGTool.checkResultSuccess(result))
                {
                    errorCallback(new XDGError(result.code, result.message));
                    return;
                }

                XDGUserWrapper wrapper = new XDGUserWrapper(result.content);
                if (wrapper.error != null)
                {
                    errorCallback(wrapper.error);
                    return;
                }

                callback(wrapper.user);
            });
        }
    }
}