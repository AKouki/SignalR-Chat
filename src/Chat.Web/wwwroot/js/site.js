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

    $("#expand-sidebar").click(function () {
        $(".sidebar").toggleClass("open");
        $(".users-container").removeClass("open");
    });

    $("#expand-users-list").click(function () {
        $(".users-container").toggleClass("open");
        $(".sidebar").removeClass("open");
    });

    $(document).on("click", ".sidebar.open ul li a, #users-list li", function () {
        $(".sidebar, .users-container").removeClass("open");
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

    $("#remove-message-modal").on("shown.bs.modal", function (e) {
        const id = e.relatedTarget.getAttribute('data-messageId');
        $("#itemToDelete").val(id);
    });

    $(document).on("mouseenter", ".ismine", function () {
        $(this).find(".actions").removeClass("d-none");
    });

    $(document).on("mouseleave", ".ismine", function () {
        var isDropdownOpen = $(this).find(".dropdown-menu.show").length > 0;
        if (!isDropdownOpen)
            $(this).find(".actions").addClass("d-none");
    });

    $(document).on("hidden.bs.dropdown", ".actions .dropdown", function () {
        $(this).closest(".actions").addClass("d-none");
    });
});