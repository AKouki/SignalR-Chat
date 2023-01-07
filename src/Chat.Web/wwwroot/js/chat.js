$(document).ready(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    connection.start().then(function () {
        console.log('SignalR Started...')
        viewModel.roomList();
        viewModel.userList();
    }).catch(function (err) {
        return console.error(err);
    });

    connection.on("newMessage", function (messageView) {
        var isMine = messageView.fromUserName === viewModel.myProfile().userName();
        var message = new ChatMessage(messageView.id, messageView.content, messageView.timestamp, messageView.fromUserName, messageView.fromFullName, isMine, messageView.avatar);
        viewModel.chatMessages.push(message);
        $(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 1000);
    });

    connection.on("getProfileInfo", function (user) {
        viewModel.myProfile(new ProfileInfo(user.userName, user.fullName, user.avatar));
        viewModel.isLoading(false);
    });

    connection.on("addUser", function (user) {
        viewModel.userAdded(new ChatUser(user.userName, user.fullName, user.avatar, user.currentRoom, user.device));
    });

    connection.on("removeUser", function (user) {
        viewModel.userRemoved(user.userName);
    });

    connection.on("addChatRoom", function (room) {
        viewModel.roomAdded(new ChatRoom(room.id, room.name, room.admin));
    });

    connection.on("updateChatRoom", function (room) {
        viewModel.roomUpdated(new ChatRoom(room.id, room.name, room.admin));
    });

    connection.on("removeChatRoom", function (id) {
        viewModel.roomDeleted(id);
    });

    connection.on("removeChatMessage", function (id) {
        viewModel.messageDeleted(id);
    });

    connection.on("onError", function (message) {
        viewModel.serverInfoMessage(message);
        $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);
    });

    connection.on("onRoomDeleted", function () {
        if (viewModel.chatRooms().length == 0) {
            viewModel.joinedRoom(null);
        }
        else {
            // Join to the first room from the list
            viewModel.joinRoom(viewModel.chatRooms()[0]);
        }
    });

    function AppViewModel() {
        var self = this;

        self.message = ko.observable("");
        self.chatRooms = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]);
        self.joinedRoom = ko.observable();
        self.serverInfoMessage = ko.observable("");
        self.myProfile = ko.observable();
        self.isLoading = ko.observable(true);

        self.showAvatar = ko.computed(function () {
            return self.isLoading() == false && self.myProfile().avatar() != null;
        });

        self.showRoomActions = ko.computed(function () {
            return self.joinedRoom()?.admin() == self.myProfile()?.userName();
        });

        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {
                self.sendNewMessage();
            }
            return true;
        }
        self.filter = ko.observable("");
        self.filteredChatUsers = ko.computed(function () {
            if (!self.filter()) {
                return self.chatUsers();
            } else {
                return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                    var fullName = user.fullName().toLowerCase();
                    return fullName.includes(self.filter().toLowerCase());
                });
            }
        });

        self.sendNewMessage = function () {
            var text = self.message();
            if (text.startsWith("/")) {
                var receiver = text.substring(text.indexOf("(") + 1, text.indexOf(")"));
                var message = text.substring(text.indexOf(")") + 1, text.length);
                self.sendPrivate(receiver, message);
            }
            else {
                self.sendToRoom(self.joinedRoom(), self.message());
            }

            self.message("");
        }

        self.sendToRoom = function (room, message) {
            if (room.name()?.length > 0 && message.length > 0) {
                fetch('/api/Messages', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ room: room.name(), content: message })
                });
            }
        }

        self.sendPrivate = function (receiver, message) {
            if (receiver.length > 0 && message.length > 0) {
                connection.invoke("SendPrivate", receiver.trim(), message.trim());
            }
        }

        self.joinRoom = function (room) {
            connection.invoke("Join", room.name()).then(function () {
                self.joinedRoom(room);
                self.userList();
                self.messageHistory();
            });
        }

        self.roomList = function () {
            fetch('/api/Rooms')
                .then(response => response.json())
                .then(data => {
                    self.chatRooms.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        self.chatRooms.push(new ChatRoom(data[i].id, data[i].name, data[i].admin));
                    }

                    if (self.chatRooms().length > 0)
                        self.joinRoom(self.chatRooms()[0]);
                });
        }

        self.userList = function () {
            connection.invoke("GetUsers", self.joinedRoom()?.name()).then(function (result) {
                self.chatUsers.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatUsers.push(new ChatUser(result[i].userName,
                        result[i].fullName,
                        result[i].avatar,
                        result[i].currentRoom,
                        result[i].device))
                }
            });
        }

        self.createRoom = function () {
            var roomName = $("#roomName").val();
            fetch('/api/Rooms', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: roomName })
            });
        }

        self.editRoom = function () {
            var roomId = self.joinedRoom().id();
            var roomName = $("#newRoomName").val();
            fetch('/api/Rooms/' + roomId, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: roomId, name: roomName })
            });
        }

        self.deleteRoom = function () {
            fetch('/api/Rooms/' + self.joinedRoom().id(), {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: self.joinedRoom().id() })
            });
        }

        self.deleteMessage = function () {
            var messageId = $("#itemToDelete").val();

            fetch('/api/Messages/' + messageId, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: messageId })
            });
        }

        self.messageHistory = function () {
            fetch('/api/Messages/Room/' + viewModel.joinedRoom().name())
                .then(response => response.json())
                .then(data => {
                    self.chatMessages.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        var isMine = data[i].fromUserName == self.myProfile().userName();
                        self.chatMessages.push(new ChatMessage(data[i].id,
                            data[i].content,
                            data[i].timestamp,
                            data[i].fromUserName,
                            data[i].fromFullName,
                            isMine,
                            data[i].avatar))
                    }

                    $(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 1000);
                });
        }

        self.roomAdded = function (room) {
            self.chatRooms.push(room);
        }

        self.roomUpdated = function (updatedRoom) {
            var room = ko.utils.arrayFirst(self.chatRooms(), function (item) {
                return updatedRoom.id() == item.id();
            });

            room.name(updatedRoom.name());

            if (self.joinedRoom().id() == room.id()) {
                self.joinRoom(room);
            }
        }

        self.roomDeleted = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatRooms(), function (room) {
                if (room.id() == id)
                    temp = room;
            });
            self.chatRooms.remove(temp);
        }

        self.messageDeleted = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatMessages(), function (message) {
                if (message.id() == id)
                    temp = message;
            });
            self.chatMessages.remove(temp);
        }

        self.userAdded = function (user) {
            self.chatUsers.push(user);
        }

        self.userRemoved = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatUsers(), function (user) {
                if (user.userName() == id)
                    temp = user;
            });
            self.chatUsers.remove(temp);
        }

        self.uploadFiles = function () {
            var form = document.getElementById("uploadForm");
            $.ajax({
                type: "POST",
                url: '/api/Upload',
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function () {
                    $("#UploadedFile").val("");
                },
                error: function (error) {
                    alert('Error: ' + error.responseText);
                }
            });
        }
    }

    function ChatRoom(id, name, admin) {
        var self = this;
        self.id = ko.observable(id);
        self.name = ko.observable(name);
        self.admin = ko.observable(admin);
    }

    function ChatUser(userName, fullName, avatar, currentRoom, device) {
        var self = this;
        self.userName = ko.observable(userName);
        self.fullName = ko.observable(fullName);
        self.avatar = ko.observable(avatar);
        self.currentRoom = ko.observable(currentRoom);
        self.device = ko.observable(device);
    }

    function ChatMessage(id, content, timestamp, fromUserName, fromFullName, isMine, avatar) {
        var self = this;
        self.id = ko.observable(id);
        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.timestampRelative = ko.pureComputed(function () {
            // Get diff
            var date = new Date(self.timestamp());
            var now = new Date();
            var diff = Math.round((date.getTime() - now.getTime()) / (1000 * 3600 * 24));

            // Format date
            var { dateOnly, timeOnly } = formatDate(date);

            // Generate relative datetime
            if (diff == 0)
                return `Today, ${timeOnly}`;
            if (diff == -1)
                return `Yestrday, ${timeOnly}`;

            return `${dateOnly}`;
        });
        self.timestampFull = ko.pureComputed(function () {
            var date = new Date(self.timestamp());
            var { fullDateTime } = formatDate(date);
            return fullDateTime;
        });
        self.fromUserName = ko.observable(fromUserName);
        self.fromFullName = ko.observable(fromFullName);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
    }

    function ProfileInfo(userName, fullName, avatar) {
        var self = this;
        self.userName = ko.observable(userName);
        self.fullName = ko.observable(fullName);
        self.avatar = ko.observable(avatar);
    }

    function formatDate(date) {
        // Get fields
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        var day = date.getDate();
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var d = hours >= 12 ? "PM" : "AM";

        // Correction
        if (hours > 12)
            hours = hours % 12;

        if (minutes < 10)
            minutes = "0" + minutes;

        // Result
        var dateOnly = `${day}/${month}/${year}`;
        var timeOnly = `${hours}:${minutes} ${d}`;
        var fullDateTime = `${dateOnly} ${timeOnly}`;

        return { dateOnly, timeOnly, fullDateTime };
    }

    var viewModel = new AppViewModel();
    ko.applyBindings(viewModel);
});
