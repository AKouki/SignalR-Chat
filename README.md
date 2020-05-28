# SignalR-Chat
A real-time chat application using ASP.NET SignalR and KnockoutJS. Available for Web, Desktop and Mobile (Android)

Find mobile version here: [Chat.Mobile](https://github.com/AKouki/SignalR-Chat.Mobile)

![](https://raw.githubusercontent.com/AKouki/SignalR-Chat/master/Chat.Web/Content/screenshots/mockup1.png)
![](https://raw.githubusercontent.com/AKouki/SignalR-Chat/master/Chat.Web/Content/screenshots/mockup2.png)

## Features
* Group chat
* Private chat `/private(Name) Hello, how are you?`
* Photo message
* Basic Emojis
* Chat Rooms

## Getting Started
In order to run Desktop or Mobile application you need first to run Chat.Web project which is the Chat Service.

1. Grab the Project
2. Open Visual Studio as Administrator and load the Solution
3. Resolve any missing/required nuget package

### For Chat.Web
1. Build Database. Open `Package Manager Console` and run the following commands: <br />
`update-database` <br />
2. That's all... Run the Project!

### For Chat.Desktop
1. Open `ChatHubManager.cs`, `LoginWindow.xaml.cs` and change the address from `localhost:2325` to address where `Chat.Web` is running
2. You are ready! run the desktop application: `Right-Click -> Debug -> Start new instance`
