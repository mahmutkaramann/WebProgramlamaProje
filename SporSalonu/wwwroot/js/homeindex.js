// Carousel otomatik geçiş
document.addEventListener('DOMContentLoaded', function () {
    var myCarousel = document.querySelector('#heroCarousel');
    var carousel = new bootstrap.Carousel(myCarousel, {
        interval: 3000,
        wrap: true
    });
});