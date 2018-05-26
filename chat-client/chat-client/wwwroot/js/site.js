// Write your JavaScript code.

var currentChatId = -1;

function selectChat(newChatId) {
    currentChatId = newChatId;
    refreshUserLayout();
    refreshMessageLayout();
}

function refreshUserLayout() {
    $.get(serverAdresss + "/chat/UserLayout/").done(function(data) {
        document.getElementById('userListExternalContainer').innerHTML = data;
    });
}

function refreshMessageLayout() {
    Id = currentChatId;
    $.get(serverAdresss + "/chat/MessageLayout/" + Id + "/cookie=" + document.cookie.substring(6)).done(function(data) {
        document.getElementById('messageListExternalContainer').innerHTML = data;
        try{ScrollDownMessages();} catch(err) {};
    });
}

function refreshMessageLayoutSoftly() {
    Id = currentChatId;
    $.get(serverAdresss + "/chat/MessageLayout/" + Id + "/cookie=" + document.cookie.substring(6)).done(function(data) {
        Content = document.getElementById('messageListExternalContainer').innerHTML;
        Data = data;        
        shortedContent = Content.replace(/\W/g, '');
        shortedData = Data.replace(/\W/g, '');
        if (shortedContent != shortedData) {
            document.getElementById('messageListExternalContainer').innerHTML = data;
            try{ScrollDownMessages();} catch (err) {};
        }
    });
}


function setRefreshInterval(interval = 2) {
    refreshUserLayout();
    refreshMessageLayout();
    window.setInterval(function(){
        refreshUserLayout();
        refreshMessageLayoutSoftly();
    }, interval * 1000);
}

function ScrollDownMessages() {
    var objDiv = document.getElementById("messageListContainer");
    objDiv.scrollTop = objDiv.scrollHeight;
}

function sendMessage() {
    if (currentChatId == -1) {
        alert("Select chat");
        return;
    }
    to_id = currentChatId;
    messageToSend = document.getElementById("messageInput").value;
    $.get(serverAdresss + "/api/send_message/for_id=" + to_id + "/message=" + messageToSend + "/cookie=" + document.cookie.substring(6)).done(function(data) {
        var response = data;
        if (response == -1) {
            alert("Error sending message");
            return;
        }
        refreshMessageLayout(Id);
    });
    document.getElementById("messageInput").value = "";
}