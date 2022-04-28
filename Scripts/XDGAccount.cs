﻿using System;
using XD.Intl.Common;

namespace XD.Intl.Account
{
    public class XDGAccount
    {
        public static void Login(Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            XDGAccountImpl.GetInstance().Login((u) => {
                callback(u);
                EventManager.LoginSuccessEvent();
            }, (e) => {
                errorCallback(e);
                EventManager.LoginFailEvent(e.error_msg);
            });
        }
        
        public static void LoginByType(LoginType loginType, Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            XDGAccountImpl.GetInstance().LoginByType(loginType, (u) => {
                callback(u);
                EventManager.LoginSuccessEvent();
            }, (e) => {
                errorCallback(e);
                EventManager.LoginFailEvent(e.error_msg);
            });
        }


        public static void Logout()
        {
            XDGAccountImpl.GetInstance().Logout();
        }

        public static void AddUserStatusChangeCallback(Action<int, string> callback)
        {
            XDGAccountImpl.GetInstance().AddUserStatusChangeCallback(callback);
        }

        public static void GetUser(Action<XDGUser> callback, Action<XDGError> errorCallback)
        {
            XDGAccountImpl.GetInstance().GetUser(callback, errorCallback);
        }

        public static void OpenUserCenter()
        {
            XDGAccountImpl.GetInstance().OpenUserCenter();
        }
        
        public static void AccountCancellation(){
            XDGAccountImpl.GetInstance().AccountCancellation();
        }

    }
}