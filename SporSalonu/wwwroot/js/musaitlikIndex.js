document.addEventListener('DOMContentLoaded', function () {
    var deleteModal = document.getElementById('deleteModal');

    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var musaitlikId = button.getAttribute('data-id');
            var gun = button.getAttribute('data-gun');
            var baslangic = button.getAttribute('data-baslangic');
            var bitis = button.getAttribute('data-bitis');

            var musaitlikGun = deleteModal.querySelector('#musaitlikGun');
            var musaitlikSaat = deleteModal.querySelector('#musaitlikSaat');
            var deleteForm = deleteModal.querySelector('#deleteForm');

            musaitlikGun.textContent = gun;
            musaitlikSaat.textContent = baslangic + ' - ' + bitis;
            deleteForm.action = '/MusaitlikSaati/Delete/' + musaitlikId;
        });
    }
});