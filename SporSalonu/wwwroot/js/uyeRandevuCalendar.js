// wwwroot/js/uyeRandevuTakvim.js

document.addEventListener('DOMContentLoaded', function () {
    // FullCalendar kütüphanesini kontrol et
    if (typeof FullCalendar === 'undefined') {
        console.error('FullCalendar kütüphanesi yüklenemedi.');
        return;
    }

    var calendarEl = document.getElementById('calendar');

    if (!calendarEl) {
        console.error('#calendar elementi bulunamadı.');
        return;
    }

    // Takvim verilerini al
    var takvimVerileri = window.takvimVerileri || [];

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'tr',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        events: takvimVerileri.map(function (event) {
            return {
                id: event.id,
                title: event.title,
                start: event.start,
                end: event.end,
                color: event.color,
                extendedProps: {
                    antrenorAdi: event.antrenorAdi,
                    hizmetAdi: event.hizmetAdi,
                    durum: event.durum
                }
            };
        }),
        eventClick: function (info) {
            // Randevu detaylarına git
            window.location.href = '/UyeRandevu/Details/' + info.event.id;
        },
        eventMouseEnter: function (info) {
            // Tooltip göster
            info.el.setAttribute('title',
                info.event.extendedProps.antrenorAdi + ' - ' +
                info.event.extendedProps.hizmetAdi + '\n' +
                'Durum: ' + info.event.extendedProps.durum
            );
        }
    });

    calendar.render();
});