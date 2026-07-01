(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const page = document.querySelector('.trainer-submissions-page');

        if (!page) {
            return;
        }

        const elements = {
            total: document.getElementById('trainerSubmissionTotal'),
            graded: document.getElementById('trainerSubmissionGraded'),
            pending: document.getElementById('trainerSubmissionPending'),
            missing: document.getElementById('trainerSubmissionMissing'),

            search: document.getElementById('trainerSubmissionSearch'),
            courseFilter: document.getElementById('trainerSubmissionCourseFilter'),
            moduleFilter: document.getElementById('trainerSubmissionModuleFilter'),
            statusFilter: document.getElementById('trainerSubmissionStatusFilter'),

            recordCount: document.getElementById('trainerSubmissionRecordCount'),
            rows: document.getElementById('trainerSubmissionRows'),
            empty: document.getElementById('trainerSubmissionEmpty'),
            error: document.getElementById('trainerSubmissionError'),

            modalElement: document.getElementById('trainerMarkZeroModal'),
            modalText: document.getElementById('trainerMarkZeroModalText'),
            confirmBtn: document.getElementById('trainerMarkZeroConfirmBtn'),
            cancelBtn: document.getElementById('trainerMarkZeroCancelBtn'),

            toast: document.getElementById('trainerSubmissionToast'),
            toastTitle: document.getElementById('trainerSubmissionToastTitle'),
            toastMessage: document.getElementById('trainerSubmissionToastMessage'),
            toastIcon: document.getElementById('trainerSubmissionToastIcon'),
            toastClose: document.getElementById('trainerSubmissionToastClose')
        };

        const state = {
            modules: [],
            searchTimer: null,
            selectedAssignmentId: 0,
            selectedStudentId: 0,
            toastTimer: null
        };

        bindTrainerSubmissionEvents();
        loadTrainerAssignmentSubmissions();

        function bindTrainerSubmissionEvents() {
            if (elements.search) {
                elements.search.addEventListener('input', function () {
                    clearTimeout(state.searchTimer);

                    state.searchTimer = setTimeout(function () {
                        loadTrainerAssignmentSubmissions();
                    }, 350);
                });
            }

            if (elements.courseFilter) {
                elements.courseFilter.addEventListener('change', function () {
                    populateTrainerSubmissionModules();
                    loadTrainerAssignmentSubmissions();
                });
            }

            if (elements.moduleFilter) {
                elements.moduleFilter.addEventListener('change', function () {
                    loadTrainerAssignmentSubmissions();
                });
            }

            if (elements.statusFilter) {
                elements.statusFilter.addEventListener('change', function () {
                    loadTrainerAssignmentSubmissions();
                });
            }

            if (elements.rows) {
                elements.rows.addEventListener('click', function (e) {
                    const btn = e.target.closest('.trainer-mark-zero-btn');

                    if (!btn) {
                        return;
                    }

                    const assignmentId = parseInt(btn.getAttribute('data-assignment-id') || '0', 10);
                    const studentId = parseInt(btn.getAttribute('data-student-id') || '0', 10);

                    openMarkZeroModal(assignmentId, studentId);
                });
            }

            if (elements.confirmBtn) {
                elements.confirmBtn.addEventListener('click', function () {
                    markMissingSubmissionZero();
                });
            }

            if (elements.cancelBtn) {
                elements.cancelBtn.addEventListener('click', function () {
                    closeMarkZeroModal();
                    resetMarkZeroState();
                });
            }

            if (elements.modalElement) {
                elements.modalElement.addEventListener('click', function (e) {
                    if (e.target === elements.modalElement) {
                        closeMarkZeroModal();
                        resetMarkZeroState();
                    }
                });
            }

            if (elements.toastClose) {
                elements.toastClose.addEventListener('click', function () {
                    hideTrainerSubmissionToast();
                });
            }
        }

        function openMarkZeroModal(assignmentId, studentId) {
            clearTrainerSubmissionError();

            if (!assignmentId || !studentId) {
                showTrainerSubmissionToast(
                    'Invalid selection',
                    'Invalid assignment or student selected.',
                    'error'
                );
                return;
            }

            state.selectedAssignmentId = assignmentId;
            state.selectedStudentId = studentId;

            if (elements.modalText) {
                elements.modalText.textContent =
                    'This student did not submit the assignment before the due date. Do you want to assign 0 marks?';
            }

            showElement(elements.modalElement);
        }

        function loadTrainerAssignmentSubmissions() {
            setTrainerSubmissionLoading();
            clearTrainerSubmissionError();

            fetch(buildTrainerSubmissionUrl(), {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Unable to load assignment submissions.');
                    }

                    return response.json();
                })
                .then(function (response) {
                    if (!response || response.success !== true) {
                        throw new Error(getResponseMessage(response));
                    }

                    const data = response.data || {};

                    renderTrainerSubmissionSummary(data.summary || {});
                    renderTrainerSubmissionFilters(data.filters || {});
                    renderTrainerSubmissionRows(data.submissions || []);
                })
                .catch(function (error) {
                    renderTrainerSubmissionSummary({});
                    renderTrainerSubmissionRows([]);
                    showTrainerSubmissionError(error.message || 'Unable to load assignment submissions.');
                    showTrainerSubmissionToast(
                        'Unable to load',
                        error.message || 'Unable to load assignment submissions.',
                        'error'
                    );
                });
        }

        function buildTrainerSubmissionUrl() {
            const params = new URLSearchParams();

            const search = getElementValue(elements.search);
            const courseId = getElementValue(elements.courseFilter);
            const moduleId = getElementValue(elements.moduleFilter);
            const status = getElementValue(elements.statusFilter);

            if (search) {
                params.append('search', search);
            }

            if (courseId) {
                params.append('courseId', courseId);
            }

            if (moduleId) {
                params.append('moduleId', moduleId);
            }

            if (status) {
                params.append('status', status);
            }

            const query = params.toString();

            return query
                ? '/api/TrainerAssignmentGrading/submissions?' + query
                : '/api/TrainerAssignmentGrading/submissions';
        }

        function renderTrainerSubmissionSummary(summary) {
            setText(elements.total, summary.totalSubmissions || 0);
            setText(elements.graded, summary.graded || 0);
            setText(elements.pending, summary.pending || 0);
            setText(elements.missing, summary.notSubmitted || 0);
        }

        function renderTrainerSubmissionFilters(filters) {
            populateTrainerSubmissionCourses(filters.courses || []);

            state.modules = filters.modules || [];
            populateTrainerSubmissionModules();
        }

        function populateTrainerSubmissionCourses(courses) {
            if (!elements.courseFilter) {
                return;
            }

            const selectedCourseId = elements.courseFilter.value;

            let html = '<option value="">All courses</option>';

            courses.forEach(function (course) {
                const courseId = course.courseId || '';
                const title = course.courseTitle || '';

                html += '<option value="' + escapeAttribute(courseId) + '">' +
                    escapeHtml(title) +
                    '</option>';
            });

            elements.courseFilter.innerHTML = html;

            if (selectedCourseId) {
                elements.courseFilter.value = selectedCourseId;
            }
        }

        function populateTrainerSubmissionModules() {
            if (!elements.moduleFilter) {
                return;
            }

            const selectedCourseId = getElementValue(elements.courseFilter);
            const selectedModuleId = elements.moduleFilter.value;

            let modules = state.modules || [];

            if (selectedCourseId) {
                modules = modules.filter(function (module) {
                    return String(module.courseId) === String(selectedCourseId);
                });
            }

            let html = '<option value="">All modules</option>';

            modules.forEach(function (module) {
                const moduleId = module.moduleId || '';
                const title = module.moduleTitle || '';

                html += '<option value="' + escapeAttribute(moduleId) + '">' +
                    escapeHtml(title) +
                    '</option>';
            });

            elements.moduleFilter.innerHTML = html;

            if (selectedModuleId && modules.some(function (module) {
                return String(module.moduleId) === String(selectedModuleId);
            })) {
                elements.moduleFilter.value = selectedModuleId;
            }
        }

        function renderTrainerSubmissionRows(submissions) {
            if (!elements.rows) {
                return;
            }

            const count = submissions.length;

            setText(elements.recordCount, count + ' record(s)');

            if (count === 0) {
                elements.rows.innerHTML = '';
                showElement(elements.empty);
                return;
            }

            hideElement(elements.empty);

            let html = '';

            submissions.forEach(function (item) {
                html += buildTrainerSubmissionRow(item);
            });

            elements.rows.innerHTML = html;
        }

        function buildTrainerSubmissionRow(item) {
            const studentName = item.studentName || '—';
            const assignmentTitle = item.assignmentTitle || '—';
            const courseTitle = item.courseTitle || '—';
            const moduleTitle = item.moduleTitle || 'Course Level';
            const submittedAtText = item.submittedAtText || '—';
            const status = item.status || '—';
            const statusClass = item.statusCssClass || '';
            const scoreText = item.scoreText || '—';
            const scoreClass = item.scoreCssClass || 'trainer-score-muted';

            return '' +
                '<div class="dashboard-table-row trainer-submissions-table-row">' +
                '<div class="dashboard-cell-strong" title="' + escapeAttribute(studentName) + '">' + escapeHtml(studentName) + '</div>' +
                '<div title="' + escapeAttribute(assignmentTitle) + '">' + escapeHtml(assignmentTitle) + '</div>' +
                '<div title="' + escapeAttribute(courseTitle) + '">' + escapeHtml(courseTitle) + '</div>' +
                '<div title="' + escapeAttribute(moduleTitle) + '">' + escapeHtml(moduleTitle) + '</div>' +
                '<div title="' + escapeAttribute(submittedAtText) + '">' + escapeHtml(submittedAtText) + '</div>' +
                '<div title="' + escapeAttribute(status) + '">' +
                '<span class="' + escapeAttribute(statusClass) + '">' + escapeHtml(status) + '</span>' +
                '</div>' +
                '<div class="' + escapeAttribute(scoreClass) + '" title="' + escapeAttribute(scoreText) + '">' + escapeHtml(scoreText) + '</div>' +
                '<div class="trainer-courses-actions trainer-submissions-actions">' +
                buildTrainerSubmissionAction(item) +
                '</div>' +
                '</div>';
        }

        function buildTrainerSubmissionAction(item) {
            if (!item) {
                return '<span class="trainer-action-dash">—</span>';
            }

            if (item.canEdit && item.actionUrl) {
                return '' +
                    '<a href="' + escapeAttribute(item.actionUrl) + '" ' +
                    'class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">' +
                    'Edit' +
                    '</a>';
            }

            if (item.canGrade && item.actionUrl) {
                return '' +
                    '<a href="' + escapeAttribute(item.actionUrl) + '" ' +
                    'class="dashboard-btn trainer-sub-soft-btn trainer-course-action-btn">' +
                    'Grade' +
                    '</a>';
            }

            if (item.canMarkZero) {
                return '' +
                    '<button type="button" ' +
                    'class="dashboard-btn dashboard-btn-outline trainer-course-action-btn trainer-mark-zero-btn" ' +
                    'data-assignment-id="' + escapeAttribute(item.assignmentId || 0) + '" ' +
                    'data-student-id="' + escapeAttribute(item.studentId || 0) + '">' +
                    'Mark 0' +
                    '</button>';
            }

            return '<span class="trainer-action-dash">—</span>';
        }

        function markMissingSubmissionZero() {
            clearTrainerSubmissionError();

            if (!state.selectedAssignmentId || !state.selectedStudentId) {
                showTrainerSubmissionToast(
                    'Invalid selection',
                    'Invalid assignment or student selected.',
                    'error'
                );
                return;
            }

            setMarkZeroButtonLoading(true);

            fetch('/api/TrainerAssignmentGrading/missing/mark-zero', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    assignmentId: state.selectedAssignmentId,
                    studentId: state.selectedStudentId
                })
            })
                .then(function (response) {
                    return response.json().then(function (data) {
                        if (!response.ok) {
                            throw new Error(getResponseMessage(data));
                        }

                        return data;
                    });
                })
                .then(function (response) {
                    if (!response || response.success !== true) {
                        throw new Error(getResponseMessage(response));
                    }

                    closeMarkZeroModal();
                    resetMarkZeroState();

                    showTrainerSubmissionToast(
                        'Marked as 0',
                        response.message || 'Missing submission has been marked as 0.',
                        'success'
                    );

                    loadTrainerAssignmentSubmissions();
                })
                .catch(function (error) {
                    showTrainerSubmissionToast(
                        'Unable to mark 0',
                        error.message || 'Unable to mark missing submission as 0.',
                        'error'
                    );
                })
                .finally(function () {
                    setMarkZeroButtonLoading(false);
                });
        }

        function closeMarkZeroModal() {
            hideElement(elements.modalElement);
        }

        function resetMarkZeroState() {
            state.selectedAssignmentId = 0;
            state.selectedStudentId = 0;
        }

        function setMarkZeroButtonLoading(isLoading) {
            if (!elements.confirmBtn) {
                return;
            }

            if (isLoading) {
                elements.confirmBtn.disabled = true;
                elements.confirmBtn.textContent = 'Saving...';
                return;
            }

            elements.confirmBtn.disabled = false;
            elements.confirmBtn.textContent = 'Confirm';
        }

        function showTrainerSubmissionToast(title, message, type) {
            if (!elements.toast) {
                return;
            }

            clearTimeout(state.toastTimer);

            setText(elements.toastTitle, title || 'Notification');
            setText(elements.toastMessage, message || '');

            if (elements.toastIcon) {
                if (type === 'error') {
                    elements.toastIcon.innerHTML = '<i class="bi bi-exclamation-circle"></i>';
                } else {
                    elements.toastIcon.innerHTML = '<i class="bi bi-check2-circle"></i>';
                }
            }

            showElement(elements.toast);

            state.toastTimer = setTimeout(function () {
                hideTrainerSubmissionToast();
            }, 3500);
        }

        function hideTrainerSubmissionToast() {
            hideElement(elements.toast);
        }

        function setTrainerSubmissionLoading() {
            if (!elements.rows) {
                return;
            }

            hideElement(elements.empty);

            elements.rows.innerHTML = '' +
                '<div class="dashboard-table-row trainer-submissions-table-row">' +
                '<div class="dashboard-cell-strong">Loading...</div>' +
                '<div>—</div>' +
                '<div>—</div>' +
                '<div>—</div>' +
                '<div>—</div>' +
                '<div>—</div>' +
                '<div>—</div>' +
                '<div>—</div>' +
                '</div>';
        }

        function showTrainerSubmissionError(message) {
            if (!elements.error) {
                return;
            }

            elements.error.innerHTML = '<div>' + escapeHtml(message) + '</div>';
        }

        function clearTrainerSubmissionError() {
            if (elements.error) {
                elements.error.innerHTML = '';
            }
        }

        function getResponseMessage(response) {
            return response && (response.message || response.errorMessage || response.title)
                ? response.message || response.errorMessage || response.title
                : 'Something went wrong. Please try again.';
        }

        function getElementValue(element) {
            return element && element.value
                ? element.value.trim()
                : '';
        }

        function setText(element, value) {
            if (element) {
                element.textContent = value;
            }
        }

        function showElement(element) {
            if (element) {
                element.classList.remove('trainer-hidden');
            }
        }

        function hideElement(element) {
            if (element) {
                element.classList.add('trainer-hidden');
            }
        }

        function escapeHtml(value) {
            return String(value ?? '')
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#039;');
        }

        function escapeAttribute(value) {
            return escapeHtml(value);
        }
    });
})();