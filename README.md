﻿# BiliSilverGet
b站直播的瓜子搜刮机

V2.1 测试版<br/>
Project 2016 填坑计划
### 目前的代码填坑走向
> 继续的debug... & 优化GDI的输出渲染缓存算法

### 编译&运行
1. 点击Clone或者下载zip获取代码
2. 使用Visual Studio 2015编译（其实应该没什么所谓，只要支持.net 4.0就好了，之前用vs2013和vs2012都编译过）
3. 选择 `x86平台` ，生成目标程序，运行就好了
5. 勾选自己需要的功能就ok了，如果不管用的话可以尝试再次勾选

### 说明
- b站登录接口是自己抓包的，emmmm，因为登陆的要求换了，有空再补登陆的文档
- 有bug可以汇报，当然也希望有人能够帮忙修一修bug以及整理一下乱成麻花的程序
- 以后或许还会更新b站直播的活动，不过要看个人时间允不允许了

### 更新
v2.1
- 更换登陆接口，支持GeeTest自动验证（感谢某知乎提供的详细抓包说明），是时候大喊一声
- “这进度条我不屑去拉！”
- 代码纯C#了。。。把之前的坑爹依赖项VBUtil给去掉

v2.0
- 把窗体重写了一遍（虽然并没有什么卵用）

v1.7
- 之前使用的手机版瓜子搜刮api被提示 `api sign invalid(code=-3)` ，再次换成电脑版接口+OCR验证码识别，初步测试运行良好
- 更换弹幕发送的域名

v1.6
- 支持竞猜数据实时更新
- 修改b站活动：红叶祭
- 推出第一个release，虽然还是alpha版本

v1.5
- bug修复
- 支持更多的直播间消息种类

v1.4
- 可以发送弹幕了……
- 也可以看用户信息了……

v1.3
- UI再改版
- 道具可以自己送了，有时候无脑就是干的感觉可真糟糕
- 支持限时活动：团扇get da★ze！ 屠龙宝刀，点击就送！
- 支持up领取的特定房间号id，兼容原cid
- 把挂机领经验的功能干出来了……
- 增加了icon……决定就是你了！银瓜子！

v1.2
- 不知道有没有修复过bug反正就先更新一发（笑）

v1.1
- 支持直播录播功能
- 新增直播弹幕查看 
- UI改版
- 多线程代码优化

### License
GNU GPLv3

<p align="right">
Project 2016<br/>
Pandasxd
</p>
