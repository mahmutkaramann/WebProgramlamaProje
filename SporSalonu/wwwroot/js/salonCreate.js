$(document).ready(function () {
    initializeCreatePage();
});

function initializeCreatePage() {
    // Zaman inputlarına min-max değerleri ekle
    $('#AcilisSaati').attr('min', '00:00').attr('max', '23:59');
    $('#KapanisSaati').attr('min', '00:00').attr('max', '23:59');

    // Telefon input'u için mask
    $('#Telefon').on('input', function () {
        formatPhoneNumber($(this));
    });

    // Salon adına göre otomatik email oluştur
    $('#SalonAdi').on('input', function () {
        autoGenerateEmail($(this).val());
    });

    // Form submit işlemi
    $('#createSalonForm').submit(function (e) {
        return validateForm();
    });
}

// Form validasyonu
function validateForm() {
    // Zorunlu alan kontrolleri
    if (!$('#SalonAdi').val().trim()) {
        toastr.error('Salon adı zorunludur.');
        $('#SalonAdi').focus();
        return false;
    }

    // Saat kontrolleri
    const acilis = $('#AcilisSaati').val();
    const kapanis = $('#KapanisSaati').val();

    if (!acilis || !kapanis) {
        toastr.error('Açılış ve kapanış saatleri zorunludur.');
        return false;
    }

    if (acilis >= kapanis) {
        toastr.error('Kapanış saati açılış saatinden sonra olmalıdır.');
        return false;
    }

    return true;
}

// API ile salon oluşturma
function createSalonViaAPI() {
    const salonData = getFormData();

    // Validasyon
    if (!salonData.salonAdi) {
        toastr.error('Salon adı zorunludur.');
        return;
    }

    showLoadingIndicator();

    $.ajax({
        url: '/api/SalonApi',
        type: 'POST',
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        data: JSON.stringify(salonData),
        success: function (response) {
            showSuccessResponse(response);
        },
        error: function (xhr) {
            showErrorResponse(xhr);
        }
    });
}

// Form verilerini al
function getFormData() {
    return {
        salonAdi: $('#SalonAdi').val().trim(),
        adres: $('#Adres').val().trim(),
        telefon: $('#Telefon').val().trim(),
        email: $('#Email').val().trim(),
        acilisSaati: formatTimeForAPI($('#AcilisSaati').val()),
        kapanisSaati: formatTimeForAPI($('#KapanisSaati').val())
    };
}

// Saat formatını API için düzenle
function formatTimeForAPI(time) {
    return time ? time + ':00' : null;
}

// Yükleme göstergesi
function showLoadingIndicator() {
    $('#apiResult').html(`
        <div class="text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Yükleniyor...</span>
            </div>
            <p class="mt-2">API ile oluşturuluyor...</p>
        </div>
    `);
}

// Başarılı yanıtı göster
function showSuccessResponse(response) {
    $('#apiResult').html(`
        <div class="alert alert-success">
            <i class="fas fa-check-circle me-2"></i>
            Salon API üzerinden başarıyla oluşturuldu!
            <br><small>Salon ID: ${response.salon.salonId}</small>
        </div>
        <div class="mt-2">
            <a href="/Salon/Details/${response.salon.salonId}" class="btn btn-sm btn-success">
                <i class="fas fa-eye me-1"></i>Detayları Gör
            </a>
            <a href="/Salon/Index" class="btn btn-sm btn-primary">
                <i class="fas fa-list me-1"></i>Listeye Git
            </a>
        </div>
    `);

    toastr.success(response.message || 'Salon başarıyla oluşturuldu!');
}

// Hata yanıtını göster
function showErrorResponse(xhr) {
    let errorMessage = 'API hatası oluştu.';
    if (xhr.responseJSON && xhr.responseJSON.message) {
        errorMessage = xhr.responseJSON.message;
    }

    $('#apiResult').html(`
        <div class="alert alert-danger">
            <i class="fas fa-exclamation-triangle me-2"></i>
            ${escapeHtml(errorMessage)}
        </div>
    `);

    toastr.error(errorMessage);
}

// Şablon yükleme
function loadTemplate(type) {
    const templates = {
        'main': {
            salonAdi: 'Ana Spor Salonu',
            adres: 'Merkez Mah. Spor Cad. No:1',
            telefon: '0212 345 67 89',
            email: 'ana@sporsalonu.com',
            acilisSaati: '06:00',
            kapanisSaati: '23:00'
        },
        'pool': {
            salonAdi: 'Yüzme Havuzu',
            adres: 'Spor Kompleksi Havuz Binası',
            telefon: '0212 987 65 43',
            email: 'havuz@sporsalonu.com',
            acilisSaati: '07:00',
            kapanisSaati: '21:00'
        },
        'yoga': {
            salonAdi: 'Pilates ve Yoga Salonu',
            adres: 'Studio Binası Kat:2',
            telefon: '0212 111 22 33',
            email: 'studio@sporsalonu.com',
            acilisSaati: '08:00',
            kapanisSaati: '20:00'
        }
    };

    const template = templates[type];
    if (template) {
        $('#SalonAdi').val(template.salonAdi);
        $('#Adres').val(template.adres);
        $('#Telefon').val(template.telefon);
        $('#Email').val(template.email);
        $('#AcilisSaati').val(template.acilisSaati);
        $('#KapanisSaati').val(template.kapanisSaati);

        toastr.success(`"${template.salonAdi}" şablonu yüklendi.`);
    }
}

// Oluştur ve devam et
function saveAndContinue() {
    $('#createSalonForm').append('<input type="hidden" name="continue" value="true">');
    $('#createSalonForm').submit();
}

// Telefon numarası formatlama
function formatPhoneNumber(input) {
    let value = input.val().replace(/\D/g, '');
    if (value.length > 0) {
        if (value.length <= 4) {
            value = value.replace(/(\d{1,4})/, '$1');
        } else if (value.length <= 7) {
            value = value.replace(/(\d{4})(\d{1,3})/, '$1 $2');
        } else {
            value = value.replace(/(\d{4})(\d{3})(\d{1,4})/, '$1 $2 $3');
        }
    }
    input.val(value);
}

// Otomatik email oluştur
function autoGenerateEmail(salonAdi) {
    if (salonAdi && !$('#Email').val()) {
        const email = salonAdi.toLowerCase()
            .replace(/[^a-z0-9]/g, '')
            .replace(/\s+/g, '') + '@sporsalonu.com';
        $('#Email').val(email);
    }
}

// HTML escape
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text?.toString().replace(/[&<>"']/g, m => map[m]) || '';
}

// Global fonksiyonlar (HTML'de onclick ile çağrılanlar)
window.createSalonViaAPI = createSalonViaAPI;
window.loadTemplate = loadTemplate;
window.saveAndContinue = saveAndContinue;