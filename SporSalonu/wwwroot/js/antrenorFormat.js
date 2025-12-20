$(document).ready(function () {
    // TC Kimlik No formatı - sadece rakam
    $('#TCKimlikNo').on('input', function () {
        $(this).val($(this).val().replace(/\D/g, ''));
    });

    // Saatlik ücret formatı - sadece rakam ve nokta
    $('#SaatlikUcret').on('input', function () {
        $(this).val($(this).val().replace(/[^\d.]/g, ''));
    });
});