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
    
    setTimeout(function () {
        toast.classList.remove('show');
        toast.classList.add('fade');
        setTimeout(function() { toast.remove(); }, 300);
    }, 4000);
}

function showConfirm(title, message, confirmBtnText, callback) {
    if (typeof confirmBtnText === 'function') {
        callback = confirmBtnText;
        confirmBtnText = 'Confirm';
    }
    
    $('#confirmModalTitle').text(title || 'Confirm Action');
    $('#confirmModalMessage').text(message || 'Are you sure you want to proceed?');
    $('#confirmModalBtn').text(confirmBtnText || 'Confirm');
    
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
    
    $(document).on('click', '.sidebar-nav-item', function () {
        if (window.matchMedia('(max-width: 991.98px)').matches) {
            sidebar.classList.remove('show');
            if (overlay) overlay.classList.remove('show');
        }
    });
    
    var currentPath = window.location.pathname.toLowerCase();
    if (currentPath.endsWith('/') && currentPath.length > 1) {
        currentPath = currentPath.substring(0, currentPath.length - 1);
    }
    var currentQuery = window.location.search.toLowerCase();
    
    var matchedItem = null;
    var exactMatchFound = false;
    
    $('.sidebar-nav-item').removeClass('active');
    
    $('.sidebar-nav-item').each(function() {
        var hrefAttr = $(this).attr('href');
        if (hrefAttr) {
            var parts = hrefAttr.toLowerCase().split('?');
            var linkPath = parts[0];
            var linkQuery = parts.length > 1 ? '?' + parts[1] : '';
            
            if (linkPath.endsWith('/') && linkPath.length > 1) {
                linkPath = linkPath.substring(0, linkPath.length - 1);
            }
            
            if (currentPath === linkPath && currentQuery === linkQuery) {
                matchedItem = $(this);
                exactMatchFound = true;
                return false;
            }
        }
    });
    
    if (!exactMatchFound) {
        $('.sidebar-nav-item').each(function() {
            var hrefAttr = $(this).attr('href');
            if (hrefAttr) {
                var parts = hrefAttr.toLowerCase().split('?');
                var linkPath = parts[0];
                var linkQuery = parts.length > 1 ? '?' + parts[1] : '';
                
                if (linkPath.endsWith('/') && linkPath.length > 1) {
                    linkPath = linkPath.substring(0, linkPath.length - 1);
                }
                
                if (currentPath === linkPath && linkQuery === '') {
                    matchedItem = $(this);
                    exactMatchFound = true;
                    return false;
                }
            }
        });
    }
    
    if (!exactMatchFound) {
        $('.sidebar-nav-item').each(function() {
            var hrefAttr = $(this).attr('href');
            if (hrefAttr) {
                var parts = hrefAttr.toLowerCase().split('?');
                var linkPath = parts[0];
                if (linkPath.endsWith('/') && linkPath.length > 1) {
                    linkPath = linkPath.substring(0, linkPath.length - 1);
                }
                if (linkPath !== '/' && (currentPath.indexOf(linkPath + '/') === 0)) {
                    matchedItem = $(this);
                    return false;
                }
            }
        });
    }
    
    if (!matchedItem && (currentPath === '/' || currentPath === '/dashboard' || currentPath === '/dashboard/index')) {
        matchedItem = $('.sidebar-nav-item[href*="Dashboard"]');
    }
    
    if (matchedItem) {
        matchedItem.addClass('active');
        var parentCollapse = matchedItem.closest('.collapse');
        if (parentCollapse.length > 0) {
            parentCollapse.addClass('show');
            var toggleButton = $('[data-bs-target="#' + parentCollapse.attr('id') + '"]');
            toggleButton.removeClass('collapsed').attr('aria-expanded', 'true');
        }
    }

    if ($('#unassignedCountBadge').length > 0) {
        $.getJSON('/Ticket/GetUnassignedCount', function(data) {
            if (data && data.count > 0) {
                $('#unassignedCountBadge').text(data.count).show();
            }
        });
    }
});

