$(function () {

    $('ul#user-list').on('click', 'li', function () {
        var username = $("input[type=hidden].username", $(this)).val();
        var input = $('#chat-message');

        var text = input.val();
        if (text.startsWith("/")) {
            text = text.split(")")[1];
        }

        text = "/private(" + username + ") " + text;
        input.val(text);
        input.focus();
    });

    $('#emojis-container').on('click', 'a', function () {
        var value = $("input", $(this)).val();
        var input = $('#chat-message');
        input.val(input.val() + value);
        input.focus();
        input.change();
    });

    // Show/Hide Emoji Window
    $("#emojibtn").click(function () {
        var x = $("#emojis-container");
        if (x.hasClass("hidden")) {
            x.removeClass("hidden");
        }
        else {
            x.addClass("hidden");
        }
    });

    $("#chat-message, #btn-send-message").click(function () {
        $("#emojis-container").addClass("hidden")
    });

    $('.modal').on('hidden.bs.modal', function () {
        $("input").val("");
    });
});