$(document).ready(function () {
    // Müsaitlik kontrolü
    $('#musaitlikKontrol').click(function () {
        var antrenorId = $('#antrenorSelect').val();
        var randevuTarihi = $('#randevuTarihi').val();
        var hizmetSelect = $('#hizmetSelect');
        var hizmetId = hizmetSelect.val();

        if (!antrenorId || !randevuTarihi || !hizmetId) {
            alert('Lütfen antrenör, hizmet ve randevu tarihi seçiniz.');
            return;
        }

        // Hizmet süresini al
        var selectedOption = hizmetSelect.find('option:selected');
        var sureText = selectedOption.data('sure');
        var sureDakika = sureText ? parseInt(sureText) : 60;

        // Yükleniyor göster
        var sonucDiv = $('#musaitlikSonucu');
        sonucDiv.removeClass('d-none alert-success alert-danger');
        sonucDiv.addClass('alert-warning');
        sonucDiv.html('<i class="fas fa-spinner fa-spin"></i> Kontrol ediliyor...');

        // API'yi çağır
        $.get('/UyeRandevu/CheckAvailability', {
            antrenorId: antrenorId,
            randevuTarihi: randevuTarihi,
            sureDakika: sureDakika
        }, function (response) {
            sonucDiv.removeClass('alert-warning');

            if (response.Musait) {
                sonucDiv.addClass('alert-success');
                sonucDiv.html('<i class="fas fa-check-circle"></i> Seçtiğiniz tarihte antrenör müsait. Randevu güncelleyebilirsiniz.');
            } else {
                sonucDiv.addClass('alert-danger');
                var html = '<i class="fas fa-exclamation-circle"></i> Seçtiğiniz tarihte antrenör müsait değil.';

                if (response.CakisanRandevular && response.CakisanRandevular.length > 0) {
                    html += ' Çakışan randevular:<ul>';

                    $.each(response.CakisanRandevular, function (index, randevu) {
                        html += '<li>' + randevu.Hizmet + ' (' +
                            randevu.Baslangic + ' - ' + randevu.Bitis + ')</li>';
                    });

                    html += '</ul>';
                }

                sonucDiv.html(html);
            }
        }).fail(function () {
            sonucDiv.removeClass('alert-warning');
            sonucDiv.addClass('alert-secondary');
            sonucDiv.html('<i class="fas fa-info-circle"></i> Kontrol yapılamadı. Lütfen tekrar deneyin.');
        });
    });
});