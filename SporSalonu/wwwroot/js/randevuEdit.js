$(document).ready(function () {
    // Hizmet değiştiğinde bitiş tarihini hesapla
    $('#hizmetSelect').change(function () {
        calculateEndTime();
    });

    $('#randevuTarihi').change(function () {
        calculateEndTime();
    });

    function calculateEndTime() {
        var randevuTarihi = $('#randevuTarihi').val();
        var hizmetSelect = $('#hizmetSelect');
        var selectedOption = hizmetSelect.find('option:selected');
        var sureText = selectedOption.data('sure');

        if (randevuTarihi && sureText) {
            var sureDakika = parseInt(sureText);
            var tarih = new Date(randevuTarihi);
            tarih.setMinutes(tarih.getMinutes() + sureDakika);

            // Bitiş tarihini formata uygun hale getir
            var bitisTarihi = tarih.toISOString().slice(0, 16);
            $('#bitisTarihi').val(bitisTarihi);
        }
    }

    // Sayfa yüklendiğinde bitiş tarihini hesapla
    calculateEndTime();
});