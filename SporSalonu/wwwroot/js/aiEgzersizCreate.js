$(document).ready(function () {
    initializeAIForm();
});

function initializeAIForm() {
    // İlk yüklemede istek tipini kontrol et
    handleRequestTypeChange($('#istekTipi').val());

    // İstek tipi değişikliği
    $('#istekTipi').change(function () {
        handleRequestTypeChange($(this).val());
    });

    // Karakter sayacı
    $('#girilenBilgiTextarea').on('input', function () {
        updateCharacterCount($(this).val().length);
    });

    // Fotoğraf önizleme
    $('#fotoInput').change(function (e) {
        handleFileUpload(e);
    });

    // Hızlı öneri butonu
    $('#quickSuggestionBtn').click(function () {
        handleQuickSuggestion();
    });

    // Form gönderimi
    $('#aiForm').on('submit', function () {
        return validateAIForm();
    });

    // Karakter sayacını başlat
    $('#girilenBilgiTextarea').trigger('input');
}

// İstek tipine göre form alanlarını yönet
function handleRequestTypeChange(selectedType) {
    const $fotoSection = $('#fotoSection');
    const $fotoInput = $('#fotoInput');

    // Görsel Simülasyon ve Vücut Analizi için fotoğraf alanını göster
    if (selectedType === 'GorselSimulasyon' || selectedType === 'VucutAnalizi') {
        $fotoSection.slideDown(300);
        $fotoInput.prop('required', true);

        if (selectedType === 'VucutAnalizi') {
            updateLabelAndPlaceholder(
                'Ek Açıklama (OPSİYONEL)',
                'Eklemek istediğiniz özel notlar...'
            );
            // Fotoğraf için açıklama güncelle
            $('label[for="Foto"]').text('Vücut Analizi Fotoğrafı (ZORUNLU)');
            $('#fotoDescription').html('<i class="fas fa-exclamation-circle text-danger"></i> Vücut analizi için fotoğraf zorunludur. Max 5MB');
        } else if (selectedType === 'GorselSimulasyon') {
            updateLabelAndPlaceholder(
                'Görsel Açıklaması',
                'Oluşturulmasını istediğiniz görselin detaylı açıklamasını yazın...'
            );
            // Fotoğraf için açıklama güncelle
            $('label[for="Foto"]').text('Mevcut Durum Fotoğrafı (ZORUNLU)');
            $('#fotoDescription').html('<i class="fas fa-exclamation-circle text-danger"></i> Görsel simülasyon için fotoğraf zorunludur. Max 5MB');
        }
    } else {
        // Egzersiz ve Diyet için fotoğraf alanını gizle
        $fotoSection.slideUp(300);
        $fotoInput.prop('required', false);
        updateLabelAndPlaceholder(
            'Açıklama / Soru',
            'Detaylı bilgi verirseniz daha iyi öneriler alabilirsiniz...'
        );
    }
}

// Karakter sayacını güncelle
function updateCharacterCount(length) {
    const remaining = 2000 - length;
    const $charCount = $('#charCount');

    $charCount.text('(' + length + '/2000 karakter)');

    // Renk sınıflarını temizle
    $charCount.removeClass('text-danger text-warning');

    // Kalan karaktere göre renk ekle
    if (remaining < 0) {
        $charCount.addClass('text-danger');
    } else if (remaining < 100) {
        $charCount.addClass('text-warning');
    }
}

// Dosya yükleme işlemi
function handleFileUpload(event) {
    const file = event.target.files[0];
    const $preview = $('#fotoPreview');

    if (!file) {
        $preview.html('');
        return;
    }

    // Dosya boyutu kontrolü (5MB)
    if (file.size > 5 * 1024 * 1024) {
        showAlert('Dosya boyutu 5MB\'dan küçük olmalıdır.', 'warning');
        $('#fotoInput').val('');
        $preview.html('');
        return;
    }

    // Dosya türü kontrolü
    if (!file.type.match('image.*')) {
        showAlert('Lütfen sadece resim dosyası yükleyin.', 'warning');
        $('#fotoInput').val('');
        $preview.html('');
        return;
    }

    // Önizleme oluştur
    const reader = new FileReader();
    reader.onload = function (e) {
        $preview.html(`
            <div class="mt-2">
                <img src="${e.target.result}" class="img-thumbnail" style="max-height: 200px;">
                <small class="d-block text-muted mt-1">
                    <i class="fas fa-image"></i> Önizleme: ${file.name} (${Math.round(file.size / 1024)} KB)
                </small>
            </div>
        `);
    };
    reader.readAsDataURL(file);
}

// Hızlı öneri oluştur
function handleQuickSuggestion() {
    const hedef = $('#hedefInput').val().trim();
    const yas = $('#yasInput').val();
    const cinsiyet = $('#cinsiyetSelect').val();
    const boy = $('#boyInput').val();
    const kilo = $('#kiloInput').val();
    const istekTipi = $('#istekTipi').val();

    if (!hedef) {
        showAlert('Lütfen bir hedef giriniz.', 'warning');
        $('#hedefInput').focus();
        return;
    }

    const $btn = $('#quickSuggestionBtn');
    setButtonLoading($btn, true);

    // Prompt oluştur
    const prompt = generatePrompt(hedef, yas, cinsiyet, boy, kilo, istekTipi);

    // Textarea'ya ekle
    $('#girilenBilgiTextarea').val(prompt).trigger('input');

    // Butonu eski haline getir
    setTimeout(() => {
        setButtonLoading($btn, false);
        showAlert('Hızlı öneri metni oluşturuldu! Dilediğiniz gibi düzenleyebilirsiniz.', 'success');
    }, 1000);
}

