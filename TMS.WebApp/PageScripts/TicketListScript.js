$(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;
    const PageSize = 10;
    let isInitializing = true;
    let listUrl = $('#searchForm').data('url');
    let sortColumn = $('#SortColumn').val() || 'CreatedOn';
    let sortDirection = $('#SortDirection').val() || 'DESC';
    let assigning = false;

    $('#StatusId').select2({
        placeholder: 'All Statuses',
        allowClear: true,
        width: '100%'
    });

    $('#PriorityId').select2({
        placeholder: 'All Priorities',
        allowClear: true,
        width: '100%'
    });

    $('#CategoryId').select2({
        placeholder: 'All Categories',
        allowClear: true,
        width: '100%'
    });

    RestoreFilterState();
    isInitializing = false;
    $('#SortColumn').val(sortColumn);
    $('#SortDirection').val(sortDirection);
    FetchData();

    function initAssignSelect2() {
        $('.assign-user').select2({
            width: '100%',
            placeholder: '-- Unassigned --',
            allowClear: false,
            dropdownParent: $('body')
        });
    }

    function FetchData() {
        $.ajax({
            url: listUrl,
            type: 'POST',
            data: $('#searchForm').serialize(),
            success: function (result) {
                $('#resultContainer').html(result);
                initAssignSelect2();
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
                $('#resultContainer').html(
                    '<div class="alert alert-danger m-3">Error loading data: ' + error + '</div>'
                );
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
            $('#SearchTerm').val('');
            $('#StatusId').val('').trigger('change');
            $('#PriorityId').val('').trigger('change');
            $('#CategoryId').val('').trigger('change');
            $('#DateFrom').val('');
            $('#DateTo').val('');
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

    $('#DateFrom, #DateTo').on('change', function () {
        $('#PageNumber').val(1);
        PageStart = 1;
        if (!isInitializing) {
            FetchData();
        }
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

    $(document).on('change', '.assign-user', function () {
        if (assigning) return;
        var sel = $(this);
        var ticketId = sel.data('ticketid');
        var assignedToUserId = sel.val();
        assigning = true;
        $.post('/Ticket/Assign', { TicketId: ticketId, AssignedToUserId: assignedToUserId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() })
            .done(function (r) {
                if (r.success) {
                    showToast(r.message, 'info');
                    setTimeout(function () { FetchData(); }, 800);
                } else {
                    showToast(r.message || 'Error assigning ticket', 'error');
                    sel.val(sel.data('orig')).trigger('change');
                }
            })
            .fail(function () { showToast('Error assigning ticket', 'error'); sel.val(sel.data('orig')).trigger('change'); })
            .always(function () { assigning = false; });
    });

    $(document).on('click', '.delete-ticket', function () {
        var btn = $(this);
        var ticketId = btn.data('ticketid');
        var title = btn.data('title');
        showConfirm('Delete Ticket', 'Are you sure you want to delete ticket "' + title + '"? This cannot be undone.', 'Delete', function () {
            $.post('/Ticket/Delete', { id: ticketId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() })
                .done(function (r) {
                    if (r.success) {
                        showToast(r.message, 'success');
                        setTimeout(function () { FetchData(); }, 800);
                    } else {
                        showToast(r.message || 'Error deleting ticket', 'error');
                    }
                })
                .fail(function () { showToast('Error deleting ticket', 'error'); });
        });
    });

    function SaveFilterState() {
        let state = {
            pageNumber: $('#PageNumber').val(),
            searchTerm: $('#SearchTerm').val().trim(),
            statusId: $('#StatusId').val(),
            priorityId: $('#PriorityId').val(),
            categoryId: $('#CategoryId').val(),
            dateFrom: $('#DateFrom').val(),
            dateTo: $('#DateTo').val(),
            sortColumn: sortColumn,
            sortDirection: sortDirection
        };
        sessionStorage.setItem('TicketListFilters', JSON.stringify(state));
    }

    function RestoreFilterState() {
        let saved = sessionStorage.getItem('TicketListFilters');
        if (saved) {
            try {
                var state = JSON.parse(saved);
            } catch (e) {
                sessionStorage.removeItem('TicketListFilters');
                return;
            }
            if (state.pageNumber) $('#PageNumber').val(state.pageNumber);
            if (state.searchTerm) $('#SearchTerm').val(state.searchTerm);
            if (state.statusId) $('#StatusId').val(state.statusId).trigger('change');
            if (state.priorityId) $('#PriorityId').val(state.priorityId).trigger('change');
            if (state.categoryId) $('#CategoryId').val(state.categoryId).trigger('change');
            if (state.dateFrom) $('#DateFrom').val(state.dateFrom);
            if (state.dateTo) $('#DateTo').val(state.dateTo);
            if (state.sortColumn) sortColumn = state.sortColumn;
            if (state.sortDirection) sortDirection = state.sortDirection;
        }
        updateSortIcons();
    }
});
