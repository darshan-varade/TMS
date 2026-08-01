function showToast(message, type) {
    var container = document.getElementById('toastContainer');
    if (!container) return;
    
    var toast = document.createElement('div');
    toast.className = 'toast align-items-center border-0 show shadow-lg mb-2';
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    var iconClass = 'bi-info-circle-fill';
    var accentColor = '#4F46E5'; // primary default
    
    if (type === 'error') {
        iconClass = 'bi-exclamation-octagon-fill text-danger';
        accentColor = '#EF4444';
        toast.style.borderLeft = '4px solid #EF4444';
    } else if (type === 'warning') {
        iconClass = 'bi-exclamation-triangle-fill text-warning';
        accentColor = '#F59E0B';
        toast.style.borderLeft = '4px solid #F59E0B';
    } else if (type === 'success') {
        iconClass = 'bi-check-circle-fill text-success';
        accentColor = '#10B981';
        toast.style.borderLeft = '4px solid #10B981';
    } else {
        iconClass = 'bi-info-circle-fill text-primary';
        toast.style.borderLeft = '4px solid var(--primary)';
    }
    
    toast.style.backgroundColor = 'var(--card-bg)';
    toast.style.color = 'var(--text-primary)';
    toast.style.borderRadius = '10px';
    
    toast.innerHTML = 
        '<div class="d-flex align-items-center p-3">' +
            '<div class="fs-5 me-3 d-flex align-items-center">' +
                '<i class="bi ' + iconClass + '"></i>' +
            '</div>' +
            '<div class="toast-body p-0 fw-medium" style="font-size: 0.9rem;">' + message + '</div>' +
            '<button type="button" class="btn-close ms-auto me-0" data-bs-dismiss="toast" style="font-size: 0.75rem;' + 
            '"></button>' +
        '</div>';
        
    container.appendChild(toast);
    
    // Auto-remove after 4 seconds
    setTimeout(function () {
        toast.classList.remove('show');
        toast.classList.add('fade');
        setTimeout(function() { toast.remove(); }, 300);
    }, 4000);
}

function showConfirm(title, message, confirmBtnText, callback) {
    // If only 3 arguments are passed, adjust signature
    if (typeof confirmBtnText === 'function') {
        callback = confirmBtnText;
        confirmBtnText = 'Confirm';
    }
    
    $('#confirmModalTitle').text(title || 'Confirm Action');
    $('#confirmModalMessage').text(message || 'Are you sure you want to proceed?');
    $('#confirmModalBtn').text(confirmBtnText || 'Confirm');
    
    // Customize button color if it's a destructive action like Delete
    if (title.toLowerCase().indexOf('delete') > -1 || message.toLowerCase().indexOf('delete') > -1) {
        $('#confirmModalBtn').removeClass('btn-primary').addClass('btn-danger');
    } else {
        $('#confirmModalBtn').removeClass('btn-danger').addClass('btn-primary');
    }
    
    $('#confirmModalBtn').off('click').on('click', function () {
        $('#confirmModal').modal('hide');
        if (callback) callback();
    });
    $('#confirmModal').modal('show');
}

$(function () {
    // Sidebar toggles
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
    
    // Set active sidebar item based on URL path
    var currentPath = window.location.pathname;
    $('.sidebar-nav-item').removeClass('active');
    $('.sidebar-nav-item').each(function() {
        var href = $(this).attr('href');
        if (href && (currentPath === href || currentPath.indexOf(href) === 0 && href !== '/')) {
            $(this).addClass('active');
        }
    });
    // Fallback if home
    if (currentPath === '/' || currentPath === '/Dashboard' || currentPath === '/Dashboard/Index') {
        $('.sidebar-nav-item[href*="Dashboard"]').addClass('active');
    }


});

