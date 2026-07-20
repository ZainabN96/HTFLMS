(function () {
    const apiBaseUrl = '/api/student/certificates';

    document.addEventListener('DOMContentLoaded', function () {
        ensureCertificateToast();
        ensureCertificateConfirmModal();

        const certificateGrid = document.getElementById('certificateGrid');
        const certificatePage = document.querySelector('.certificate-view-page');

        if (certificateGrid) {
            loadCertificates();
            bindSearch();
        }

        if (certificatePage) {
            loadCertificateDetail(certificatePage);
        }
    });

    async function loadCertificates() {
        const grid = document.getElementById('certificateGrid');
        const emptyState = document.getElementById('certificateEmptyState');
        const countEl = document.getElementById('certificateCount');

        if (!grid) return;

        grid.innerHTML = getLoadingCard();

        try {
            const response = await fetch(apiBaseUrl);
            const result = await response.json();

            if (!response.ok || !result.success) {
                grid.innerHTML = '';
                showEmpty(emptyState, true);
                showToast(result.message || 'Unable to load certificates.');
                return;
            }

            const certificates = result.data || [];
            const approvedCount = certificates.filter(x => x.status === 'Approved').length;

            if (countEl) {
                countEl.textContent = `Approved Certificates: ${approvedCount}`;
            }

            if (!certificates.length) {
                grid.innerHTML = '';
                showEmpty(emptyState, true);
                return;
            }

            showEmpty(emptyState, false);
            grid.innerHTML = certificates.map(renderCertificateCard).join('');
            bindApplyButtons();

        } catch (error) {
            grid.innerHTML = '';
            showEmpty(emptyState, true);
            showToast('Unable to load certificates.');
        }
    }

    function renderCertificateCard(item) {
        const imagePath = item.courseImagePath || '/img/course/certificate.png';
        const metaDate = getMetaDateText(item);
        const actions = renderActions(item);
        const message = item.message ? `<p class="student-course-author certificate-status-message">${escapeHtml(item.message)}</p>` : '';
        const statusIcon = getStatusIcon(item.status);

        return `
            <div class="student-course-card certificate-card" data-title="${escapeHtml(item.courseTitle)}">
                <div class="student-course-image-wrap certificate-image-wrap">
                    <img src="${escapeHtml(imagePath)}" alt="${escapeHtml(item.courseTitle)} Certificate" class="student-course-image" />
                </div>

                <div class="student-course-body">
                    <h3 class="student-course-title">${escapeHtml(item.courseTitle)}</h3>
                    <p class="student-course-author">Issued to ${escapeHtml(item.studentName)}</p>

                    <div class="certificate-meta">
                        <div class="student-course-meta-item certificate-status-line">
                            <i class="bi ${statusIcon}"></i>
                            <span>${escapeHtml(item.statusText)}</span>
                        </div>

                        <div class="student-course-meta-item">
                            <i class="bi bi-calendar-event"></i>
                            <span>${escapeHtml(metaDate)}</span>
                        </div>
                    </div>

                    ${message}

                    <div class="certificate-actions">
                        ${actions}
                    </div>
                </div>
            </div>
        `;
    }

    function renderActions(item) {
        if (item.status === 'Pending') {
            return '';
        }

        if (item.canView) {
            const viewButton = `
                <a href="${escapeHtml(item.viewUrl || '#')}" class="student-material-btn">
                    <i class="bi bi-eye"></i>
                    View Certificate
                </a>
            `;

            const downloadButton = item.canDownload && item.downloadUrl
                ? `
                    <a href="${escapeHtml(item.downloadUrl)}" download class="student-material-btn download">
                        <i class="bi bi-download"></i>
                        Download PDF
                    </a>
                `
                : '';

            return viewButton + downloadButton;
        }

        if (item.canApply) {
            return `
                <button type="button"
                        class="student-material-btn certificate-apply-btn"
                        data-course-id="${item.courseId}"
                        data-course-title="${escapeHtml(item.courseTitle)}"
                        data-button-text="${escapeHtml(item.buttonText || 'Apply Certificate')}">
                    <i class="bi bi-send"></i>
                    ${escapeHtml(item.buttonText || 'Apply Certificate')}
                </button>
            `;
        }

        return `
            <button type="button" class="student-material-btn" disabled>
                <i class="bi bi-clock-history"></i>
                ${escapeHtml(item.buttonText || item.statusText)}
            </button>
        `;
    }

    function getStatusIcon(status) {
        if (status === 'Pending') return 'bi-hourglass-split';
        if (status === 'Rejected') return 'bi-x-circle';
        if (status === 'Approved') return 'bi-patch-check';
        if (status === 'ReadyToApply') return 'bi-send-check';
        if (status === 'CourseInProgress') return 'bi-clock-history';

        return 'bi-award';
    }

    function getMetaDateText(item) {
        if (item.status === 'Approved' && item.approvedAtText) {
            return `Issued: ${item.approvedAtText}`;
        }

        if (item.status === 'Pending' && item.requestedAtText) {
            return `Requested: ${item.requestedAtText}`;
        }

        if (item.status === 'Rejected' && item.requestedAtText) {
            return `Last Applied: ${item.requestedAtText}`;
        }

        if (item.batchEndDateText) {
            return `End Date: ${item.batchEndDateText}`;
        }

        return 'Date not available';
    }

    function bindApplyButtons() {
        const buttons = document.querySelectorAll('.certificate-apply-btn');

        buttons.forEach(button => {
            button.addEventListener('click', async function () {
                const courseId = this.getAttribute('data-course-id');
                const courseTitle = this.getAttribute('data-course-title') || 'this course';
                const buttonText = this.getAttribute('data-button-text') || 'Apply Certificate';

                if (!courseId) return;

                const confirmed = await showConfirmModal({
                    title: buttonText,
                    message: `Are you sure you want to submit certificate request for ${courseTitle}?`,
                    confirmText: buttonText,
                    cancelText: 'Cancel'
                });

                if (!confirmed) return;

                this.disabled = true;
                this.innerHTML = '<i class="bi bi-hourglass-split"></i> Applying...';

                try {
                    const response = await fetch(`${apiBaseUrl}/apply/${courseId}`, {
                        method: 'POST'
                    });

                    const result = await response.json();

                    if (!response.ok || !result.success) {
                        showToast(result.message || 'Unable to apply for certificate.');
                        await loadCertificates();
                        return;
                    }

                    showToast(result.message || 'Certificate request submitted successfully.');
                    await loadCertificates();

                } catch (error) {
                    showToast('Unable to apply for certificate.');
                    await loadCertificates();
                }
            });
        });
    }

    function bindSearch() {
        const searchInput = document.getElementById('certificateSearch');

        if (!searchInput) return;

        searchInput.addEventListener('input', function () {
            const term = this.value.toLowerCase().trim();
            const cards = document.querySelectorAll('.certificate-card');

            cards.forEach(card => {
                const title = (card.getAttribute('data-title') || '').toLowerCase();
                card.hidden = !title.includes(term);
            });
        });
    }

    async function loadCertificateDetail(page) {
        const requestId = page.getAttribute('data-certificate-request-id');
        const wrap = document.getElementById('certificateDetailWrap');
        const empty = document.getElementById('certificateDetailEmpty');

        if (!requestId || requestId === '0') {
            showCertificateDetailError(wrap, empty);
            return;
        }

        try {
            const response = await fetch(`${apiBaseUrl}/${requestId}`);
            const result = await response.json();

            if (!response.ok || !result.success || !result.data) {
                showCertificateDetailError(wrap, empty);
                return;
            }

            const certificate = result.data;

            setText('certificateStudentName', certificate.studentName);
            setText('certificateCourseTitle', certificate.courseTitle);
            setText('certificateIssueDate', certificate.issueDateText);
            setText('certificateId', certificate.certificateId);
            setText('certificateBatchNumber', certificate.batchNumber || '-');
            setText('certificateDuration', certificate.durationText || '-');

            const downloadWrap = document.getElementById('certificateDownloadWrap');

            if (downloadWrap && certificate.downloadUrl) {
                downloadWrap.innerHTML = `
                    <a href="${escapeHtml(certificate.downloadUrl)}"
                       download
                       class="student-material-btn download">
                        <i class="bi bi-download"></i>
                        Download PDF
                    </a>
                `;
            }

        } catch (error) {
            showCertificateDetailError(wrap, empty);
        }
    }

    function ensureCertificateConfirmModal() {
        if (document.getElementById('certificateConfirmModal')) return;

        const modal = document.createElement('div');
        modal.id = 'certificateConfirmModal';
        modal.className = 'certificate-modal-backdrop';
        modal.hidden = true;

        modal.innerHTML = `
            <div class="certificate-modal-card" role="dialog" aria-modal="true">
                <div class="certificate-modal-icon">
                    <i class="bi bi-patch-question"></i>
                </div>

                <h3 id="certificateConfirmTitle">Confirm Action</h3>
                <p id="certificateConfirmMessage">Are you sure?</p>

                <div class="certificate-modal-actions">
                    <button type="button" class="dashboard-btn dashboard-btn-outline" id="certificateConfirmCancel">
                        Cancel
                    </button>

                    <button type="button" class="dashboard-btn" id="certificateConfirmOk">
                        Confirm
                    </button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);
    }

    function showConfirmModal(options) {
        return new Promise(resolve => {
            const modal = document.getElementById('certificateConfirmModal');
            const title = document.getElementById('certificateConfirmTitle');
            const message = document.getElementById('certificateConfirmMessage');
            const okButton = document.getElementById('certificateConfirmOk');
            const cancelButton = document.getElementById('certificateConfirmCancel');

            if (!modal || !title || !message || !okButton || !cancelButton) {
                resolve(false);
                return;
            }

            title.textContent = options.title || 'Confirm Action';
            message.textContent = options.message || 'Are you sure?';
            okButton.textContent = options.confirmText || 'Confirm';
            cancelButton.textContent = options.cancelText || 'Cancel';

            modal.hidden = false;
            document.body.classList.add('certificate-modal-open');

            const close = value => {
                modal.hidden = true;
                document.body.classList.remove('certificate-modal-open');

                okButton.removeEventListener('click', onOk);
                cancelButton.removeEventListener('click', onCancel);
                modal.removeEventListener('click', onBackdrop);

                resolve(value);
            };

            const onOk = () => close(true);
            const onCancel = () => close(false);
            const onBackdrop = event => {
                if (event.target === modal) {
                    close(false);
                }
            };

            okButton.addEventListener('click', onOk);
            cancelButton.addEventListener('click', onCancel);
            modal.addEventListener('click', onBackdrop);
        });
    }

    function ensureCertificateToast() {
        if (document.getElementById('certificateToast')) return;

        const toast = document.createElement('div');
        toast.id = 'certificateToast';
        toast.className = 'certificate-toast';
        toast.hidden = true;

        toast.innerHTML = `
            <div class="certificate-toast-icon">
                <i class="bi bi-info-circle"></i>
            </div>
            <div id="certificateToastMessage"></div>
        `;

        document.body.appendChild(toast);
    }

    function showToast(message) {
        const toast = document.getElementById('certificateToast');
        const messageEl = document.getElementById('certificateToastMessage');

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

    function showCertificateDetailError(wrap, empty) {
        if (wrap) wrap.hidden = true;
        if (empty) empty.hidden = false;
    }

    function showEmpty(element, shouldShow) {
        if (!element) return;
        element.hidden = !shouldShow;
    }

    function setText(id, value) {
        const element = document.getElementById(id);

        if (element) {
            element.textContent = value || '-';
        }
    }

    function getLoadingCard() {
        return `
            <div class="dashboard-panel">
                <div class="certificate-empty-inner">
                    <div class="certificate-empty-icon">
                        <i class="bi bi-hourglass-split"></i>
                    </div>
                    <h3>Loading certificates...</h3>
                    <p>Please wait while we load your certificate records.</p>
                </div>
            </div>
        `;
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