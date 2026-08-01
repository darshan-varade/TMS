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
});
