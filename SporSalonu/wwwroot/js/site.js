//Home Index için
$(document).ready(function () {
    // Slider responsive ayarı
    function adjustSlider() {
        var sliderWidth = $('.slider-right').width();
        $('#slider1 li img').css({
            'width': sliderWidth,
            'height': 'auto'
        });
    }

    // Sayfa yüklendiğinde ve boyut değiştiğinde
    adjustSlider();
    $(window).resize(adjustSlider);

    // Basit slider döngüsü (isteğe bağlı)
    var currentSlide = 0;
    var slideCount = $('#slider1 li').length;

    function nextSlide() {
        currentSlide = (currentSlide + 1) % slideCount;
        $('#slider1 li').removeClass('active').eq(currentSlide).addClass('active');
    }

    // Otomatik geçiş (isteğe bağlı)
    setInterval(nextSlide, 5000);
});
// HOME INDEX BİTTİ