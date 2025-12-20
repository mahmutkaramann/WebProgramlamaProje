let currentPage = 1;
let currentSearch = '';
let currentSortBy = 'SalonAdi';
let currentSortOrder = 'asc';
let currentPageSize = 10;
let salonToDelete = null;

// Sayfa yüklendiğinde salonları getir
$(document).ready(function () {
    initializeEventListeners();
    loadSalons();
});

// Event listener'ları başlat
function initializeEventListeners() {
    // Filtreleme butonu
    $('#filterBtn').click(function () {
        currentPage = 1;
        currentSearch = $('#searchInput').val();
        currentSortBy = $('#sortBy').val();
        currentSortOrder = $('#sortOrder').val();
        currentPageSize = $('#pageSize').val();
        loadSalons();
    });

    // Enter tuşu ile arama
    $('#searchInput').keypress(function (e) {
        if (e.which === 13) {
            $('#filterBtn').click();
        }
    });

    // Silme modalı
    $('#confirmDelete').click(function () {
        if (salonToDelete) {
            deleteSalon(salonToDelete);
        }
    });
}

// Salonları API'den yükle
function loadSalons() {
    const apiUrl = `/api/SalonApi?search=${encodeURIComponent(currentSearch)}&sortBy=${currentSortBy}&sortOrder=${currentSortOrder}&page=${currentPage}&pageSize=${currentPageSize}`;

    $.ajax({
        url: apiUrl,
        type: 'GET',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            updateTable(response.data);
            updatePagination(response);
        },
        error: function (xhr) {
            showError('Salonlar yüklenirken bir hata oluştu.');
        }
    });
}

// Tabloyu güncelle
function updateTable(salons) {
    const tbody = $('#salonTableBody');
    tbody.empty();

    if (salons.length === 0) {
        tbody.append(`
            <tr>
                <td colspan="8" class="text-center">
                    <div class="alert alert-info mb-0">
                        <i class="fas fa-info-circle me-2"></i>Kayıt bulunamadı.
                    </div>
                </td>
            </tr>
        `);
        return;
    }

    salons.forEach(salon => {
        const row = `
            <tr>
                <td>
                    <strong>${escapeHtml(salon.salonAdi)}</strong>
                </td>
                <td>${escapeHtml(salon.adres || '-')}</td>
                <td>${escapeHtml(salon.telefon || '-')}</td>
                <td>${escapeHtml(salon.email || '-')}</td>
                <td>${formatTime(salon.acilisSaati)} - ${formatTime(salon.kapanisSaati)}</td>
                <td>
                    <span class="badge bg-info">${salon.antrenorSayisi} Antrenör</span>
                </td>
                <td>
                    <span class="badge bg-success">${salon.hizmetSayisi} Hizmet</span>
                </td>
                <td>
                    <div class="btn-group" role="group">
                        <a href="/Salon/Details/${salon.salonId}" class="btn btn-sm btn-info" title="Detaylar">
                            <i class="fas fa-eye"></i>
                        </a>
                        <a href="/Salon/Edit/${salon.salonId}" class="btn btn-sm btn-warning" title="Düzenle">
                            <i class="fas fa-edit"></i>
                        </a>
                        <button onclick="showDeleteModal(${salon.salonId}, '${escapeHtml(salon.salonAdi)}')"
                                class="btn btn-sm btn-danger" title="Sil">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
}

// Sayfalamayı güncelle
function updatePagination(response) {
    const pagination = $('#pagination');
    pagination.empty();

    // Önceki sayfa butonu
    const prevClass = response.currentPage === 1 ? 'disabled' : '';
    pagination.append(`
        <li class="page-item ${prevClass}">
            <a class="page-link" href="#" onclick="changePage(${response.currentPage - 1})">
                <i class="fas fa-chevron-left"></i>
            </a>
        </li>
    `);

    // Sayfa numaraları
    for (let i = 1; i <= response.totalPages; i++) {
        const activeClass = i === response.currentPage ? 'active' : '';
        pagination.append(`
            <li class="page-item ${activeClass}">
                <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
            </li>
        `);
    }

    // Sonraki sayfa butonu
    const nextClass = response.currentPage === response.totalPages ? 'disabled' : '';
    pagination.append(`
        <li class="page-item ${nextClass}">
            <a class="page-link" href="#" onclick="changePage(${response.currentPage + 1})">
                <i class="fas fa-chevron-right"></i>
            </a>
        </li>
    `);
}

// Sayfa değiştirme
function changePage(page) {
    currentPage = page;
    loadSalons();
    $('html, body').animate({ scrollTop: 0 }, 'slow');
}

// Silme modalını göster
function showDeleteModal(id, salonAdi) {
    salonToDelete = id;
    $('#deleteModal .modal-body p:first').text(`"${salonAdi}" adlı salonu silmek istediğinize emin misiniz?`);
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

// Salon silme
function deleteSalon(id) {
    $.ajax({
        url: `/api/SalonApi/${id}`,
        type: 'DELETE',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            $('#deleteModal').modal('hide');
            showSuccess(response.message || 'Salon başarıyla silindi.');
            loadSalons();
        },
        error: function (xhr) {
            $('#deleteModal').modal('hide');
            if (xhr.responseJSON && xhr.responseJSON.message) {
                showError(xhr.responseJSON.message);
            } else {
                showError('Silme işlemi sırasında bir hata oluştu.');
            }
        }
    });
}

// Yardımcı fonksiyonlar
function formatTime(timeString) {
    const time = new Date(`2000-01-01T${timeString}`);
    return time.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
}

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

function showSuccess(message) {
    toastr.success(message);
}

function showError(message) {
    toastr.error(message);
}

// Global fonksiyonlar (HTML'de onclick ile çağrılanlar)
window.changePage = changePage;
window.showDeleteModal = showDeleteModal;