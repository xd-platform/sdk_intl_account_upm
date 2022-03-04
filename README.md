# XD-Intl
## 1.在Packages/manifest.json中加入如下引用
```
   "com.leancloud.realtime": "https://github.com/leancloud/csharp-sdk-upm.git#realtime-0.10.2",
    "com.leancloud.storage": "https://github.com/leancloud/csharp-sdk-upm.git#storage-0.10.2",
    "com.taptap.tds.bootstrap": "https://github.com/TapTap/TapBootstrap-Unity.git#3.6.1",
    "com.taptap.tds.common": "https://github.com/TapTap/TapCommon-Unity.git#3.6.1",
    "com.taptap.tds.login": "https://github.com/TapTap/TapLogin-Unity.git#3.6.1",
    "com.taptap.tds.tapdb": "https://github.com/TapTap/TapDB-Unity.git#3.6.1",
    "com.xd.intl.common": "https://github.com/xd-platform/sdk_intl_common_upm.git#6.1.1",
    "com.xd.intl.account": "https://github.com/xd-platform/sdk_intl_account_upm.git#6.1.1",
    "com.xd.intl.payment": "https://github.com/xd-platform/sdk_intl_payment_upm.git#6.1.1",
    
    "scopedRegistries": [
    {
      "name": "XD Intl SDK",
      "url": "http://npm.xindong.com",
      "scopes": [
        "com.xd.intl"
      ]
    },
    {
      "name": "Game Package Registry by Google",
      "url": "https://unityregistry-pa.googleapis.com",
      "scopes": [
        "com.google"
      ]
    }
  ]
```
(依赖的仓库地址)
* [TapTap.Common](https://github.com/TapTap/TapCommon-Unity.git)
* [TapTap.Bootstrap](https://github.com/TapTap/TapBootstrap-Unity.git)
* [TapTap.Login](https://github.com/TapTap/TapLogin-Unity.git)
* [TapTap.TapDB](https://github.com/TapTap/TapDB-Unity.git)
* [LeanCloud](https://github.com/leancloud/csharp-sdk-upm)


## 2.配置SDK
#### iOS配置
* 将TDS-Info.plist 放在 /Assets/Plugins 中
* 将XDG-Info.plist 放在 /Assets/Plists/iOS 中
* 在Capabilities中打开In-App Purchase、Push Notifications、Sign In With Apple功能

#### Android配置
* 将XDG_info.json、google-Service.json 文件放在 /Assets/Plugins/Android/assets中

## 3.命名空间

```
using XD.Intl.Account;
using XD.Intl.Common;
```

## 4.接口使用
#### 切换语言
```
XDGCommon.SetLanguage(LangType.ZH_CN);
```

#### 初始化SDK
使用sdk前需先初始化
```
 XDGCommon.InitSDK((success => {
                if (success){
              
                }else{
                
                }
            }));
```

#### 登录
```
 XDGAccount.Login(user={
    
},(error)=>{
    
});
```

#### 第三方登录
```
 XDGAccount.LoginByType(LoginType.Google, user => {
              
              },error => {
                
             });
```

#### 绑定用户状态回调(绑定，解绑，退出)
```
 XDGAccount.AddUserStatusChangeCallback((code)={

});
```

#### 获取用户信息
```
 XDGAccount.GetUser((user)={
   
},(error)=>{
    
});
```

#### 打开用户中心
```
  XDGAccount.OpenUserCenter();
```

#### 退出登录
```
  XDGAccount.Logout();
```
