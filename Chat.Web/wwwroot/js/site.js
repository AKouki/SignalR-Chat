$(function () {
    $('ul#users-list').on('click', 'li', function () {
        var username = $(this).data("username");
        var input = $('#message-input');

        var text = input.val();
        if (text.startsWith("/")) {
            text = text.split(")")[1];
        }

        text = "/private(" + username + ") " + text.trim();
        input.val(text);
        input.change();
        input.focus();
    });

    $('#emojis-container').on('click', 'button', function () {
        var emojiValue = $(this).data("value");
        var input = $('#message-input');
        input.val(input.val() + emojiValue + " ");
        input.focus();
        input.change();
    });

    $("#btn-show-emojis").click(function () {
        $("#emojis-container").toggleClass("d-none");
    });

    $("#message-input, .messages-container, #btn-send-message, #emojis-container button").click(function () {
        $("#emojis-container").addClass("d-none");
    });

    $(".modal").on("shown.bs.modal", function () {
        $(this).find("input[type=text]:first-child").focus();
    });

    $('.modal').on('hidden.bs.modal', function () {
        $(".modal-body input:not(#newRoomName)").val("");
    });

    $(".alert .btn-close").on('click', function () {
        $(this).parent().hide();
    });

    $('body').tooltip({
        selector: '[data-bs-toggle="tooltip"]',
        delay: { show: 500 }
    });
});