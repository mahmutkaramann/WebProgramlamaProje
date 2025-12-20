// String truncate extension
String.prototype.Truncate = function (maxLength) {
    if (this.length <= maxLength) return this;
    return this.substring(0, maxLength) + '...';
};

$(document).ready(function () {
    // Checkbox seçildiğinde kartı vurgula
    $('input[name="hizmetIds"]').change(function () {
        const $card = $(this).closest('.card');
        if ($(this).is(':checked')) {
            $card.addClass('border-primary').removeClass('border-secondary');
        } else {
            $card.removeClass('border-primary').addClass('border-secondary');
        }
    });

    // İlk yüklemede kartları vurgula
    $('input[name="hizmetIds"]:checked').each(function () {
        $(this).closest('.card').addClass('border-primary').removeClass('border-secondary');
    });
});