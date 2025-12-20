$(document).ready(function () {
    // Gün ve saat değiştiğinde önizleme göster
    function updatePreview() {
        var gunSelect = $('#gunSelect');
        var baslangic = $('#baslangicSaati').val();
        var bitis = $('#bitisSaati').val();
        var seciliGunText = gunSelect.find('option:selected').text();

        if (seciliGunText && baslangic && bitis) {
            $('#saatOnizleme').removeClass('d-none');
            $('#seciliGun').text(seciliGunText + ':');
            $('#seciliSaat').text(baslangic + ' - ' + bitis);
        } else {
            $('#saatOnizleme').addClass('d-none');
        }
    }

    // Değişiklikleri dinle
    $('#gunSelect, #baslangicSaati, #bitisSaati').change(updatePreview);

    // Bitiş saatinin başlangıçtan sonra olmasını kontrol et
    $('#baslangicSaati, #bitisSaati').change(function () {
        var baslangic = $('#baslangicSaati').val();
        var bitis = $('#bitisSaati').val();

        if (baslangic && bitis && baslangic >= bitis) {
            alert('Bitiş saati başlangıç saatinden sonra olmalıdır.');
            $('#bitisSaati').val('');
        }
    });

    // Örnek saatleri hızlı ekleme
    $('.card.bg-light').click(function () {
        var saatText = $(this).find('.fw-bold').text();
        var saatler = saatText.split(' - ');

        if (saatler.length === 2) {
            $('#baslangicSaati').val(saatler[0]);
            $('#bitisSaati').val(saatler[1]);
            updatePreview();
        }
    });
});