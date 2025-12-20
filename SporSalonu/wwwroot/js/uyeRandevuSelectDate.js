$(document).ready(function () {
    // Available dates data from ViewBag
    var availableDates = window.availableDates || [];
    var antrenorId = window.antrenorId || 0;
    var sureDakika = window.sureDakika || 60;
    var hizmetId = window.hizmetId || 0;

    var selectedDate = null;
    var selectedTime = null;

    // Tarih seçildiğinde saatleri yükle
    $('#dateSelect').change(function () {
        var selectedDateStr = $(this).val();
        var timeSelect = $('#timeSelect');

        if (!selectedDateStr) {
            timeSelect.prop('disabled', true);
            timeSelect.html('<option value="">Önce tarih seçin</option>');
            $('#continueButton').prop('disabled', true);
            updateSelectedDateTime();
            return;
        }

        // Seçilen tarihi bul
        selectedDate = availableDates.find(function (d) {
            return d.date === selectedDateStr;
        });

        if (selectedDate && selectedDate.availableSlots && selectedDate.availableSlots.length > 0) {
            timeSelect.prop('disabled', false);
            timeSelect.html('<option value="">Saat seçin</option>');

            // Saatleri doldur
            selectedDate.availableSlots.forEach(function (slot) {
                var startTime = formatTime(slot.startTime);
                var endTime = formatTime(slot.endTime);
                timeSelect.append('<option value="' + slot.startTime + '">' + startTime + ' - ' + endTime + '</option>');
            });
        } else {
            timeSelect.prop('disabled', true);
            timeSelect.html('<option value="">Bu tarihte müsait saat yok</option>');
        }

        $('#continueButton').prop('disabled', true);
        updateSelectedDateTime();
    });

    // Saat seçildiğinde
    $('#timeSelect').change(function () {
        selectedTime = $(this).val();
        if (selectedDate && selectedTime) {
            $('#continueButton').prop('disabled', false);
        }
        updateSelectedDateTime();
    });

    // Seçilen tarih/saati güncelle
    function updateSelectedDateTime() {
        if (selectedDate && selectedTime) {
            var dateTime = new Date(selectedDate.date + 'T' + selectedTime);
            var formattedDate = dateTime.toLocaleDateString('tr-TR', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            $('#selectedDateTimeText').text(formattedDate);
            $('#formDateTime').val(dateTime.toISOString());
        } else {
            $('#selectedDateTimeText').text('Henüz tarih/saat seçmediniz');
        }
    }

    // Saat formatını düzenle
    function formatTime(timeString) {
        var time = new Date('2000-01-01T' + timeString);
        return time.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    }

    // Müsaitlik kontrolü
    $('#checkAvailability').click(function () {
        if (!selectedDate || !selectedTime) {
            alert('Lütfen önce bir tarih ve saat seçin.');
            return;
        }

        var dateTime = selectedDate.date + 'T' + selectedTime + ':00';

        // Yükleniyor göster
        var resultDiv = $('#availabilityResult');
        resultDiv.removeClass('d-none alert-success alert-danger alert-warning');
        resultDiv.addClass('alert-warning');
        resultDiv.html('<i class="fas fa-spinner fa-spin"></i> Müsaitlik kontrol ediliyor...');

        // API çağrısı
        $.get('/UyeRandevu/CheckAvailability', {
            antrenorId: antrenorId,
            randevuTarihi: dateTime,
            sureDakika: sureDakika
        }, function (response) {
            resultDiv.removeClass('alert-warning');

            if (response.Musait) {
                resultDiv.addClass('alert-success');
                resultDiv.html('<i class="fas fa-check-circle"></i> Antrenör müsait! Randevu oluşturabilirsiniz.');
            } else {
                resultDiv.addClass('alert-danger');
                var html = '<i class="fas fa-exclamation-circle"></i> Antrenör müsait değil. ';

                if (response.CakisanRandevular && response.CakisanRandevular.length > 0) {
                    html += 'Çakışan randevu: ' + response.CakisanRandevular[0].Hizmet +
                        ' (' + response.CakisanRandevular[0].Baslangic + ' - ' +
                        response.CakisanRandevular[0].Bitis + ')';
                }

                resultDiv.html(html);
            }
        }).fail(function () {
            resultDiv.removeClass('alert-warning');
            resultDiv.addClass('alert-danger');
            resultDiv.html('<i class="fas fa-times-circle"></i> Müsaitlik kontrolü sırasında hata oluştu.');
        });
    });

    // DEVAM ET butonu
    $('#continueButton').click(function () {
        if (!selectedDate || !selectedTime) {
            alert('Lütfen bir tarih ve saat seçin.');
            return;
        }

        // Not'u forma ekle
        $('#formNot').val($('#not').val());

        // Butonu yükleniyor durumuna getir
        $(this).html('<i class="fas fa-spinner fa-spin"></i> Gönderiliyor...');
        $(this).prop('disabled', true);

        // Formu submit et
        $('#appointmentForm').submit();
    });

    // Not değiştiğinde
    $('#not').on('input', function () {
        $('#formNot').val($(this).val());
    });
});