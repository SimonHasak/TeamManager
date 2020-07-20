// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
getInputMessage = () => {
    return document.getElementById("myInput").value;
}

contains = (textOne, textTwo) => {
    return textOne.indexOf(textTwo) != -1;
}

showChoosedNames = () => {
    var searchText = getInputMessage().toLowerCase();
    $('.searchTable').each(function () {
        var rowText = $(this).children('#Name').text().toLowerCase();
        if (contains(rowText, searchText)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})