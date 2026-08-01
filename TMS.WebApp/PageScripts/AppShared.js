function showToast(message, type) {
    var container = document.getElementById('toastContainer');
    if (!container) return;
    
    var toast = document.createElement('div');
    toast.className = 'toast align-items-center border-0 show shadow-lg mb-2';
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    var iconClass = 'bi-info-circle-fill';
    var bgColor = '#f0f9ff';
    var textColor = '#0369a1';
    var borderColor = '#0ea5e9';
    
    if (type === 'error') {
        iconClass = 'bi-exclamation-octagon-fill';
        bgColor = '#fef2f2';
        textColor = '#b91c1c';
        borderColor = '#EF4444';
    } else if (type === 'warning') {
        iconClass = 'bi-exclamation-triangle-fill';
        bgColor = '#fffbeb';
        textColor = '#b45309';
        borderColor = '#F59E0B';
    } else if (type === 'success') {
        iconClass = 'bi-check-circle-fill';
        bgColor = '#f0fdf4';
        textColor = '#15803d';
        borderColor = '#10B981';
    } else {
        iconClass = 'bi-info-circle-fill';
        bgColor = '#f0f9ff';
        textColor = '#0369a1';
        borderColor = '#0ea5e9';
    }
    
    toast.style.backgroundColor = bgColor;
    toast.style.color = textColor;
    toast.style.borderLeft = '4px solid ' + borderColor;
    toast.style.borderRadius = '8px';
    
    toast.innerHTML = 
        '<div class="d-flex align-items-center p-3">' +
            '<div class="fs-5 me-3 d-flex align-items-center" style="color: ' + borderColor + ';">' +
                '<i class="bi ' + iconClass + '"></i>' +
            '</div>' +
            '<div class="toast-body p-0 fw-semibold" style="font-size: 0.9rem;">' + message + '</div>' +
            '<button type="button" class="btn-close ms-auto me-0" data-bs-dismiss="toast" style="font-size: 0.75rem; filter: opacity(0.5);' + 
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
    var currentPath = window.location.pathname.toLowerCase();
    if (currentPath.endsWith('/') && currentPath.length > 1) {
        currentPath = currentPath.substring(0, currentPath.length - 1);
    }
    
    var matchedItem = null;
    var exactMatchFound = false;
    
    $('.sidebar-nav-item').removeClass('active');
    
    // First pass: try to find an exact match
    $('.sidebar-nav-item').each(function() {
        var href = $(this).attr('href');
        if (href) {
            href = href.toLowerCase();
            if (href.endsWith('/') && href.length > 1) {
                href = href.substring(0, href.length - 1);
            }
            if (currentPath === href) {
                matchedItem = $(this);
                exactMatchFound = true;
                return false;
            }
        }
    });
    
    // Second pass: if no exact match, prefix match
    if (!exactMatchFound) {
        $('.sidebar-nav-item').each(function() {
            var href = $(this).attr('href');
            if (href) {
                href = href.toLowerCase();
                if (href.endsWith('/') && href.length > 1) {
                    href = href.substring(0, href.length - 1);
                }
                if (href !== '/' && (currentPath.indexOf(href + '/') === 0)) {
                    matchedItem = $(this);
                    return false;
                }
            }
        });
    }
    
    // Fallback for dashboard home paths
    if (!matchedItem && (currentPath === '/' || currentPath === '/dashboard' || currentPath === '/dashboard/index')) {
        matchedItem = $('.sidebar-nav-item[href*="Dashboard"]');
    }
    
    if (matchedItem) {
        matchedItem.addClass('active');
    }


});

