using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCloud.Storage;
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

        public void Login(Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            var command = new Command(XDG_ACCOUNT_SERVICE, "login", true, null);
            EngineBridge.GetInstance().CallHandler(command, result =>
            {
                try
                {
                    XDGTool.Log("Login 方法结果: " + result.ToJSON());
                    if (!XDGTool.checkResultSuccess(result))
                    {
                        XDGTool.LogError($"Login 登录失败1：code:{result.code}  msg:{result.message}");
                        errorCallback(new XDGError(result.code, result.message));
                        return;
                    }

                    var userWrapper = new XDGUserWrapper(result.content);
                    if (userWrapper.error != null)
                    {
                        XDGTool.LogError($"Login 登录失败2 code:{userWrapper.error.code}  msg:{userWrapper.error.error_msg}");
                        errorCallback(userWrapper.error);
                        return;
                    }
                    ActiveLearnCloudToken(userWrapper.user, callback, errorCallback);
                    
                    XDGTool.Log("login block end");
                }
                catch (Exception e)
                {
                    errorCallback(new XDGError(result.code, result.message));
                    XDGTool.LogError("Login 报错" + e.Message);
                    Console.WriteLine(e);
                }
            });
        }

        private async void ActiveLearnCloudToken(XDGUser user, Action<XDGUser> callback, Action<XDGError> errorCallback){
            if (user == null || XDGTool.IsEmpty(user.userId)){
                errorCallback(new XDGError(-1001, "user is null"));
                XDGTool.LogError("LoginSync 报错：user 是空！");
                return;
            } else{
                XDGTool.userId = user.userId; //日志打印用
            }

            TDSUser preUser = await TDSUser.GetCurrent();
            if (preUser != null){
                if (preUser.ObjectId == user.userId) {
                    XDGTool.Log("LoginSync 使用local pre user");
                    callback(user);
                    return;
                } else{
                    // id 不同可能是有残存的数据，则清空后走重新创建逻辑
                    await LCUser.Logout();   
                }
            }
            
            XDGCommon.ShowLoading();
            XDGTool.Log("LoginSync 开始执行  ActiveLearnCloudToken");
            var resultJson = "空";
            var command = new Command(XDG_ACCOUNT_SERVICE, "loginSync", true, null);
            EngineBridge.GetInstance().CallHandler(command, (async result =>
            {
                try{
                    resultJson = result.ToJSON();
                    XDGTool.Log("LoginSync 方法结果: " + resultJson);
                    if (!XDGTool.checkResultSuccess(result)){
                        XDGCommon.HideLoading();
                        errorCallback(new XDGError(result.code, result.message));
                        return;
                    }

                    var contentDic = Json.Deserialize(result.content) as Dictionary<string, object>;
                    var sessionToken = SafeDictionary.GetValue<string>(contentDic, "sessionToken");
                    var errorDic = SafeDictionary.GetValue<Dictionary<string, object>>(contentDic, "error");

                    if (errorDic != null){ //接口失败
                        XDGCommon.HideLoading();
                        errorCallback(new XDGError(errorDic));
                        XDGTool.LogError("LoginSync 报错：请求sessionToken接口失败， 【result结果：" + resultJson + "】");
                        return;
                    }

                    if (XDGTool.IsEmpty(sessionToken)){//接口成功，token是空(不太可能吧)
                        XDGCommon.HideLoading();
                        errorCallback(new XDGError(-1000, "sessionToken is null"));
                        XDGTool.LogError("LoginSync 报错：token 是空！ 【result结果：" + resultJson + "】");
                        return;
                    }

                    LCUser lcUser = LCObject.CreateWithoutData(LCUser.CLASS_NAME, user.userId) as LCUser;
                    lcUser.SessionToken = sessionToken;
                    await lcUser.SaveToLocal(); 
                        
                    callback(user);
                    XDGCommon.HideLoading();
                    XDGTool.Log("LoginSync  BecomeWithSessionToken 执行完毕");
                    
                } catch (Exception e){
                    XDGCommon.HideLoading();
                    errorCallback(new XDGError(result.code, result.message));
                    if (e.InnerException != null){
                        XDGTool.LogError("LoginSync 报错：" + e.Message + e.StackTrace + "【InnerException： " +e.InnerException.Message + e.InnerException.StackTrace+ "】" +"。 【result结果：" + resultJson + "】");
                    } else{
                        XDGTool.LogError("LoginSync 报错：" + e.Message + e.StackTrace + "。 【result结果：" + resultJson + "】");   
                    }
                    Console.WriteLine(e);
                }
            }));
        }


        public async void Logout()
        {
            await TDSUser.Logout(); //退出LC
            var command = new Command(XDG_ACCOUNT_SERVICE, "logout", false, null);
            EngineBridge.GetInstance().CallHandler(command);
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
                if (wrapper.code == XDGUserStatusCode.LOGOUT){
                    TDSUser.Logout();
                }
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
                    XDGTool.LogError($"LoginByType 登录失败1：code:{result.code}  msg:{result.message}");
                    errorCallback(new XDGError(result.code, result.message));
                    return;
                }

                XDGUserWrapper wrapper = new XDGUserWrapper(result.content);
                if (wrapper.error != null)
                {
                    XDGTool.LogError($"LoginByType 登录失败2：code:{wrapper.error.code}  msg:{wrapper.error.error_msg}");
                    errorCallback(wrapper.error);
                    return;
                }
                ActiveLearnCloudToken(wrapper.user, callback, errorCallback);
            });
        }

        public void AccountCancellation(){ //iOS有，安卓没有
#if UNITY_IOS
            var command = new Command(XDG_ACCOUNT_SERVICE, "accountCancellation", false, null);
            EngineBridge.GetInstance().CallHandler(command); 
#endif
        }
        
    }
}