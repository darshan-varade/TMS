function showToast(message, type) {
    var bg = type === 'error' ? '#dc3545' : type === 'warning' ? '#ffc107' : '#0dcaf0';
    var container = document.getElementById('toastContainer');
    if (!container) return;
    var toast = document.createElement('div');
    toast.className = 'toast align-items-center text-white border-0 show';
    toast.style.background = bg;
    toast.innerHTML = '<div class="d-flex"><div class="toast-body">' + message + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
    container.appendChild(toast);
    setTimeout(function () { toast.remove(); }, 4000);
}

function showConfirm(title, message, callback) {
    $('#confirmModalTitle').text(title || 'Confirm');
    $('#confirmModalMessage').text(message || 'Are you sure?');
    $('#confirmModalBtn').off('click').on('click', function () {
        $('#confirmModal').modal('hide');
        if (callback) callback();
    });
    $('#confirmModal').modal('show');
}

$(function () {
    var sidebar = document.getElementById('sidebar');
    var toggle = document.getElementById('sidebarToggle');
    var overlay = document.getElementById('sidebarOverlay');
    if (toggle && sidebar) {
        toggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
            if (overlay) overlay.classList.toggle('show');
        });
    }
    if (overlay) {
        overlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        });
    }
    var activeLink = document.querySelector('.sidebar-nav-item[href="' + window.location.pathname + '"]');
    if (activeLink) activeLink.classList.add('active');
});
