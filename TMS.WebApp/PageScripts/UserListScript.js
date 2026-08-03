$(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;
    const PageSize = 10;
    let isInitializing = true;
    let listUrl = $('#searchForm').data('url');
    let sortColumn = $('#SortColumn').val() || 'CreatedOn';
    let sortDirection = $('#SortDirection').val() || 'DESC';

    $('#SearchName').select2({
        ajax: {
            url: '/User/UserSearch',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { term: params.term };
            },
            processResults: function (data) {
                return { results: data };
            }
        },
        minimumInputLength: 2,
        placeholder: 'Search by name or email...',
        allowClear: true,
        tags: true,
        width: '100%'
    }).on('select2:select', function (e) {
        var d = e.params.data;
        $('#SearchTerm').val(d.text);
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    }).on('select2:clear', function () {
        $('#SearchTerm').val('');
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    });

    $('#RoleId').select2({
        placeholder: 'All Roles',
        allowClear: true,
        width: '100%'
    });

    RestoreFilterState();
    isInitializing = false;
    $('#SortColumn').val(sortColumn);
    $('#SortDirection').val(sortDirection);
    FetchData();

    function FetchData() {
        $.ajax({
            url: listUrl,
            type: 'POST',
            data: $('#searchForm').serialize(),
            success: function (result) {
                if (result && result.success === false) {
                    showToast(result.message || 'Something went wrong. Please try again.', 'error');
                    return;
                }
                $('#resultContainer').html(result);
                updateSortIcons();

                TotalRows = parseInt($('#TotalRows').val(), 10);
                LoadButton();

                $('#PageItems').empty();

                let page = Number($('#PageNumber').val());
                let size = PageSize;

                if (TotalRows === 0) {
                    $('#PageItems').text('0 out of 0');
                } else {
                    let start = (page - 1) * size + 1;
                    let end = (page - 1) * size + size;
                    if (end > TotalRows || end <= 0)
                        end = TotalRows;

                    $('#PageItems').text(start + ' to ' + end + ' out of ' + TotalRows);
                }

                SaveFilterState();
            },
            error: function (xhr, status, error) {
                showToast('Something went wrong. Please try again.', 'error');
            }
        });
    }

    function LoadButton() {
        let buttonlist = $('#buttonlist');
        let page = Number($('#PageNumber').val());

        buttonlist.empty();

        let TotalPages = Math.ceil(TotalRows / PageSize);

        if (page > 1) {
            $('#prevButton').prop('disabled', false).removeClass('disabled');
        } else {
            $('#prevButton').prop('disabled', true).addClass('disabled');
        }

        for (let i = PageStart; i <= TotalPages && i <= PageStart + (PageWindow - 1); i++) {
            if (i == page) {
                buttonlist.append(
                    '<button type="button" class="btn btn-sm page-btn btn-success" data-page="' + i + '">' + i + '</button>'
                );
            } else {
                buttonlist.append(
                    '<button type="button" class="btn btn-sm page-btn btn-outline-secondary" data-page="' + i + '">' + i + '</button>'
                );
            }
        }

        if (page < TotalPages) {
            $('#nextButton').prop('disabled', false).removeClass('disabled');
        } else {
            $('#nextButton').prop('disabled', true).addClass('disabled');
        }
    }

    $('#searchForm').submit(function (e) {
        e.preventDefault();
        FetchData();
    });

    $('#searchBtn').click(function () {
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    });

    $('#searchForm').on('reset', function () {
        setTimeout(function () {
            $('#SearchName').val(null).trigger('change');
            $('#SearchTerm').val('');
            $('#RoleId').val('').trigger('change');
            PageStart = 1;
            $('#PageNumber').val(1);
            sortColumn = 'CreatedOn';
            sortDirection = 'DESC';
            $('#SortColumn').val(sortColumn);
            $('#SortDirection').val(sortDirection);
            updateSortIcons();
            FetchData();
        }, 0);
    });

    $('#buttonlist').on('click', '.page-btn', function () {
        let page = Number($(this).data('page'));
        $('#PageNumber').val(page);
        FetchData();
    });

    $('#prevButton').click(function () {
        let num = Number($('#PageNumber').val());

        if (num % PageWindow == 1) {
            PageStart = PageStart - PageWindow;
            LoadButton();
        }

        $('#PageNumber').val(num - 1);
        FetchData();
    });

    $('#nextButton').click(function () {
        let num = Number($('#PageNumber').val());

        if (num % PageWindow == 0) {
            PageStart = PageStart + PageWindow;
            LoadButton();
        }

        $('#PageNumber').val(num + 1);
        FetchData();
    });

    function updateSortIcons() {
        $('.sortable .sort-icon').each(function () {
            let col = $(this).closest('.sortable').data('column');
            if (col === sortColumn) {
                $(this).text(sortDirection === 'ASC' ? ' \u25B2' : ' \u25BC');
            } else {
                $(this).text(' \u2195');
            }
        });
    }

    $(document).on('click', '.sortable', function () {
        let col = $(this).data('column');
        if (sortColumn === col) {
            sortDirection = sortDirection === 'ASC' ? 'DESC' : 'ASC';
        } else {
            sortColumn = col;
            sortDirection = 'ASC';
        }
        updateSortIcons();
        $('#SortColumn').val(sortColumn);
        $('#SortDirection').val(sortDirection);
        $('#PageNumber').val(1);
        PageStart = 1;
        if (!isInitializing) FetchData();
    });

    $(document).on('change', '.change-role', function () {
        var sel = $(this);
        var userId = sel.data('userid');
        var roleId = sel.val();
        sel.prop('disabled', true);
        $.post('/User/ChangeRole', { userId: userId, roleId: roleId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() })
            .done(function (r) {
                if (r.success) {
                    showToast(r.message, 'info');
                    sel.data('orig', roleId).attr('data-orig', roleId);
                } else {
                    showToast(r.error, 'error');
                    sel.val(sel.data('orig'));
                }
            })
            .fail(function () {
                showToast('Error changing role', 'error');
                sel.val(sel.data('orig'));
            })
            .always(function () {
                sel.prop('disabled', false);
            });
    });

    $(document).on('change', '.approval-radio', function () {
        var seg = $(this).closest('.approval-seg');
        var userId = seg.data('userid');
        var approval = $(this).val();
        seg.addClass('loading');
        $.post('/User/SetApproval', { userId: userId, approval: approval, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() })
            .done(function (r) {
                if (r.success) {
                    seg.data('orig', approval).attr('data-orig', approval);
                    var toastType = 'success';
                    if (approval === 'rejected') {
                        toastType = 'error';
                    } else if (approval === 'awaiting') {
                        toastType = 'warning';
                    }
                    showToast(r.message, toastType);
                } else {
                    showToast(r.error, 'error');
                    seg.find('input[value="' + seg.data('orig') + '"]').prop('checked', true);
                }
            })
            .fail(function () {
                showToast('Error updating approval', 'error');
                seg.find('input[value="' + seg.data('orig') + '"]').prop('checked', true);
            })
            .always(function () {
                seg.removeClass('loading');
            });
    });

    $(document).on('click', '.delete-user', function () {
        var btn = $(this);
        var userId = btn.data('userid');
        var name = btn.data('name');
        showConfirm('Delete User', 'Are you sure you want to delete "' + name + '"? They will be deactivated and unable to log in. Their history will be preserved.', 'Delete', function () {
            $.post('/User/Delete', { userId: userId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() })
                .done(function (r) {
                    if (r.success) { showToast(r.message, 'success'); setTimeout(function () { FetchData(); }, 800); }
                    else { showToast(r.error, 'error'); }
                })
                .fail(function () { showToast('Error deleting user', 'error'); });
        });
    });

    function SaveFilterState() {
        let state = {
            pageNumber: $('#PageNumber').val(),
            searchTerm: $('#SearchTerm').val().trim(),
            roleId: $('#RoleId').val(),
            sortColumn: sortColumn,
            sortDirection: sortDirection
        };
        sessionStorage.setItem('UserListFilters', JSON.stringify(state));
    }

    function RestoreFilterState() {
        let saved = sessionStorage.getItem('UserListFilters');
        if (saved) {
            try {
                var state = JSON.parse(saved);
            } catch (e) {
                sessionStorage.removeItem('UserListFilters');
                return;
            }
            if (state.pageNumber) $('#PageNumber').val(state.pageNumber);
            if (state.searchTerm) {
                $('#SearchTerm').val(state.searchTerm);
                var searchOption = new Option(state.searchTerm, state.searchTerm, true, true);
                $('#SearchName').append(searchOption).trigger('change');
            }
            if (state.roleId) $('#RoleId').val(state.roleId).trigger('change');
            if (state.sortColumn) sortColumn = state.sortColumn;
            if (state.sortDirection) sortDirection = state.sortDirection;
        }
        updateSortIcons();
    }
});
