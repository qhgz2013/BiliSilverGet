# BiliSilverGet
b站直播的瓜子搜刮机

V1.3 测试版<br/>
Project 2016 填坑计划
### 目前的代码填坑走向
> 研究b站直播挂机get exp的方式
> 支持弹幕发送

### 编译&运行
1. 点击Clone或者下载zip获取代码
2. 使用Visual Studio 2015编译（其实应该没什么所谓，只要支持.net 4.0就好了，之前用vs2013和vs2012都编译过）
3. 生成目标程序，运行就好了
4. 按照提示登录，PS：验证码接口一直懒着没做，因为是用miniLogin的所以在一般情况下是免验证码的 `注意：该登陆算法为RSA加密，*非官方开放接口*，开发者不承担用户因账号丢失所损失的一切费用`
5. 勾选自己需要的功能就ok了，如果不管用的话可以尝试再次勾选
6. 注意：开机启动需要用到管理员权限，运行时请右键——以管理员身份运行，否则会抛出权限不足异常 `还有就是win10好奇怪，注册表开机启动不管用了？`

### 说明
- 本搜刮机使用的是手机版的API，*有`信仰`加成*，但是无法获得投票券
- appkey和secretkey是从[别处](https://github.com/cnbeining/bilibili-grab-silver/blob/master/autograb.py)搜刮到的
- b站登录接口是自己抓包的，想要了解更多，请见`登录说明`
- 有bug可以汇报，当然也希望有人能够帮忙修一修bug以及整理一下乱成麻花的程序
- 以后或许还会更新b站直播的活动，不过要看个人时间允不允许了

### 更新
v1.3
- UI再改版
- 道具可以自己送了，有时候无脑就是干的感觉可真糟糕
- 支持限时活动：团扇get da★ze！ 屠龙宝刀，点击就送！
- 支持up领取的特定房间号id，兼容原cid

v1.2
- 不知道有没有修复过bug反正就先更新一发（笑）

v1.1
- 支持直播录播功能
- 新增直播弹幕查看 
- UI改版
- 多线程代码优化

### License

**Doubi License**
<br/>
Version 1.0, January 2016

> Everyone is permitted to copy and distribute verbatim or modified copies of this license document.
And changing it is allowed.

**TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION**
<br/>
0. You can copy, modify and distribute all the code, but making a profit is not allowed.
<br/>
<p align="right">
Project 2016<br/>
Pandasxd
</p>
