(function () {
    const apiBaseUrl = '/api/admin/certificates';

    const state = {
        selectedCourseId: '',
        search: '',
        status: 'All',
        searchTimer: null
    };

    document.addEventListener('DOMContentLoaded', function () {
        resetCertificateOverlayState();
        bindEvents();
        loadReview();
    });

    function resetCertificateOverlayState() {
        const modal = document.getElementById('adminCertificateConfirmModal');
        const toast = document.getElementById('adminCertificateToast');

        if (modal) {
            modal.hidden = true;
        }

        if (toast) {
            toast.hidden = true;
            toast.classList.remove('show');
        }

        document.body.classList.remove('certificate-modal-open');
    }

    function bindEvents() {
        const courseSelect = document.getElementById('adminCertificateCourseSelect');
        const statusSelect = document.getElementById('adminCertificateStatusSelect');
        const searchInput = document.getElementById('adminCertificateSearch');
        const refreshBtn = document.getElementById('adminCertificateRefreshBtn');
        const generateBtn = document.getElementById('adminGenerateCertificatesBtn');

        if (courseSelect) {
            courseSelect.addEventListener('change', function () {
                state.selectedCourseId = this.value;
                loadReview();
            });
        }

        if (statusSelect) {
            statusSelect.addEventListener('change', function () {
                state.status = this.value || 'All';
                loadReview();
            });
        }

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                window.clearTimeout(state.searchTimer);

                state.searchTimer = window.setTimeout(function () {
                    state.search = searchInput.value.trim();
                    loadReview();
                }, 350);
            });
        }

        if (refreshBtn) {
            refreshBtn.addEventListener('click', function () {
                loadReview();
            });
        }
        if (generateBtn) {
            generateBtn.addEventListener('click', function () {
                generateCertificates(generateBtn);
            });
        }
    }

    async function loadReview() {
        const tableBody = document.getElementById('adminCertificateTableBody');
        const emptyState = document.getElementById('adminCertificateEmptyState');
        const tableWrap = document.getElementById('adminCertificateTableWrap');

        if (tableBody) {
            tableBody.innerHTML = getLoadingRow();
        }

        if (tableWrap) {
            tableWrap.hidden = false;
        }

        if (emptyState) {
            emptyState.hidden = true;
        }

        try {
            const query = buildQuery();
            const response = await fetch(`${apiBaseUrl}/review${query}`);
            const result = await response.json();

            if (!response.ok || !result.success || !result.data) {
                renderEmpty(result.message || 'Unable to load certificate records.');
                showToast(result.message || 'Unable to load certificate records.');
                return;
            }

            renderReview(result.data);

        } catch (error) {
            renderEmpty('Unable to load certificate records.');
            showToast('Unable to load certificate records.');
        }
    }

    function buildQuery() {
        const params = new URLSearchParams();

        if (state.selectedCourseId) {
            params.append('courseId', state.selectedCourseId);
        }

        if (state.search) {
            params.append('search', state.search);
        }

        if (state.status && state.status !== 'All') {
            params.append('status', state.status);
        }

        const query = params.toString();

        return query ? `?${query}` : '';
    }

    function renderReview(data) {
        renderFilters(data);
        renderSummary(data.summary || {});
        renderCourseMeta(data);
        renderTable(data);
    }

    function renderFilters(data) {
        const courseSelect = document.getElementById('adminCertificateCourseSelect');
        const statusSelect = document.getElementById('adminCertificateStatusSelect');

        const courses = data.filters && data.filters.courses
            ? data.filters.courses
            : [];

        const statuses = data.filters && data.filters.certificateStatuses
            ? data.filters.certificateStatuses
            : ['All', 'In Progress', 'Not Applied', 'Pending', 'Approved', 'Rejected'];

        if (courseSelect) {
            const previousValue = state.selectedCourseId || String(data.selectedCourseId || '');

            courseSelect.innerHTML = courses.length
                ? courses.map(course => {
                    const selected = String(course.courseId) === String(previousValue) ? 'selected' : '';
                    return `<option value="${course.courseId}" ${selected}>${escapeHtml(course.courseTitle)}</option>`;
                }).join('')
                : '<option value="">No certificate courses found</option>';

            if (!state.selectedCourseId && data.selectedCourseId) {
                state.selectedCourseId = String(data.selectedCourseId);
                courseSelect.value = state.selectedCourseId;
            }
        }

        if (statusSelect) {
            const currentStatus = state.status || 'All';

            statusSelect.innerHTML = statuses.map(status => {
                const selected = status === currentStatus ? 'selected' : '';
                return `<option value="${escapeHtml(status)}" ${selected}>${escapeHtml(status)}</option>`;
            }).join('');

            statusSelect.value = currentStatus;
        }
     
    }

    function renderSummary(summary) {
        setText('adminCertificateClassAverage', `${formatNumber(summary.overallClassAverage)}%`);
        setText('adminCertificatePendingCount', summary.pendingRequests || 0);
        setText('adminCertificateApprovedCount', summary.approved || 0);
        setText('adminCertificateAtRiskCount', summary.atRiskStudents || 0);
    }

    function renderCourseMeta(data) {
        const studentCountEl = document.getElementById('adminCertificateStudentCount');
        const courseMetaEl = document.getElementById('adminCertificateCourseMeta');

        const students = data.students || [];

        if (studentCountEl) {
            studentCountEl.textContent = `${students.length} student(s)`;
        }

        if (courseMetaEl) {
            const courseStatus = data.isCourseEnded ? 'Course completed' : 'Course in progress';
            courseMetaEl.textContent = `${data.selectedCourseTitle || 'Selected course'} • ${courseStatus}`;
        }
    }

    function renderTable(data) {
        const tableHead = document.getElementById('adminCertificateTableHead');
        const tableBody = document.getElementById('adminCertificateTableBody');
        const tableWrap = document.getElementById('adminCertificateTableWrap');
        const emptyState = document.getElementById('adminCertificateEmptyState');

        if (!tableHead || !tableBody || !tableWrap) return;

        const assignments = data.assignments || [];
        const students = data.students || [];

        updateTableColumnClass(tableWrap, assignments.length);

        tableHead.innerHTML = renderTableHead(assignments);

        if (!students.length) {
            tableBody.innerHTML = '';
            tableWrap.hidden = true;

            if (emptyState) {
                emptyState.hidden = false;
            }

            return;
        }

        tableWrap.hidden = false;

        if (emptyState) {
            emptyState.hidden = true;
        }

        tableBody.innerHTML = students.map(student => renderStudentRow(student, assignments)).join('');

        bindActionButtons();
        bindDeliveryModeSelects();
    }

    function renderTableHead(assignments) {
        const assignmentHeaders = assignments.map((assignment, index) => {
            return `
                <div title="${escapeHtml(assignment.assignmentTitle)}">
                    A${index + 1}
                    <br />
                    <span class="trainer-gradebook-sub">/${assignment.totalMarks}</span>
                </div>
            `;
        }).join('');

        return `
            <div title="Student">Student</div>
            ${assignmentHeaders}
            <div title="Overall Percentage">Overall %</div>
            <div title="Student Standing">Standing</div>
            <div title="Delivery Mode">Mode</div>
            <div title="Certificate Status">Certificate</div>
        `;
    }

    function renderStudentRow(student, assignments) {
        const cells = alignAssignmentCells(student.assignmentCells || [], assignments);
        const assignmentCellsHtml = cells.map(cell => renderAssignmentCell(cell)).join('');

        return `
            <div class="dashboard-table-row trainer-gradebook-table-row">
                <div class="dashboard-cell-strong" title="${escapeHtml(student.studentName)}">
                    ${escapeHtml(student.studentName)}
                </div>

                ${assignmentCellsHtml}

                <div class="dashboard-cell-strong ${escapeHtml(student.overallCssClass || '')}" title="${escapeHtml(student.overallText)}">
                    ${escapeHtml(student.overallText)}
                </div>

                <div title="${escapeHtml(student.standingText)}">
                    <span class="${escapeHtml(student.standingCssClass || 'pill trainer-grade-pill-warn')}">
                        ${escapeHtml(student.standingText)}
                    </span>
                </div>

                <div title="Delivery Mode">
                    ${renderDeliveryMode(student)}
                </div>

                <div title="${escapeHtml(student.certificateStatusText)}">
                    ${renderCertificateAction(student)}
                </div>
            </div>
        `;
    }

    function alignAssignmentCells(cells, assignments) {
        return assignments.map(assignment => {
            const found = cells.find(cell => Number(cell.assignmentId) === Number(assignment.assignmentId));

            if (found) {
                return found;
            }

            return {
                assignmentId: assignment.assignmentId,
                assignmentTitle: assignment.assignmentTitle,
                totalMarks: assignment.totalMarks,
                valueText: '—',
                valueCssClass: 'trainer-score-muted',
                statusCssClass: 'pill trainer-grade-pill-warn',
                isScore: false
            };
        });
    }

    function renderAssignmentCell(cell) {
        const titleText = `${cell.assignmentTitle || 'Assignment'} - ${cell.status || cell.valueText}`;

        if (cell.isScore) {
            return `
                <div class="${escapeHtml(cell.valueCssClass || '')}" title="${escapeHtml(titleText)}">
                    ${escapeHtml(cell.valueText)}
                </div>
            `;
        }

        return `
            <div title="${escapeHtml(titleText)}">
                <span class="${escapeHtml(cell.statusCssClass || 'pill trainer-grade-pill-warn')}">
                    ${escapeHtml(cell.valueText)}
                </span>
            </div>
        `;
    }

    function renderDeliveryMode(student) {
        const deliveryMode = student.deliveryMode || 'Onsite';
        const enrollmentId = student.enrollmentId || 0;
        const canUpdate = student.canUpdateDeliveryMode === true;

        if (!canUpdate) {
            return `
            <span class="pill trainer-grade-pill-warn" title="Delivery mode is locked because certificate has been generated">
                ${escapeHtml(deliveryMode)}
            </span>
        `;
        }

        return `
        <select class="course-input"
                data-delivery-mode-select="true"
                data-enrollment-id="${enrollmentId}"
                data-current-mode="${escapeHtml(deliveryMode)}"
                title="Change delivery mode">
            <option value="Onsite" ${deliveryMode === 'Onsite' ? 'selected' : ''}>Onsite</option>
            <option value="Online" ${deliveryMode === 'Online' ? 'selected' : ''}>Online</option>
        </select>
    `;
    }

    function renderCertificateAction(student) {
        if (student.canApprove && student.canReject && student.certificateRequestId) {
            return `
                <div class="trainer-gradebook-certificate-actions">
                    <button type="button"
                            class="dashboard-btn dashboard-btn-sm trainer-certificate-approve-btn"
                            data-request-id="${student.certificateRequestId}"
                            data-student-name="${escapeHtml(student.studentName)}"
                            data-course-title="${escapeHtml(student.courseTitle)}">
                        <i class="bi bi-check-circle"></i>
                        Approve
                    </button>

                    <button type="button"
                            class="dashboard-btn dashboard-btn-sm dashboard-btn-outline trainer-certificate-reject-btn"
                            data-request-id="${student.certificateRequestId}"
                            data-student-name="${escapeHtml(student.studentName)}"
                            data-course-title="${escapeHtml(student.courseTitle)}">
                        <i class="bi bi-x-circle"></i>
                        Reject
                    </button>
                </div>
            `;
        }

        return `
            <span class="${escapeHtml(student.certificateStatusCssClass || 'pill trainer-grade-pill-warn')}">
                ${escapeHtml(student.certificateStatusText)}
            </span>
        `;
    }

    function bindActionButtons() {
        const approveButtons = document.querySelectorAll('.trainer-certificate-approve-btn');
        const rejectButtons = document.querySelectorAll('.trainer-certificate-reject-btn');

        approveButtons.forEach(button => {
            button.addEventListener('click', function () {
                const requestId = this.getAttribute('data-request-id');
                const studentName = this.getAttribute('data-student-name') || 'this student';
                const courseTitle = this.getAttribute('data-course-title') || 'this course';

                confirmAndSubmit({
                    action: 'approve',
                    requestId: requestId,
                    title: 'Approve Certificate',
                    message: `Are you sure you want to approve certificate request for ${studentName} in ${courseTitle}?`,
                    confirmText: 'Approve'
                });
            });
        });

        rejectButtons.forEach(button => {
            button.addEventListener('click', function () {
                const requestId = this.getAttribute('data-request-id');
                const studentName = this.getAttribute('data-student-name') || 'this student';
                const courseTitle = this.getAttribute('data-course-title') || 'this course';

                confirmAndSubmit({
                    action: 'reject',
                    requestId: requestId,
                    title: 'Reject Certificate',
                    message: `Are you sure you want to reject certificate request for ${studentName} in ${courseTitle}?`,
                    confirmText: 'Reject'
                });
            });
        });
    }

    function bindDeliveryModeSelects() {
        const selects = document.querySelectorAll('select[data-delivery-mode-select="true"]');

        selects.forEach(select => {
            select.addEventListener('change', function () {
                const enrollmentId = this.getAttribute('data-enrollment-id');
                const oldMode = this.getAttribute('data-current-mode') || 'Onsite';
                const newMode = this.value;

                updateDeliveryMode(this, enrollmentId, oldMode, newMode);
            });
        });
    }

    async function updateDeliveryMode(selectElement, enrollmentId, oldMode, newMode) {
        if (!enrollmentId || oldMode === newMode) {
            return;
        }

        const confirmed = await showConfirmModal({
            title: 'Update Delivery Mode',
            message: `Are you sure you want to change delivery mode from ${oldMode} to ${newMode}?`,
            confirmText: 'Update',
            icon: 'bi-arrow-repeat'
        });

        if (!confirmed) {
            selectElement.value = oldMode;
            return;
        }

        selectElement.disabled = true;

        try {
            const response = await fetch(`${apiBaseUrl}/enrollment/${enrollmentId}/delivery-mode`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    deliveryMode: newMode
                })
            });

            const result = await response.json();

            if (!response.ok || !result.success) {
                selectElement.value = oldMode;
                showToast(result.message || 'Unable to update delivery mode.');
                return;
            }

            showToast(result.message || 'Delivery mode updated successfully.');
            await loadReview();

        } catch (error) {
            selectElement.value = oldMode;
            showToast('Unable to update delivery mode.');
        } finally {
            selectElement.disabled = false;
        }
    }

    async function confirmAndSubmit(options) {
        if (!options.requestId) return;

        const confirmed = await showConfirmModal({
            title: options.title,
            message: options.message,
            confirmText: options.confirmText,
            icon: options.action === 'reject' ? 'bi-x-circle' : 'bi-check-circle'
        });

        if (!confirmed) return;

        try {
            const response = await fetch(`${apiBaseUrl}/${options.requestId}/${options.action}`, {
                method: 'POST'
            });

            const result = await response.json();

            if (!response.ok || !result.success) {
                showToast(result.message || 'Unable to update certificate request.');
                return;
            }

            showToast(result.message || 'Certificate request updated successfully.');
            await loadReview();

        } catch (error) {
            showToast('Unable to update certificate request.');
        }
    }
    async function generateCertificates(button) {
        const courseSelect = document.getElementById('adminCertificateCourseSelect');
        const courseId = state.selectedCourseId || (courseSelect ? courseSelect.value : '');

        if (!courseId) {
            showToast('Please select a course first.');
            return;
        }

        const confirmed = await showConfirmModal({
            title: 'Generate Certificates',
            message: 'This will generate certificate numbers for approved students of the selected course. Continue?',
            confirmText: 'Generate',
            icon: 'bi-patch-check'
        });

        if (!confirmed) return;

        const oldText = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<i class="bi bi-hourglass-split"></i> Generating...';

        try {
            const response = await fetch(`${apiBaseUrl}/course/${courseId}/generate`, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json'
                }
            });

            const rawText = await response.text();

            let result = null;

            try {
                result = rawText ? JSON.parse(rawText) : null;
            } catch (jsonError) {
                console.error('Generate certificates raw response:', rawText);
                showToast('Generate request failed. Backend returned an invalid response.');
                return;
            }

            if (!response.ok || !result || !result.success) {
                let message = result && result.message
                    ? result.message
                    : 'Unable to generate certificates.';

                if (result && result.errors && result.errors.length > 0) {
                    message += ' ' + result.errors.join(' ');
                }

                console.error('Generate certificates failed:', {
                    status: response.status,
                    result: result
                });

                showToast(message);
                return;
            }

            showToast(result.message || 'Certificates generated successfully.');
            await loadReview();

        } catch (error) {
            console.error('Generate certificates request error:', error);
            showToast('Unable to generate certificates. Please check console.');
        } finally {
            button.disabled = false;
            button.innerHTML = oldText;
        }
    }
    //async function generateCertificates(button) {
    //    const courseId = state.selectedCourseId;

    //    if (!courseId) {
    //        showToast('Please select a course first.');
    //        return;
    //    }

    //    const confirmed = await showConfirmModal({
    //        title: 'Generate Certificates',
    //        message: 'This will generate certificate numbers for approved students of the selected course. Continue?',
    //        confirmText: 'Generate',
    //        icon: 'bi-patch-check'
    //    });

    //    if (!confirmed) return;

    //    const oldText = button.innerHTML;
    //    button.disabled = true;
    //    button.innerHTML = '<i class="bi bi-hourglass-split"></i> Generating...';

    //    try {
    //        const response = await fetch(`${apiBaseUrl}/course/${courseId}/generate`, {
    //            method: 'POST'
    //        });

    //        const result = await response.json();

    //        if (!response.ok || !result.success) {
    //            let message = result.message || 'Unable to generate certificates.';

    //            if (result.errors && result.errors.length > 0) {
    //                message += ' ' + result.errors.join(' ');
    //            }

    //            showToast(message);
    //            return;
    //        }

    //        showToast(result.message || 'Certificates generated successfully.');
    //        await loadReview();

    //    } catch (error) {
    //        showToast('Unable to generate certificates.');
    //    } finally {
    //        button.disabled = false;
    //        button.innerHTML = oldText;
    //    }
    //}
    
    function showConfirmModal(options) {
        return new Promise(resolve => {
            const modal = document.getElementById('adminCertificateConfirmModal');
            const title = document.getElementById('adminCertificateConfirmTitle');
            const message = document.getElementById('adminCertificateConfirmMessage');
            const confirmBtn = document.getElementById('adminCertificateConfirmBtn');
            const cancelBtn = document.getElementById('adminCertificateCancelBtn');
            const icon = modal ? modal.querySelector('.certificate-modal-icon i') : null;

            if (!modal || !title || !message || !confirmBtn || !cancelBtn) {
                resolve(false);
                return;
            }

            title.textContent = options.title || 'Confirm Action';
            message.textContent = options.message || 'Are you sure?';
            confirmBtn.textContent = options.confirmText || 'Confirm';

            if (icon) {
                icon.className = `bi ${options.icon || 'bi-patch-question'}`;
            }

            modal.hidden = false;
            document.body.classList.add('certificate-modal-open');

            const close = value => {
                modal.hidden = true;
                document.body.classList.remove('certificate-modal-open');

                confirmBtn.removeEventListener('click', onConfirm);
                cancelBtn.removeEventListener('click', onCancel);
                modal.removeEventListener('click', onBackdrop);

                resolve(value);
            };

            const onConfirm = () => close(true);
            const onCancel = () => close(false);
            const onBackdrop = event => {
                if (event.target === modal) {
                    close(false);
                }
            };

            confirmBtn.addEventListener('click', onConfirm);
            cancelBtn.addEventListener('click', onCancel);
            modal.addEventListener('click', onBackdrop);
        });
    }

    function showToast(message) {
        const toast = document.getElementById('adminCertificateToast');
        const messageEl = document.getElementById('adminCertificateToastMessage');

        if (!toast || !messageEl) return;

        messageEl.textContent = message || 'Action completed.';
        toast.hidden = false;
        toast.classList.add('show');

        window.clearTimeout(showToast.timer);
        showToast.timer = window.setTimeout(function () {
            toast.classList.remove('show');
            toast.hidden = true;
        }, 2500);
    }

    function renderEmpty(message) {
        const tableWrap = document.getElementById('adminCertificateTableWrap');
        const emptyState = document.getElementById('adminCertificateEmptyState');

        if (tableWrap) {
            tableWrap.hidden = true;
        }

        if (emptyState) {
            emptyState.hidden = false;

            const paragraph = emptyState.querySelector('p');
            if (paragraph) {
                paragraph.textContent = message || 'No records found for the selected course.';
            }
        }

        setText('adminCertificateStudentCount', '0 student(s)');
    }

    function getLoadingRow() {
        return `
            <div class="dashboard-table-row trainer-gradebook-table-row">
                <div class="dashboard-cell-strong">Loading...</div>
                <div>—</div>
                <div>—</div>
                <div>—</div>
                <div>—</div>
            </div>
        `;
    }

    function updateTableColumnClass(tableWrap, assignmentCount) {
        const maxColumnClass = 12;

        for (let i = 0; i <= maxColumnClass; i++) {
            tableWrap.classList.remove(`certificate-review-cols-${i}`);
        }

        const safeCount = Math.min(Math.max(assignmentCount, 0), maxColumnClass);
        tableWrap.classList.add(`certificate-review-cols-${safeCount}`);
    }

    function formatNumber(value) {
        const number = Number(value || 0);

        if (Number.isInteger(number)) {
            return String(number);
        }

        return number.toFixed(1);
    }

    function setText(id, value) {
        const element = document.getElementById(id);

        if (element) {
            element.textContent = value;
        }
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) return '';

        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
})();