$(document).ready(function () {
    let currentStep = 1;
    let isAvailabilityChecked = false;
    let isAvailable = false;

    // Adım 1 -> Adım 2 geçişi
    $('#nextStep1').click(function () {
        // Form doğrulama
        const requiredFields = $('#step1 select[required], #step1 input[required]');
        let isValid = true;

        requiredFields.each(function () {
            if (!$(this).val()) {
                isValid = false;
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        if (!isValid) {
            alert('Lütfen tüm gerekli alanları doldurun.');
            return;
        }

        // Seçilen bilgileri göster
        const antrenorText = $('#antrenorSelect option:selected').text();
        const hizmetText = $('#hizmetSelect option:selected').text();
        const sure = $('#hizmetSelect option:selected').data('sure');

        $('#selectedAntrenor').text(antrenorText);
        $('#selectedHizmet').text(hizmetText);
        $('#selectedSure').text(sure || '60');

        // Adım değiştir
        $('#step1').hide();
        $('#step2').show();
        $('#stepIndicator').text('2. Adım');
        currentStep = 2;
    });

    // Adım 2 -> Adım 1 geri dönüş
    $('#prevStep2').click(function () {
        $('#step2').hide();
        $('#step1').show();
        $('#stepIndicator').text('1. Adım');
        currentStep = 1;
    });

    // Tarih veya saat değiştiğinde
    $('#dateSelect, #timeSelect').change(function () {
        const tarih = $('#dateSelect').val();
        const saat = $('#timeSelect').val();

        if (tarih && saat) {
            const tarihObj = new Date(tarih + 'T' + saat);
            const formattedDate = tarihObj.toLocaleDateString('tr-TR', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            $('#selectedDateTimeText').text(formattedDate);
            $('#selectedDateTimeInput').val(tarih + 'T' + saat);

            // Müsaitlik kontrolü butonunu aktif et
            $('#musaitlikKontrol').prop('disabled', false);
        } else {
            $('#selectedDateTimeText').text('-');
            $('#selectedDateTimeInput').val('');
            $('#musaitlikKontrol').prop('disabled', true);
        }
    });

    // Müsaitlik kontrolü
    $('#musaitlikKontrol').click(function () {
        const tarih = $('#dateSelect').val();
        const saat = $('#timeSelect').val();

        if (!tarih || !saat) {
            alert('Lütfen önce tarih ve saat seçin.');
            return;
        }

        const antrenorId = $('#antrenorSelect').val();
        const hizmetSelect = $('#hizmetSelect');
        const sureDakika = hizmetSelect.find('option:selected').data('sure') || 60;
        const dateTime = tarih + 'T' + saat;

        // Yükleniyor göster
        const sonucDiv = $('#musaitlikSonucu');
        sonucDiv.removeClass('d-none alert-success alert-danger alert-warning');
        sonucDiv.addClass('alert-warning');
        sonucDiv.html('<i class="fas fa-spinner fa-spin"></i> Kontrol ediliyor...');

        // API çağrısı
        $.get('/Randevu/CheckAvailability', {
            antrenorId: antrenorId,
            randevuTarihi: dateTime,
            sureDakika: sureDakika
        }, function (response) {
            sonucDiv.removeClass('alert-warning');
            isAvailabilityChecked = true;

            if (response.Musait) {
                sonucDiv.addClass('alert-success');
                sonucDiv.html('<i class="fas fa-check-circle"></i> Antrenör müsait. Randevu oluşturabilirsiniz.');
                isAvailable = true;
            } else {
                sonucDiv.addClass('alert-danger');
                sonucDiv.html('<i class="fas fa-exclamation-circle"></i> Antrenör müsait değil.');
                isAvailable = false;
            }
        }).fail(function () {
            sonucDiv.removeClass('alert-warning');
            sonucDiv.addClass('alert-secondary');
            sonucDiv.html('<i class="fas fa-info-circle"></i> Kontrol yapılamadı. Devam edebilirsiniz.');
            isAvailabilityChecked = false;
            isAvailable = true;
        });
    });

    // DEVAM ET butonu - HER ZAMAN AKTİF
    $('#devamEtBtn').click(function () {
        const tarih = $('#dateSelect').val();
        const saat = $('#timeSelect').val();

        if (!tarih || !saat) {
            alert('Lütfen bir tarih ve saat seçin.');
            return;
        }

        if (isAvailabilityChecked && !isAvailable) {
            if (!confirm('Antrenör müsait değil. Yine de devam etmek istiyor musunuz?')) {
                return;
            }
        }

        // Devam Et butonunu gizle, Kaydet butonunu göster
        $('#devamEtBtn').hide();
        $('#prevStep2').hide();
        $('#submitSection').show();
    });
});