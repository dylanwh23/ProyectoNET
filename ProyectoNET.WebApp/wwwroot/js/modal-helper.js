// Usa la API de Bootstrap (o mdb.Modal si está cargado) para mostrar el modal por ID
function showBootstrapModal(modalId) {
    var modalElement = document.getElementById(modalId);
    if (modalElement) {
        // Preferimos Bootstrap 5 (bootstrap.Modal) si está disponible
        var ModalLib = typeof bootstrap !== 'undefined' ? bootstrap.Modal : window.Modal; 
        
        // Si el modal ya fue inicializado, solo lo mostramos. Si no, lo inicializamos.
        var modal = ModalLib.getInstance(modalElement) || new ModalLib(modalElement);
        modal.show();
    }
}

// Usa la API de Bootstrap para ocultar el modal por ID
function hideBootstrapModal(modalId) {
    var modalElement = document.getElementById(modalId);
    if (modalElement) {
        var ModalLib = typeof bootstrap !== 'undefined' ? bootstrap.Modal : window.Modal;
        var modalInstance = ModalLib.getInstance(modalElement);
        if (modalInstance) {
            modalInstance.hide();
        }
    }
}
