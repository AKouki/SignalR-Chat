$(function () {
    $('ul#users-list').on('click', 'li', function () {
        var username = $("input[type=hidden].username", $(this)).val();
        var input = $('#chat-message');

        var text = input.val();
        if (text.startsWith("/")) {
            text = text.split(")")[1];
        }

        text = "/private(" + username + ") " + text.trim();
        input.val(text);
        input.change();
        input.focus();
    });

    $('#emojis-container').on('click', 'a', function () {
        var value = $("input", $(this)).val();
        var input = $('#chat-message');
        input.val(input.val() + value);
        input.focus();
        input.change();
    });

    $("#emojibtn").click(function () {
        $("#emojis-container").toggleClass("d-none");
    });

    $("#chat-message, #btn-send-message").click(function () {
        $("#emojis-container").addClass("d-none")
    });

    $('.modal').on('hidden.bs.modal', function () {
        $(".modal-body input:not(#newRoomName)").val("");
    });

    $(".alert .close").on('click', function () {
        $(this).parent().hide();
    });
});