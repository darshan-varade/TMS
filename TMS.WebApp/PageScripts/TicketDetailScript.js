$(function () {
    var actionModalEl = document.getElementById('ticketActionModal');
    if (!actionModalEl) return;
    var actionModal = new bootstrap.Modal(actionModalEl);
    var actionModalContent = $('#ticketActionModalContent');

    function initModalSelect2() {
        actionModalContent.find('select.form-select').each(function () {
            if (!$(this).data('select2')) {
                $(this).select2({
                    width: '100%',
                    dropdownParent: actionModalEl
                });
            }
        });
    }

    function loadModal(url, ticketId) {
        actionModalContent.html('<div class="modal-body text-center py-4"><div class="spinner-border text-primary" role="status"></div></div>');
        actionModal.show();
        $.get(url, { id: ticketId })
            .done(function (html) {
                actionModalContent.html(html);
                initModalSelect2();
                var form = actionModalContent.find('form');
                if (form.length) bindModalForm(form);
            })
            .fail(function () {
                actionModal.hide();
                showToast('Unable to load. Please try again.', 'error');
            });
    }

    function bindModalForm(form) {
        form.off('submit').on('submit', function (e) {
            e.preventDefault();
            var $form = $(this);
            var btn = $form.find('button[type="submit"]');
            btn.prop('disabled', true);

            $.post($form.attr('action'), $form.serialize())
                .done(function (response) {
                    if (response && response.success !== undefined) {
                        actionModal.hide();
                        showToast(response.message, response.success ? 'info' : 'error');
                        if (response.success) {
                            setTimeout(function () { window.location.reload(); }, 800);
                        }
                    } else {
                        actionModalContent.html(response);
                        initModalSelect2();
                        bindModalForm(actionModalContent.find('form'));
                    }
                })
                .fail(function (xhr) {
                    if (xhr.status === 403 || xhr.status === 401) {
                        actionModal.hide();
                        showToast('You are not authorized to perform this action.', 'error');
                    } else {
                        showToast('An error occurred. Please try again.', 'error');
                        btn.prop('disabled', false);
                    }
                });
        });
    }

    $(document).on('click', '[data-action="assign"]', function () {
        var id = $(this).data('ticket-id');
        loadModal('/Ticket/AssignPartial', id);
    });

    $(document).on('click', '[data-action="updatestatus"]', function () {
        var id = $(this).data('ticket-id');
        loadModal('/Ticket/UpdateStatusPartial', id);
    });

    actionModalEl.addEventListener('hidden.bs.modal', function () {
        actionModalContent.html('');
    });

    // Lightbox Overlay logic
    var overlayHtml = 
        '<div id="attachment-preview-overlay" class="attachment-preview-overlay d-none">' +
        '    <div class="preview-top-bar">' +
        '        <div class="preview-file-info">' +
        '            <span class="preview-file-icon"></span>' +
        '            <div class="preview-file-text">' +
        '                <span class="preview-file-name"></span>' +
        '                <span class="preview-file-details"></span>' +
        '            </div>' +
        '        </div>' +
        '        <div class="preview-actions">' +
        '            <a href="#" class="preview-action-btn download-btn" download title="Download"><i class="bi bi-download"></i></a>' +
        '            <button class="preview-action-btn close-btn" title="Close"><i class="bi bi-x-lg"></i></button>' +
        '        </div>' +
        '    </div>' +
        '    <button class="preview-nav-btn prev-btn"><i class="bi bi-chevron-left"></i></button>' +
        '    <button class="preview-nav-btn next-btn"><i class="bi bi-chevron-right"></i></button>' +
        '    <div class="preview-content-stage">' +
        '        <div class="preview-zoom-container">' +
        '            <img id="preview-image-element" class="preview-media d-none" src="" alt="" />' +
        '            <iframe id="preview-iframe-element" class="preview-media d-none" src="" frameborder="0"></iframe>' +
        '            <div id="preview-placeholder-element" class="attachment-preview-placeholder d-none">' +
        '                <i class="bi bi-file-earmark-x" style="font-size: 3rem; color: #94a3b8;"></i>' +
        '                <p class="mt-3 mb-0" style="color: #cbd5e1;">Preview not available for this file type.<br/><span style="color: #64748b; font-size: 0.85rem;">Use Download to view the file.</span></p>' +
        '            </div>' +
        '        </div>' +
        '    </div>' +
        '    <div class="preview-bottom-bar">' +
        '        <div class="preview-zoom-controls">' +
        '            <button class="zoom-btn zoom-out-btn" title="Zoom Out"><i class="bi bi-zoom-out"></i></button>' +
        '            <button class="zoom-btn zoom-in-btn" title="Zoom In"><i class="bi bi-zoom-in"></i></button>' +
        '        </div>' +
        '        <div class="preview-zoom-percentage">100 %</div>' +
        '    </div>' +
        '</div>';

    $('body').append(overlayHtml);

    var $overlay = $('#attachment-preview-overlay');
    var $img = $('#preview-image-element');
    var $iframe = $('#preview-iframe-element');
    var $placeholder = $('#preview-placeholder-element');
    var $zoomContainer = $('.preview-zoom-container');
    var $stage = $('.preview-content-stage');
    
    var attachments = [];
    var currentIndex = 0;
    var currentZoom = 1.0;
    
    // Drag/Pan states
    var isDragging = false;
    var startX = 0, startY = 0;
    var translateX = 0, translateY = 0;

    function getFileIconClass(fileName) {
        var lower = fileName.toLowerCase();
        if (lower.endsWith('.jpg') || lower.endsWith('.jpeg') || lower.endsWith('.png') || lower.endsWith('.gif') || lower.endsWith('.webp')) {
            return 'bi bi-image text-warning';
        }
        if (lower.endsWith('.pdf')) {
            return 'bi bi-file-earmark-pdf-fill text-danger';
        }
        return 'bi bi-file-earmark-text text-info';
    }

    function updatePreview() {
        if (currentIndex < 0 || currentIndex >= attachments.length) return;
        var item = attachments[currentIndex];
        
        // Reset Zoom & Pan
        currentZoom = 1.0;
        translateX = 0;
        translateY = 0;
        $zoomContainer.css('transform', 'translate(0px, 0px) scale(1)');
        $('.preview-zoom-percentage').text('100 %');

        // File Details
        $overlay.find('.preview-file-name').text(item.name);
        $overlay.find('.preview-file-details').text(item.typeString + ' · ' + item.size);
        
        // Icon
        var iconClass = getFileIconClass(item.name);
        $overlay.find('.preview-file-icon').html('<i class="' + iconClass + '" style="font-size: 1.5rem;"></i>');

        // Download link
        $overlay.find('.download-btn').attr('href', item.downloadUrl).attr('download', item.name);

        // Hide/Show navigation buttons based on count
        if (attachments.length <= 1) {
            $overlay.find('.prev-btn, .next-btn').addClass('d-none');
        } else {
            $overlay.find('.prev-btn, .next-btn').removeClass('d-none');
        }

        // Show/Hide Media
        if (item.type === 'image') {
            $iframe.addClass('d-none').attr('src', '');
            $placeholder.addClass('d-none');
            $img.removeClass('d-none').attr('src', item.previewUrl);
            $overlay.find('.preview-zoom-controls, .preview-zoom-percentage').removeClass('d-none');
        } else if (item.fileType === '.pdf') {
            $img.addClass('d-none').attr('src', '');
            $placeholder.addClass('d-none');
            $iframe.removeClass('d-none').attr('src', item.previewUrl);
            $overlay.find('.preview-zoom-controls, .preview-zoom-percentage').addClass('d-none');
        } else {
            $img.addClass('d-none').attr('src', '');
            $iframe.addClass('d-none').attr('src', '');
            $placeholder.removeClass('d-none');
            $overlay.find('.preview-zoom-controls, .preview-zoom-percentage').addClass('d-none');
        }
    }

    $(document).on('click', '.attachment-preview', function(e) {
        e.preventDefault();

        var $grid = $(this).closest('.attachment-grid');
        attachments = [];
        
        $grid.find('.attachment-preview').each(function() {
            var $card = $(this);
            var name = $card.data('filename');
            var size = $card.data('filesize');
            var previewUrl = $card.data('preview-url');
            var downloadUrl = $card.data('download-url');
            var fileType = ($card.data('filetype') || '').toLowerCase();
            
            var imageTypes = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.svg'];
            var type = (imageTypes.indexOf(fileType) >= 0) ? 'image' : 'document';
            var typeString = (type === 'image') ? 'image' : 'document';
            
            attachments.push({
                name: name,
                size: size,
                previewUrl: previewUrl,
                downloadUrl: downloadUrl,
                type: type,
                typeString: typeString,
                fileType: fileType
            });
        });

        currentIndex = $grid.find('.attachment-preview').index(this);
        if (currentIndex === -1) currentIndex = 0;
        
        updatePreview();
        $overlay.removeClass('d-none');
        $('body').css('overflow', 'hidden'); // Prevent background scroll
    });

    // Close Actions
    function closeOverlay() {
        $overlay.addClass('d-none');
        $img.attr('src', '');
        $iframe.attr('src', '');
        attachments = [];
        $('body').css('overflow', '');
    }

    $(document).on('click', '#attachment-preview-overlay .close-btn', closeOverlay);
    $(document).on('click', '#attachment-preview-overlay', function(e) {
        if ($(e.target).is('#attachment-preview-overlay') || $(e.target).is('.preview-content-stage')) {
            closeOverlay();
        }
    });

    // Navigation
    $(document).on('click', '#attachment-preview-overlay .next-btn', function(e) {
        e.stopPropagation();
        if (attachments.length === 0) return;
        currentIndex = (currentIndex + 1) % attachments.length;
        updatePreview();
    });

    $(document).on('click', '#attachment-preview-overlay .prev-btn', function(e) {
        e.stopPropagation();
        if (attachments.length === 0) return;
        currentIndex = (currentIndex - 1 + attachments.length) % attachments.length;
        updatePreview();
    });

    // Keyboard Shortcuts
    $(document).on('keydown', function(e) {
        if ($overlay.hasClass('d-none')) return;
        if (e.key === 'Escape') {
            closeOverlay();
        } else if (e.key === 'ArrowRight') {
            currentIndex = (currentIndex + 1) % attachments.length;
            updatePreview();
        } else if (e.key === 'ArrowLeft') {
            currentIndex = (currentIndex - 1 + attachments.length) % attachments.length;
            updatePreview();
        }
    });

    // Zoom Functions
    function setZoom(zoom) {
        currentZoom = Math.min(3.0, Math.max(0.25, zoom));
        var percent = Math.round(currentZoom * 100) + ' %';
        $('.preview-zoom-percentage').text(percent);
        $zoomContainer.css('transform', 'translate(' + translateX + 'px, ' + translateY + 'px) scale(' + currentZoom + ')');
    }

    $(document).on('click', '#attachment-preview-overlay .zoom-in-btn', function(e) {
        e.stopPropagation();
        setZoom(currentZoom + 0.25);
    });

    $(document).on('click', '#attachment-preview-overlay .zoom-out-btn', function(e) {
        e.stopPropagation();
        setZoom(currentZoom - 0.25);
    });

    // Pan / Drag Logic (Images only)
    $stage.on('mousedown', function(e) {
        if ($img.hasClass('d-none')) return; // Only pan images
        if ($(e.target).closest('.preview-nav-btn, .preview-top-bar, .preview-bottom-bar').length) return;
        
        isDragging = true;
        startX = e.clientX - translateX;
        startY = e.clientY - translateY;
        $stage.css('cursor', 'grabbing');
        e.preventDefault();
    });

    $(document).on('mousemove', function(e) {
        if (!isDragging) return;
        translateX = e.clientX - startX;
        translateY = e.clientY - startY;
        $zoomContainer.css('transform', 'translate(' + translateX + 'px, ' + translateY + 'px) scale(' + currentZoom + ')');
    });

    $(document).on('mouseup', function() {
        if (isDragging) {
            isDragging = false;
            $stage.css('cursor', 'grab');
        }
    });
});
