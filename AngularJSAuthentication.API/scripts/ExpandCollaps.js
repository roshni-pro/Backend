$(function () {
    $('body').on('click', '.CX span', function () {
        //When Click On + sign
        if ($(this).text() == '+') {
            $(this).text('-');
        }
        else {
            $(this).text('+');
        }
        $(this).closest('tr') // row of + sign
            .next('tr') // next row of + sign
            .toggle(); // if show then hide else show

    });
});