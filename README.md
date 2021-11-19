# SignalR-Chat
A real-time chat application using .NET 5, SignalR and KnockoutJS. Available for Web and Desktop

![](https://raw.githubusercontent.com/AKouki/SignalR-Chat/main/Chat.Web/wwwroot/images/screenshots/mockup1.png)
![](https://raw.githubusercontent.com/AKouki/SignalR-Chat/main/Chat.Web/wwwroot/images/screenshots/desktop.png)

## Features
* Group chat
* Private chat `/private(Name) Hello, how are you?`
* Photo message
* Basic Emojis
* Chat Rooms

## Getting Started
In order to run Desktop application you need first to run Chat.Web project which is the Chat Service

1. Grab the Project
2. Open Visual Studio as Administrator and load the Solution
3. Resolve any missing/required nuget package

### For Chat.Web
1. Build Database. Open `Package Manager Console` and run the following commands: <br />
`update-database` <br />
2. That's all... Run the Project!

### For Chat.Desktop
1. Run the desktop application: `Right-Click -> Debug -> Start new instance`
