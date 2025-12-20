$(document).ready(function () {
    let selectedAntrenorId = null;
    let selectedHizmetId = null;
    let selectedDate = null;
    let selectedTime = null;
    let selectedDateTime = null;
    let hizmetSure = 60;

    // Antrenör seçildiğinde
    $('#antrenorSelect').change(function () {
        selectedAntrenorId = $(this).val();
        console.log('Antrenör ID:', selectedAntrenorId);
        updateDateSelection();
    });

    // Hizmet seçildiğinde
    $('#hizmetSelect').change(function () {
        selectedHizmetId = $(this).val();
        console.log('Hizmet ID:', selectedHizmetId);

        // Hizmet süresini al
        var selectedOption = $(this).find('option:selected');
        var sureText = selectedOption.data('sure');
        hizmetSure = sureText ? parseInt(sureText) : 60;

        console.log('Hizmet süresi:', hizmetSure);

        // Hizmet bilgisini göster
        var hizmetText = selectedOption.data('name') || selectedOption.text();
        $('#selectedHizmetText').text(hizmetText);
        $('#selectedSureText').text(hizmetSure);

        updateDateSelection();
    });

    // Tarih seçildiğinde
    $('#dateSelect').change(function () {
        selectedDate = $(this).val();
        console.log('Seçilen tarih:', selectedDate);
        if (selectedDate && selectedAntrenorId && selectedHizmetId) {
            loadAvailableTimes();
        } else {
            $('#timeSelect').prop('disabled', true).html('<option value="">Önce tarih seçin</option>');
            resetSelection();
        }
    });

    // Saat seçildiğinde
    $('#timeSelect').change(function () {
        selectedTime = $(this).val();
        console.log('Seçilen saat:', selectedTime);
        if (selectedTime) {
            var timeOption = $(this).find('option:selected');
            var dateTimeStr = timeOption.data('datetime') || (selectedDate + 'T' + selectedTime + ':00');

            selectedDateTime = new Date(dateTimeStr);

            console.log('Seçilen tarih-saat:', selectedDateTime);

            // Hidden input'a değeri ata
            $('#randevuTarihiHidden').val(selectedDateTime.toISOString());

            // Seçimi göster
            updateSelectedAppointment();

            // Müsaitlik kontrolü yap
            checkAvailability();

            // Submit butonunu aktif et
            $('#submitBtn').prop('disabled', false);
        } else {
            resetSelection();
            $('#submitBtn').prop('disabled', true);
        }
    });

    // Tarihleri yükle
    function updateDateSelection() {
        if (selectedAntrenorId && selectedHizmetId) {
            // Antrenör bilgisini göster
            var antrenorOption = $('#antrenorSelect option:selected');
            $('#selectedAntrenorText').text(antrenorOption.data('name') || antrenorOption.text());

            // Tarihleri yükle
            loadAvailableDates();
        } else {
            $('#dateSelect').prop('disabled', true).html('<option value="">Önce antrenör ve hizmet seçin</option>');
            $('#timeSelect').prop('disabled', true).html('<option value="">Önce tarih seçin</option>');
            resetSelection();
        }
    }

    // Müsait tarihleri yükle
    function loadAvailableDates() {
        console.log('Müsait tarihler yükleniyor...');
        $('#dateLoading').show();
        $('#dateSelect').prop('disabled', true);

        $.ajax({
            url: '/UyeRandevu/GetAvailableDates',
            type: 'GET',
            data: {
                antrenorId: selectedAntrenorId,
                hizmetId: selectedHizmetId
            },
            success: function (response) {
                console.log('Tarih yanıtı:', response);

                $('#dateLoading').hide();

                if (response.success && response.dates && response.dates.length > 0) {
                    var dateSelect = $('#dateSelect');
                    dateSelect.html('<option value="">Tarih Seçiniz</option>');

                    $.each(response.dates, function (index, dateItem) {
                        console.log('Tarih öğesi:', dateItem);
                        dateSelect.append('<option value="' + dateItem.date + '">' + dateItem.displayText + '</option>');
                    });

                    dateSelect.prop('disabled', false);

                    // Başarı mesajı
                    showAvailabilityStatus(response.dates.length + ' müsait tarih bulundu.', 'success');
                } else {
                    $('#dateSelect').html('<option value="">Müsait tarih bulunamadı</option>');
                    var message = response.message || 'Önümüzdeki 30 gün içinde müsait tarih bulunamadı.';
                    showAvailabilityStatus(message, 'warning');
                }
            },
            error: function (xhr, status, error) {
                console.error('Tarih yükleme hatası:', error);
                $('#dateLoading').hide();
                $('#dateSelect').html('<option value="">Tarihler yüklenirken hata oluştu</option>');
                showAvailabilityStatus('Tarihler yüklenirken bir hata oluştu: ' + error, 'warning');
            }
        });
    }

    // Müsait saatleri yükle
    function loadAvailableTimes() {
        console.log('Müsait saatler yükleniyor...');
        $('#timeLoading').show();
        $('#timeSelect').prop('disabled', true);

        $.ajax({
            url: '/UyeRandevu/GetAvailableTimes',
            type: 'GET',
            data: {
                antrenorId: selectedAntrenorId,
                hizmetId: selectedHizmetId,
                date: selectedDate
            },
            success: function (response) {
                console.log('Saat yanıtı:', response);

                $('#timeLoading').hide();

                if (response.success && response.times && response.times.length > 0) {
                    var timeSelect = $('#timeSelect');
                    timeSelect.html('<option value="">Saat Seçiniz</option>');

                    $.each(response.times, function (index, timeItem) {
                        console.log('Saat öğesi:', timeItem);
                        timeSelect.append('<option value="' + timeItem.time + '" data-datetime="' + timeItem.dateTime + '">' + timeItem.displayText + '</option>');
                    });

                    timeSelect.prop('disabled', false);

                    showAvailabilityStatus(response.times.length + ' müsait saat aralığı bulundu.', 'success');
                } else {
                    $('#timeSelect').html('<option value="">Müsait saat bulunamadı</option>');
                    var message = response.message || 'Bu tarihte müsait saat bulunamadı. Lütfen başka bir tarih seçin.';
                    showAvailabilityStatus(message, 'warning');
                }
            },
            error: function (xhr, status, error) {
                console.error('Saat yükleme hatası:', error);
                $('#timeLoading').hide();
                $('#timeSelect').html('<option value="">Saatler yüklenirken hata oluştu</option>');
                showAvailabilityStatus('Saatler yüklenirken bir hata oluştu: ' + error, 'warning');
            }
        });
    }

    // Müsaitlik kontrolü yap
    function checkAvailability() {
        if (!selectedDateTime) return;

        console.log('Müsaitlik kontrolü yapılıyor...');
        console.log('Seçilen tarih-saat:', selectedDateTime);
        console.log('Antrenör ID:', selectedAntrenorId);
        console.log('Hizmet süresi:', hizmetSure);
    }

    // Seçilen randevuyu göster
    function updateSelectedAppointment() {
        var formattedDate = selectedDateTime.toLocaleDateString('tr-TR', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });

        $('#selectedDateTimeText').text(formattedDate);
        $('#selectedAppointment').show();
    }

    // Seçimi sıfırla
    function resetSelection() {
        selectedDate = null;
        selectedTime = null;
        selectedDateTime = null;
        $('#randevuTarihiHidden').val('');
        $('#selectedAppointment').hide();
        $('#submitBtn').prop('disabled', true);
        $('#availabilityStatus').hide();
    }

    // Müsaitlik durumunu göster
    function showAvailabilityStatus(message, type) {
        var statusDiv = $('#availabilityStatus');
        statusDiv.removeClass('success warning info');
        statusDiv.addClass(type);
        statusDiv.html('<i class="fas fa-info-circle"></i> ' + message);
        statusDiv.show();
    }

    // Form gönderilmeden önce son kontrol
    $('#randevuForm').submit(function (e) {
        console.log('Form gönderiliyor...');

        if (!selectedDateTime) {
            e.preventDefault();
            alert('Lütfen bir tarih ve saat seçin.');
            return false;
        }

        if (!$('#randevuTarihiHidden').val()) {
            e.preventDefault();
            alert('Randevu tarihi seçilmemiş.');
            return false;
        }

        // Submit butonunu devre dışı bırak
        $('#submitBtn').html('<i class="fas fa-spinner fa-spin"></i> Gönderiliyor...');
        $('#submitBtn').prop('disabled', true);

        return true;
    });

    // Debug için tüm seçimleri konsola yaz
    window.debugSelection = function () {
        console.log('DEBUG - Seçimler:', {
            antrenorId: selectedAntrenorId,
            hizmetId: selectedHizmetId,
            tarih: selectedDate,
            saat: selectedTime,
            dateTime: selectedDateTime,
            hizmetSure: hizmetSure
        });
    }
});