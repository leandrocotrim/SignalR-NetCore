// var connection = new signalR.HubConnectionBuilder().withUrl("/api/v1/logs/streaming").build();
var connection = new signalR.HubConnectionBuilder().withUrl("streaming").build();


connection.on("ReceiveMessage", function (message) {
    var msg = message.replace(/&/g, "&").replace(/</g, "<").replace(/>/g, ">");
    var li = document.createElement("li");
    li.textContent = msg;
    document.getElementsByTagName("ul")[0].appendChild(li);
});

connection.start().then(function () {
    var li = document.createElement("li");
    li.textContent = "Connetado!";
    document.getElementsByTagName("ul")[0].appendChild(li);
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});