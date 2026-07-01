(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const page = document.getElementById('trainerAssignmentGradePage');

        if (!page) {
            return;
        }

        const elements = {
            pageTitle: document.getElementById('trainerGradePageTitle'),
            pageSub: document.getElementById('trainerGradePageSub'),

            assignmentHeading: document.getElementById('trainerGradeAssignmentHeading'),
            statusBadge: document.getElementById('trainerGradeStatusBadge'),

            metaStudent: document.getElementById('trainerGradeMetaStudent'),
            metaCourse: document.getElementById('trainerGradeMetaCourse'),
            metaModule: document.getElementById('trainerGradeMetaModule'),
            metaSubmitted: document.getElementById('trainerGradeMetaSubmitted'),

            currentScore: document.getElementById('trainerGradeCurrentScore'),
            currentMeta: document.getElementById('trainerGradeCurrentMeta'),

            form: document.getElementById('trainerAssignmentGradeForm'),
            submissionIdInput: document.getElementById('trainerGradeSubmissionId'),

            studentName: document.getElementById('trainerGradeStudentName'),
            assignmentTitle: document.getElementById('trainerGradeAssignmentTitle'),
            course: document.getElementById('trainerGradeCourse'),
            module: document.getElementById('trainerGradeModule'),
            submittedOn: document.getElementById('trainerGradeSubmittedOn'),
            totalMarks: document.getElementById('trainerGradeTotalMarks'),
            awardedMarks: document.getElementById('trainerGradeAwardedMarks'),
            resultLabel: document.getElementById('trainerGradeResultLabel'),
            feedback: document.getElementById('trainerGradeFeedback'),

            fileName: document.getElementById('trainerSubmissionFileName'),
            fileMeta: document.getElementById('trainerSubmissionFileMeta'),
            viewFileBtn: document.getElementById('trainerSubmissionViewFileBtn'),
            downloadFileBtn: document.getElementById('trainerSubmissionDownloadFileBtn'),

            previewWrap: document.getElementById('trainerSubmissionPreviewWrap'),
            preview: document.getElementById('trainerSubmissionPreview'),

            submittedTextWrap: document.getElementById('trainerSubmittedTextWrap'),
            submittedText: document.getElementById('trainerSubmittedText'),

            error: document.getElementById('trainerGradeError'),
            saveBtn: document.getElementById('trainerGradeSaveBtn')
        };

        const state = {
            submissionId: getSubmissionId(),
            mode: (page.dataset.mode || 'grade').toLowerCase(),
            detail: null
        };

        setupTrainerGradePageMode();

        if (!state.submissionId || state.submissionId <= 0) {
            showTrainerGradeError('Invalid submission selected.');
            disableTrainerGradeForm();
            return;
        }

        bindTrainerGradeEvents();
        loadTrainerGradeDetail();

        function setupTrainerGradePageMode() {
            if (state.mode === 'edit') {
                setText(elements.pageTitle, 'Edit Submission');
                setText(elements.pageSub, 'Review student work, update marks, and manage grading details.');
                setText(elements.saveBtn, 'Save Changes');
                return;
            }

            setText(elements.pageTitle, 'Grade Submission');
            setText(elements.pageSub, 'Review the submitted work and assign marks with feedback.');
            setText(elements.saveBtn, 'Submit Grade');
        }

        function bindTrainerGradeEvents() {
            if (elements.form) {
                elements.form.addEventListener('submit', function (e) {
                    e.preventDefault();
                    saveTrainerGrade();
                });
            }

            if (elements.awardedMarks) {
                elements.awardedMarks.addEventListener('input', function () {
                    updateResultLabelFromMarks();
                });
            }
        }

        function loadTrainerGradeDetail() {
            clearTrainerGradeError();
            setTrainerGradeLoading(true);

            fetch('/api/TrainerAssignmentGrading/submission/' + encodeURIComponent(state.submissionId), {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Unable to load submission detail.');
                    }

                    return response.json();
                })
                .then(function (response) {
                    if (!response || response.success !== true) {
                        throw new Error(getResponseMessage(response));
                    }

                    state.detail = response.data || null;

                    if (!state.detail) {
                        throw new Error('Submission detail not found.');
                    }

                    renderTrainerGradeDetail(state.detail);
                })
                .catch(function (error) {
                    showTrainerGradeError(error.message || 'Unable to load submission detail.');
                    disableTrainerGradeForm();
                })
                .finally(function () {
                    setTrainerGradeLoading(false);
                });
        }

        function renderTrainerGradeDetail(detail) {
            setText(elements.assignmentHeading, detail.assignmentTitle || 'Assignment');
            setStatusBadge(detail.status || 'Pending Review', detail.statusCssClass || 'pill pill-yellow');

            setText(elements.metaStudent, detail.studentName || '—');
            setText(elements.metaCourse, detail.courseTitle || '—');
            setText(elements.metaModule, detail.moduleTitle || 'Course Level');
            setText(elements.metaSubmitted, detail.submittedAtText || '—');

            setText(elements.currentScore, detail.currentScoreText || ('--/' + (detail.totalMarks || 0)));
            setText(elements.currentMeta, detail.currentScoreMeta || 'Pending');

            setValue(elements.studentName, detail.studentName || '');
            setValue(elements.assignmentTitle, detail.assignmentTitle || '');
            setValue(elements.course, detail.courseTitle || '');
            setValue(elements.module, detail.moduleTitle || 'Course Level');
            setValue(elements.submittedOn, detail.submittedAtText || '');
            setValue(elements.totalMarks, detail.totalMarks || 0);

            if (elements.awardedMarks) {
                elements.awardedMarks.max = detail.totalMarks || 0;
                elements.awardedMarks.value = detail.obtainedMarks ?? '';
            }

            setValue(elements.feedback, detail.feedback || '');

            setResultLabel(detail.currentScoreMeta || '');
            renderSubmittedText(detail);
            setupTrainerSubmissionFile(detail);
        }

        function setupTrainerSubmissionFile(detail) {
            const filePath = normalizeFilePath(detail.submittedFilePath || '');
            const fileName = detail.submittedFileName || 'No file uploaded';
            const fileViewType = detail.fileViewType || '';

            setText(elements.fileName, fileName);

            if (!filePath) {
                setText(elements.fileMeta, 'No uploaded submission file');
                hideElement(elements.viewFileBtn);
                hideElement(elements.downloadFileBtn);
                hideTrainerSubmissionPreview();
                return;
            }

            setText(elements.fileMeta, getFileMetaText(fileViewType));

            if (detail.canViewFile && fileViewType) {
                showElement(elements.viewFileBtn);

                elements.viewFileBtn.onclick = function () {
                    showTrainerSubmissionPreview(filePath, fileName, fileViewType);
                };
            } else {
                hideElement(elements.viewFileBtn);
            }

            if (detail.canDownloadFile) {
                showElement(elements.downloadFileBtn);

                elements.downloadFileBtn.href = filePath;
                elements.downloadFileBtn.setAttribute('download', fileName);
                elements.downloadFileBtn.setAttribute('target', '_blank');
            } else {
                hideElement(elements.downloadFileBtn);
            }

            hideTrainerSubmissionPreview();
        }

        function showTrainerSubmissionPreview(filePath, fileName, fileViewType) {
            if (!elements.preview || !elements.previewWrap) {
                return;
            }

            let html = '';

            if (fileViewType === 'pdf') {
                html = '<iframe src="' + escapeAttribute(filePath) + '" class="trainer-submission-preview-frame"></iframe>';
            } else if (fileViewType === 'image') {
                html = '<img src="' + escapeAttribute(filePath) + '" alt="' + escapeAttribute(fileName) + '" class="trainer-submission-preview-image" />';
            } else if (fileViewType === 'video') {
                html = '<video controls class="trainer-submission-preview-video">' +
                    '<source src="' + escapeAttribute(filePath) + '">' +
                    'Your browser does not support video preview.' +
                    '</video>';
            } else {
                html = '<div class="dashboard-muted-small">Preview is not available for this file type. Please download the file.</div>';
            }

            elements.preview.innerHTML = html;
            showElement(elements.previewWrap);
        }

        function hideTrainerSubmissionPreview() {
            if (elements.preview) {
                elements.preview.innerHTML = '';
            }

            hideElement(elements.previewWrap);
        }

        function renderSubmittedText(detail) {
            const submittedText = detail.submittedText || '';

            if (!submittedText.trim()) {
                hideElement(elements.submittedTextWrap);
                setValue(elements.submittedText, '');
                return;
            }

            setValue(elements.submittedText, submittedText);
            showElement(elements.submittedTextWrap);
        }

        function saveTrainerGrade() {
            clearTrainerGradeError();

            const totalMarks = parseInt(getElementValue(elements.totalMarks), 10) || 0;
            const obtainedMarks = parseInt(getElementValue(elements.awardedMarks), 10);

            if (Number.isNaN(obtainedMarks)) {
                showTrainerGradeError('Please enter awarded marks.');
                return;
            }

            if (obtainedMarks < 0 || obtainedMarks > totalMarks) {
                showTrainerGradeError('Awarded marks must be between 0 and ' + totalMarks + '.');
                return;
            }

            const payload = {
                obtainedMarks: obtainedMarks,
                feedback: getElementValue(elements.feedback)
            };

            setTrainerGradeSaving(true);

            fetch('/api/TrainerAssignmentGrading/submission/' + encodeURIComponent(state.submissionId) + '/grade', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
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

                    window.location.href = '/Trainer/Submissions';
                })
                .catch(function (error) {
                    showTrainerGradeError(error.message || 'Unable to save grade. Please try again.');
                })
                .finally(function () {
                    setTrainerGradeSaving(false);
                });
        }

        function updateResultLabelFromMarks() {
            const totalMarks = parseInt(getElementValue(elements.totalMarks), 10) || 0;
            const obtainedMarks = parseInt(getElementValue(elements.awardedMarks), 10);

            if (!elements.resultLabel || Number.isNaN(obtainedMarks) || totalMarks <= 0) {
                return;
            }

            const percentage = (obtainedMarks * 100) / totalMarks;

            if (percentage >= 80) {
                elements.resultLabel.value = 'Excellent Work';
            } else if (percentage >= 50) {
                elements.resultLabel.value = 'Good Attempt';
            } else {
                elements.resultLabel.value = 'Needs Improvement';
            }
        }

        function setResultLabel(label) {
            if (!elements.resultLabel) {
                return;
            }

            const allowedLabels = [
                'Excellent Work',
                'Good Attempt',
                'Needs Improvement'
            ];

            if (allowedLabels.includes(label)) {
                elements.resultLabel.value = label;
            } else {
                elements.resultLabel.value = '';
                updateResultLabelFromMarks();
            }
        }

        function setStatusBadge(status, cssClass) {
            if (!elements.statusBadge) {
                return;
            }

            elements.statusBadge.className = cssClass || 'pill pill-yellow';
            elements.statusBadge.textContent = status || 'Pending Review';
        }

        function setTrainerGradeLoading(isLoading) {
            if (!elements.saveBtn) {
                return;
            }

            if (isLoading) {
                elements.saveBtn.disabled = true;
                elements.saveBtn.textContent = 'Loading...';
                return;
            }

            elements.saveBtn.disabled = false;
            elements.saveBtn.textContent = state.mode === 'edit'
                ? 'Save Changes'
                : 'Submit Grade';
        }

        function setTrainerGradeSaving(isSaving) {
            if (!elements.saveBtn) {
                return;
            }

            if (isSaving) {
                elements.saveBtn.disabled = true;
                elements.saveBtn.textContent = state.mode === 'edit'
                    ? 'Saving...'
                    : 'Submitting...';
                return;
            }

            elements.saveBtn.disabled = false;
            elements.saveBtn.textContent = state.mode === 'edit'
                ? 'Save Changes'
                : 'Submit Grade';
        }

        function disableTrainerGradeForm() {
            if (elements.saveBtn) {
                elements.saveBtn.disabled = true;
            }

            if (elements.awardedMarks) {
                elements.awardedMarks.disabled = true;
            }

            if (elements.resultLabel) {
                elements.resultLabel.disabled = true;
            }

            if (elements.feedback) {
                elements.feedback.disabled = true;
            }
        }

        function showTrainerGradeError(message) {
            if (!elements.error) {
                return;
            }

            elements.error.innerHTML = '<div>' + escapeHtml(message) + '</div>';
        }

        function clearTrainerGradeError() {
            if (elements.error) {
                elements.error.innerHTML = '';
            }
        }

        function getSubmissionId() {
            const fromPage = page.dataset.submissionId;
            const fromInput = elements.submissionIdInput ? elements.submissionIdInput.value : '';

            const id = parseInt(fromPage || fromInput || '0', 10);

            return Number.isNaN(id) ? 0 : id;
        }

        function normalizeFilePath(filePath) {
            if (!filePath) {
                return '';
            }

            const trimmed = filePath.trim();

            if (
                trimmed.startsWith('http://') ||
                trimmed.startsWith('https://') ||
                trimmed.startsWith('/')
            ) {
                return trimmed;
            }

            return '/' + trimmed;
        }

        function getFileMetaText(fileViewType) {
            if (fileViewType === 'pdf') {
                return 'PDF submission file';
            }

            if (fileViewType === 'image') {
                return 'Image submission file';
            }

            if (fileViewType === 'video') {
                return 'Video submission file';
            }

            return 'Uploaded submission file';
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

        function setValue(element, value) {
            if (element) {
                element.value = value;
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