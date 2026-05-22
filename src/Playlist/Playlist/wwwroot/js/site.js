// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    $(".js-datetime").on("blur", function () {
        const value = $(this).val().trim();

        if (value.length === 0) {
            $(this).addClass("input-error");
            return;
        }

        const pattern = /^\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}$/;

        if (!pattern.test(value)) {
            $(this).addClass("input-error");
        } else {
            $(this).removeClass("input-error");
        }
    });
});