// Form validasyonu
function validateAIForm() {
    const istekTipi = $('#istekTipi').val();
    const girilenBilgi = $('#girilenBilgiTextarea').val().trim();

    // İstek tipi kontrolü
    if (!istekTipi) {
        showAlert('Lütfen bir istek tipi seçiniz.', 'warning');
        return false;
    }

    // Görsel Simülasyon ve Vücut Analizi için fotoğraf kontrolü
    if ((istekTipi === 'GorselSimulasyon' || istekTipi === 'VucutAnalizi') && $('#fotoInput').val() === '') {
        showAlert('Bu seçenek için fotoğraf yüklemelisiniz.', 'warning');
        return false;
    }

    // Temel açıklama kontrolü
    if (!girilenBilgi) {
        showAlert('Lütfen bir açıklama veya soru giriniz.', 'warning');
        return false;
    }

    // Karakter sınırı kontrolü
    if (girilenBilgi.length > 2000) {
        showAlert('Açıklama 2000 karakteri geçemez.', 'warning');
        return false;
    }

    // Butonu yükleme durumuna getir
    const $submitBtn = $('#submitBtn');
    setButtonLoading($submitBtn, true, 'İşleniyor...');

    return true;
}

// Yardımcı fonksiyonlar
function updateLabelAndPlaceholder(labelText, placeholderText) {
    $('#bilgiLabel').text(labelText);
    $('#girilenBilgiTextarea').attr('placeholder', placeholderText);
}

function setButtonLoading($button, isLoading, loadingText = 'İşleniyor...') {
    if (isLoading) {
        $button.prop('disabled', true);
        $button.html(`<i class="fas fa-spinner fa-spin"></i> ${loadingText}`);
    } else {
        $button.prop('disabled', false);
        $button.html('<i class="fas fa-bolt"></i> Hızlı Öneri');
    }
}

function generatePrompt(hedef, yas, cinsiyet, boy, kilo, istekTipi) {
    let prompt = `Hedefim: ${hedef}\n`;

    if (yas) prompt += `Yaşım: ${yas}\n`;
    if (cinsiyet) prompt += `Cinsiyetim: ${cinsiyet}\n`;
    if (boy) prompt += `Boyum: ${boy} cm\n`;
    if (kilo) prompt += `Kilo: ${kilo} kg\n\n`;

    switch (istekTipi) {
        case 'EgzersizOnerisi':
            prompt += "Bu bilgilere göre bana uygun bir egzersiz programı önerir misin?";
            break;
        case 'DiyetOnerisi':
            prompt += "Bu bilgilere göre bana uygun bir beslenme programı önerir misin?";
            break;
        case 'GorselSimulasyon':
            prompt += "Bu bilgilere göre bir görsel simülasyon oluşturabilir misin?";
            break;
        case 'VucutAnalizi':
            prompt += "Bu bilgilere göre vücut analizimi yapabilir misin?";
            break;
        default:
            prompt += "Bu hedefime ulaşmak için ne yapmalıyım?";
    }

    return prompt;
}

function showAlert(message, type = 'info') {
    const alertTypes = {
        'success': { class: 'alert-success', icon: 'fa-check-circle' },
        'warning': { class: 'alert-warning', icon: 'fa-exclamation-triangle' },
        'danger': { class: 'alert-danger', icon: 'fa-exclamation-circle' },
        'info': { class: 'alert-info', icon: 'fa-info-circle' }
    };

    const alertType = alertTypes[type] || alertTypes.info;

    // Mevcut alert'leri temizle (bilgi alert'ini koru)
    $('.alert-dismissible:not(.alert-info)').remove();

    // Yeni alert oluştur
    const alertHtml = `
        <div class="alert ${alertType.class} alert-dismissible fade show" role="alert">
            <i class="fas ${alertType.icon}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('.card-body').prepend(alertHtml);

    // Otomatik kapanma (bilgi alert'ini kapatmaz)
    if (type !== 'info') {
        setTimeout(() => {
            $(`.alert-${type}`).alert('close');
        }, 5000);
    }
}

// AJAX hızlı öneri
function quickSuggestionAjax() {
    const goal = $('#hedefInput').val();
    const age = $('#yasInput').val();
    const gender = $('#cinsiyetSelect').val();
    const height = $('#boyInput').val();
    const weight = $('#kiloInput').val();

    $.ajax({
        url: '/AIEgzersizOneri/QuickExerciseSuggestion',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            goal: goal,
            age: age,
            gender: gender,
            height: height,
            weight: weight
        }),
        success: function (response) {
            if (response.success) {
                $('#girilenBilgiTextarea').val(response.response);
                showAlert('Hızlı öneri başarıyla alındı!', 'success');
            } else {
                showAlert('Bir hata oluştu: ' + response.error, 'danger');
            }
        },
        error: function () {
            showAlert('Sunucu hatası. Lütfen tekrar deneyin.', 'danger');
        }
    });
}