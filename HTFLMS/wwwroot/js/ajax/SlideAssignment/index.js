let deleteMaterialId = null;
let deleteAssignmentId = null;

$(document).ready(function () {

    showStoredSuccessMessage();
    createMaterialDeleteModal();
    createAssignmentDeleteModal();
    resetStats();
    loadTrainerCourses();

    $('#courseSelect').on('change', handleCourseChange);
    $('#moduleSelect').on('change', handleModuleChange);

    $(document).on('click', '#uploadMaterialBtn', handleUploadMaterialClick);
    $(document).on('click', '#createAssignmentBtn', handleCreateAssignmentClick);

    $(document).on('click', '#cancelMaterialDeleteBtn', cancelMaterialDelete);
    $(document).on('click', '#confirmMaterialDeleteBtn', confirmMaterialDelete);

    $(document).on('click', '#cancelAssignmentDeleteBtn', cancelAssignmentDelete);
    $(document).on('click', '#confirmAssignmentDeleteBtn', confirmAssignmentDelete);
});

function showStoredSuccessMessage() {
    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showSuccessPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }
}

function handleCourseChange() {
    var courseId = $('#courseSelect').val();

    $('#selectedCourseId').val(courseId);
    $('#selectedModuleId').val('0');

    if (courseId && courseId !== '0') {
        sessionStorage.setItem("selectedCourseId", courseId);
        sessionStorage.removeItem("selectedModuleId");

        loadModules(courseId);
        loadMaterialsByCourse(courseId);
        loadAssignmentsByCourse(courseId);
    } else {
        sessionStorage.removeItem("selectedCourseId");
        sessionStorage.removeItem("selectedModuleId");

        $('#moduleSelect').html('<option value="0">Select a course first</option>');
        showMaterialEmptyState('Please select a course.');
        showAssignmentEmptyState('Please select a course.');
        resetStats();
    }

    updateUploadButton();
    updateCreateAssignmentButton();
}

function handleModuleChange() {
    var moduleId = $('#moduleSelect').val();
    var courseId = $('#courseSelect').val();

    $('#selectedModuleId').val(moduleId);

    if (moduleId && moduleId !== '0') {
        sessionStorage.setItem("selectedModuleId", moduleId);
        loadMaterials(moduleId);
        loadAssignments(moduleId);
    } else {
        sessionStorage.removeItem("selectedModuleId");

        if (courseId && courseId !== '0') {
            loadMaterialsByCourse(courseId);
            loadAssignmentsByCourse(courseId);
        } else {
            showMaterialEmptyState('Please select a course.');
            showAssignmentEmptyState('Please select a course.');
            resetStats();
        }
    }

    updateUploadButton();
    updateCreateAssignmentButton();
}

function handleUploadMaterialClick(e) {
    var courseId = $('#courseSelect').val();
    var moduleId = $('#moduleSelect').val() || '0';

    if (!courseId || courseId === '0') {
        e.preventDefault();
        showSuccessPopup('Please select a course first.');
        return;
    }

    $(this).attr(
        'href',
        '/Trainer/SlidesAssignments/CreateMaterial?courseId=' + courseId + '&moduleId=' + moduleId
    );
}

function handleCreateAssignmentClick(e) {
    var courseId = $('#courseSelect').val();
    var moduleId = $('#moduleSelect').val() || '0';

    if (!courseId || courseId === '0') {
        e.preventDefault();
        showSuccessPopup('Please select a course first.');
        return;
    }

    $(this).attr(
        'href',
        '/Trainer/SlidesAssignments/CreateAssignment?courseId=' + courseId + '&moduleId=' + moduleId
    );
}

function getValue(obj, camelName, pascalName) {
    return obj[camelName] ?? obj[pascalName];
}

function showMaterialEmptyState(message) {
    $('#materialsContainer').html('<div class="dashboard-empty-state" style="padding: 18px 15px;">' + message + '</div>');
}

function showAssignmentEmptyState(message) {
    $('#assignmentsContainer').html('<div class="dashboard-empty-state">' + message + '</div>');
}

function resetStats() {
    $('#totalFilesCount').text('0');
    $('#assignmentsCount').text('0');
    $('#videoLessonsCount').text('0');
    $('#submissionsCount').text('0');
}

function updateMaterialStats(materials) {
    var totalFiles = materials.length;

    var videos = materials.filter(function (m) {
        var type = getValue(m, 'contentType', 'ContentType');
        return type === 'Video Link' || type === 'MP4 Upload';
    }).length;

    $('#totalFilesCount').text(totalFiles);
    $('#videoLessonsCount').text(videos);
}

function updateAssignmentStats(assignments) {
    $('#assignmentsCount').text(assignments.length);

    var submissions = assignments.reduce(function (total, assignment) {
        return total + (getValue(assignment, 'submittedCount', 'SubmittedCount') || 0);
    }, 0);

    $('#submissionsCount').text(submissions);
}

function updateUploadButton() {
    var courseId = $('#courseSelect').val() || '0';
    var moduleId = $('#moduleSelect').val() || '0';

    $('#uploadMaterialBtn').attr(
        'href',
        '/Trainer/SlidesAssignments/CreateMaterial?courseId=' + courseId + '&moduleId=' + moduleId
    );
}

function updateCreateAssignmentButton() {
    var courseId = $('#courseSelect').val() || '0';
    var moduleId = $('#moduleSelect').val() || '0';

    $('#createAssignmentBtn').attr(
        'href',
        '/Trainer/SlidesAssignments/CreateAssignment?courseId=' + courseId + '&moduleId=' + moduleId
    );
}

function loadTrainerCourses() {
    $.ajax({
        url: '/api/Course',
        type: 'GET',

        success: function (courses) {
            populateCourseSelect(courses);
        },

        error: function () {
            showMaterialEmptyState('Courses could not be loaded.');
            showAssignmentEmptyState('Courses could not be loaded.');
        }
    });
}

function populateCourseSelect(courses) {
    var courseSelect = $('#courseSelect');
    var selectedCourseId = $('#selectedCourseId').val();
    var savedCourseId = sessionStorage.getItem("selectedCourseId");

    courseSelect.html('<option value="0">Select Course</option>');

    if (!courses || courses.length === 0) {
        showMaterialEmptyState('No courses found.');
        showAssignmentEmptyState('No courses found.');
        return;
    }

    $.each(courses, function (index, course) {
        var courseId = getValue(course, 'id', 'Id');
        var courseTitle = getValue(course, 'title', 'Title');

        courseSelect.append(`
            <option value="${courseId}">
                ${courseTitle}
            </option>
        `);
    });

    var courseIds = courses.map(function (c) {
        return getValue(c, 'id', 'Id').toString();
    });

    var courseIdToSelect = getValidSavedId(savedCourseId, selectedCourseId, courseIds);

    courseSelect.val(courseIdToSelect);
    $('#selectedCourseId').val(courseIdToSelect);

    if (courseIdToSelect !== '0') {
        loadModules(courseIdToSelect);
        loadMaterialsByCourse(courseIdToSelect);
        loadAssignmentsByCourse(courseIdToSelect);
    }

    updateUploadButton();
    updateCreateAssignmentButton();
}

function getValidSavedId(savedId, selectedId, validIds) {
    var successMessage = sessionStorage.getItem("successMessage");
    var restoreAfterCancel = sessionStorage.getItem("restoreSelectionAfterCancel") === "true";

    var shouldRestoreSelection = !!successMessage || restoreAfterCancel;

    if (
        shouldRestoreSelection &&
        savedId &&
        savedId !== '0' &&
        validIds.includes(savedId)
    ) {
        sessionStorage.removeItem("restoreSelectionAfterCancel");
        return savedId;
    }

    if (
        selectedId &&
        selectedId !== '0' &&
        validIds.includes(selectedId.toString())
    ) {
        return selectedId;
    }

    return '0';
}

function loadModules(courseId) {
    $.ajax({
        url: '/api/Module/course/' + courseId,
        type: 'GET',

        success: function (modules) {
            populateModuleSelect(modules);
        },

        error: function () {
            $('#moduleSelect').html('<option value="0">No modules found</option>');
        }
    });
}

function populateModuleSelect(modules) {
    var moduleSelect = $('#moduleSelect');
    var savedModuleId = sessionStorage.getItem("selectedModuleId");

    moduleSelect.html('<option value="0">All Modules</option>');

    if (!modules || modules.length === 0) {
        moduleSelect.html('<option value="0">No modules found</option>');
        return;
    }

    $.each(modules, function (index, module) {
        var moduleId = getValue(module, 'id', 'Id');
        var moduleTitle = getValue(module, 'title', 'Title');

        moduleSelect.append(`
            <option value="${moduleId}">
                ${moduleTitle}
            </option>
        `);
    });

    if (savedModuleId && savedModuleId !== '0') {
        moduleSelect.val(savedModuleId);
        $('#selectedModuleId').val(savedModuleId);

        loadMaterials(savedModuleId);
        loadAssignments(savedModuleId);
    } else {
        moduleSelect.val('0');
        $('#selectedModuleId').val('0');
    }

    updateUploadButton();
    updateCreateAssignmentButton();
}

/* MATERIAL FUNCTIONS */

function loadMaterialsByCourse(courseId) {
    loadMaterialsFromUrl(
        '/api/Material/course/' + courseId,
        'No materials found for this course.'
    );
}

function loadMaterials(moduleId) {
    loadMaterialsFromUrl(
        '/api/Material/module/' + moduleId,
        'No materials found for this module.'
    );
}

function loadMaterialsFromUrl(url, emptyMessage) {
    $.ajax({
        url: url,
        type: 'GET',

        success: function (materials) {
            renderMaterials(materials, emptyMessage);
        },

        error: function () {
            showMaterialEmptyState('Materials could not be loaded.');
            $('#totalFilesCount').text('0');
            $('#videoLessonsCount').text('0');
        }
    });
}

function renderMaterials(materials, emptyMessage) {
    var container = $('#materialsContainer');
    container.html('');

    if (!materials || materials.length === 0) {
        container.html('<div class="dashboard-empty-state" style="padding: 18px 15px;">' + emptyMessage + '</div>');
        $('#totalFilesCount').text('0');
        $('#videoLessonsCount').text('0');
        return;
    }

    updateMaterialStats(materials);

    $.each(materials, function (index, material) {
        container.append(buildMaterialRow(material));
    });
}

function buildMaterialRow(material) {
    var materialId = getValue(material, 'id', 'Id');
    var courseId = getValue(material, 'courseId', 'CourseId');
    var moduleId = getValue(material, 'moduleId', 'ModuleId');
    var moduleTitle = getValue(material, 'moduleTitle', 'ModuleTitle');
    var title = getValue(material, 'title', 'Title');
    var contentType = getValue(material, 'contentType', 'ContentType');
    var filePath = getValue(material, 'filePath', 'FilePath');
    var externalUrl = getValue(material, 'externalUrl', 'ExternalUrl');
    var isActive = getValue(material, 'isActive', 'IsActive');

    var iconInfo = getMaterialIconInfo(contentType);
    var statusText = isActive ? 'Active' : 'Draft';
    var statusClass = isActive ? 'pill-green' : 'pill-yellow';
    var moduleText = moduleTitle || (moduleId ? 'Module ' + moduleId : 'Course Level');
    var openButton = buildOpenButton(contentType, filePath, externalUrl);
    var sizeText = getMaterialSizeText(material, contentType);

    return `
        <div class="trainer-sa-files-row" data-material-id="${materialId}">
            <div class="trainer-sa-file-main dashboard-table-cell-ellipsis" title="${title}">
                <div class="trainer-sa-file-icon ${iconInfo.iconClass}">
                    <i class="bi ${iconInfo.icon}"></i>
                </div>

                <div class="trainer-sa-file-text">
                    <div class="trainer-sa-file-title">${title}</div>
                </div>
            </div>

            <div class="dashboard-table-cell-ellipsis" title="${contentType}">
                <span class="trainer-course-chip">${contentType}</span>
            </div>

            <div class="trainer-sa-meta-text dashboard-table-cell-ellipsis" title="${moduleText}">
                ${moduleText}
            </div>

            <div class="dashboard-table-cell-ellipsis" title="${statusText}">
                <span class="pill ${statusClass}">${statusText}</span>
            </div>

            <div class="trainer-courses-actions">
                ${openButton}

                <a href="/Trainer/SlidesAssignments/EditMaterial?id=${materialId}&courseId=${courseId}&moduleId=${moduleId || 0}"
                   class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn trainer-delete-soft-btn trainer-course-action-btn"
                        onclick="deleteMaterial(${materialId})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function getMaterialSizeText(material, contentType) {
    var pages = getValue(material, 'pages', 'Pages');
    var slides = getValue(material, 'slides', 'Slides');
    var minutes = getValue(material, 'minutes', 'Minutes');

    if (contentType === 'PDF') {
        return pages ? pages + ' Pages' : 'PDF';
    }

    if (contentType === 'PPTX') {
        return slides ? slides + ' Slides' : 'PPTX';
    }

    if (contentType === 'Video Link') {
        return minutes ? minutes + ' Min' : 'External';
    }

    if (contentType === 'MP4 Upload') {
        return minutes ? minutes + ' Min' : 'Video';
    }

    return '';
}

function getMaterialIconInfo(contentType) {
    if (contentType === 'PPTX') {
        return {
            iconClass: 'trainer-sa-file-ppt',
            icon: 'bi-file-earmark-slides'
        };
    }

    if (contentType === 'Video Link' || contentType === 'MP4 Upload') {
        return {
            iconClass: 'trainer-sa-file-mp4',
            icon: 'bi-play-btn'
        };
    }

    return {
        iconClass: 'trainer-sa-file-pdf',
        icon: 'bi-file-earmark-pdf'
    };
}

function buildOpenButton(contentType, filePath, externalUrl) {
    var url = contentType === 'Video Link' ? externalUrl : filePath;

    if (!url) {
        return '';
    }

    return `
        <a href="${url}" target="_blank"
           class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">
            Open
        </a>
    `;
}

function deleteMaterial(materialId) {
    deleteMaterialId = materialId;
    $('#materialDeleteModal').addClass('show');
}

function cancelMaterialDelete(e) {
    e.preventDefault();
    closeMaterialDeleteModal();
}

function confirmMaterialDelete(e) {
    e.preventDefault();

    if (!deleteMaterialId) return;

    $.ajax({
        url: '/api/Material/delete/' + deleteMaterialId,
        type: 'DELETE',

        success: function (response) {
            closeMaterialDeleteModal();

            if (response && response.message) {
                sessionStorage.setItem('successMessage', response.message);
                showSuccessPopup(response.message);
            }

            refreshCurrentMaterials();
        },

        error: function (xhr) {
            closeMaterialDeleteModal();
            showSuccessPopup(getErrorMessage(xhr));
        }
    });
}

function refreshCurrentMaterials() {
    var moduleId = $('#moduleSelect').val();
    var courseId = $('#courseSelect').val();

    if (moduleId && moduleId !== '0') {
        loadMaterials(moduleId);
    } else if (courseId && courseId !== '0') {
        loadMaterialsByCourse(courseId);
    }
}

function closeMaterialDeleteModal() {
    $('#materialDeleteModal').removeClass('show');
    deleteMaterialId = null;
}

/* ASSIGNMENT FUNCTIONS */

function loadAssignmentsByCourse(courseId) {
    loadAssignmentsFromUrl(
        '/api/Assignment/course/' + courseId,
        'No assignments found for this course.'
    );
}

function loadAssignments(moduleId) {
    loadAssignmentsFromUrl(
        '/api/Assignment/module/' + moduleId,
        'No assignments found for this module.'
    );
}

function loadAssignmentsFromUrl(url, emptyMessage) {
    $.ajax({
        url: url,
        type: 'GET',

        success: function (assignments) {
            renderAssignments(assignments, emptyMessage);
        },

        error: function () {
            showAssignmentEmptyState('Assignments could not be loaded.');
            $('#assignmentsCount').text('0');
            $('#submissionsCount').text('0');
        }
    });
}

function renderAssignments(assignments, emptyMessage) {
    var container = $('#assignmentsContainer');
    container.html('');

    if (!assignments || assignments.length === 0) {
        showAssignmentEmptyState(emptyMessage);
        $('#assignmentsCount').text('0');
        $('#submissionsCount').text('0');
        return;
    }

    updateAssignmentStats(assignments);

    $.each(assignments, function (index, assignment) {
        container.append(buildAssignmentRow(assignment));
    });
}

function buildAssignmentRow(assignment) {
    var assignmentId = getValue(assignment, 'id', 'Id');
    var courseId = getValue(assignment, 'courseId', 'CourseId');
    var moduleId = getValue(assignment, 'moduleId', 'ModuleId');
    var title = getValue(assignment, 'title', 'Title');
    var instructions = getValue(assignment, 'instructions', 'Instructions') || getValue(assignment, 'description', 'Description');
    var marks = getValue(assignment, 'marks', 'Marks');
    var dueDateTime = getValue(assignment, 'dueDateTime', 'DueDateTime');
    var filePath = getValue(assignment, 'filePath', 'FilePath');
    var isActive = getValue(assignment, 'isActive', 'IsActive');
    var submittedCount = getValue(assignment, 'submittedCount', 'SubmittedCount') || 0;
    var totalStudents = getValue(assignment, 'totalStudents', 'TotalStudents') || 0;

    var statusText = isActive ? 'Active' : 'Draft';
    var statusClass = isActive ? 'pill-green' : 'pill-yellow';
    var dueText = formatAssignmentDueDate(dueDateTime);
    var openButton = buildAssignmentOpenButton(filePath);

    var submissionText = totalStudents > 0
        ? submittedCount + '/' + totalStudents + ' submitted'
        : submittedCount + ' submitted';

    return `
        <div class="trainer-assignment-card" data-assignment-id="${assignmentId}">
            <div class="trainer-assignment-card-top">
                <div class="trainer-assignment-info">
                    <h3>${title}</h3>

                    <div class="trainer-assignment-meta">
                        <span>Marks: ${marks}</span>
                        <span>Due: ${dueText}</span>
                    </div>

                    <div class="trainer-assignment-badges">
                        <span class="pill ${statusClass}">${statusText}</span>
                        <span class="pill pill-green">${submissionText}</span>
                    </div>
                </div>

                <div class="trainer-assignment-actions">
                    ${openButton}

                    <a href="/Trainer/SlidesAssignments/EditAssignment?id=${assignmentId}&courseId=${courseId}&moduleId=${moduleId || 0}"
                       class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">
                        Edit
                    </a>

                    <button type="button"
                            class="dashboard-btn trainer-delete-soft-btn trainer-course-action-btn"
                            onclick="deleteAssignment(${assignmentId})">
                        Delete
                    </button>
                </div>
            </div>

            <div class="trainer-assignment-instructions">
                ${instructions || 'No instructions added.'}
            </div>
        </div>
    `;
}
function buildAssignmentOpenButton(filePath) {
    if (!filePath) {
        return '';
    }

    return `
        <a href="${filePath}" target="_blank"
           class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">
            Open
        </a>
    `;
}

function formatAssignmentDueDate(dueDateTime) {
    if (!dueDateTime) return 'Not set';

    var date = new Date(dueDateTime);

    if (isNaN(date.getTime())) return 'Not set';

    return date.toLocaleDateString() + ', ' + date.toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function deleteAssignment(assignmentId) {
    deleteAssignmentId = assignmentId;
    $('#assignmentDeleteModal').addClass('show');
}

function cancelAssignmentDelete(e) {
    e.preventDefault();
    closeAssignmentDeleteModal();
}

function confirmAssignmentDelete(e) {
    e.preventDefault();

    if (!deleteAssignmentId) return;

    $.ajax({
        url: '/api/Assignment/delete/' + deleteAssignmentId,
        type: 'DELETE',

        success: function (response) {
            closeAssignmentDeleteModal();

            if (response && response.message) {
                sessionStorage.setItem('successMessage', response.message);
                showSuccessPopup(response.message);
            }

            refreshCurrentAssignments();
        },

        error: function (xhr) {
            closeAssignmentDeleteModal();
            showSuccessPopup(getErrorMessage(xhr));
        }
    });
}

function refreshCurrentAssignments() {
    var moduleId = $('#moduleSelect').val();
    var courseId = $('#courseSelect').val();

    if (moduleId && moduleId !== '0') {
        loadAssignments(moduleId);
    } else if (courseId && courseId !== '0') {
        loadAssignmentsByCourse(courseId);
    }
}

function closeAssignmentDeleteModal() {
    $('#assignmentDeleteModal').removeClass('show');
    deleteAssignmentId = null;
}

/* COMMON ERROR + MODALS */

function getErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}

function createMaterialDeleteModal() {
    if ($('#materialDeleteModal').length > 0) {
        return;
    }

    $('body').append(`
        <div id="materialDeleteModal" class="custom-modal">
            <div class="custom-modal-backdrop"></div>

            <div class="custom-modal-box">
                <h3>Delete Material</h3>
                <p>Are you sure you want to delete this material?</p>

                <div class="custom-modal-actions">
                    <button type="button"
                            id="cancelMaterialDeleteBtn"
                            class="dashboard-btn dashboard-btn-outline">
                        Cancel
                    </button>

                    <button type="button"
                            id="confirmMaterialDeleteBtn"
                            class="dashboard-btn add-course-btn">
                        Delete
                    </button>
                </div>
            </div>
        </div>
    `);
}

function createAssignmentDeleteModal() {
    if ($('#assignmentDeleteModal').length > 0) {
        return;
    }

    $('body').append(`
        <div id="assignmentDeleteModal" class="custom-modal">
            <div class="custom-modal-backdrop"></div>

            <div class="custom-modal-box">
                <h3>Delete Assignment</h3>
                <p>Are you sure you want to delete this assignment?</p>

                <div class="custom-modal-actions">
                    <button type="button"
                            id="cancelAssignmentDeleteBtn"
                            class="dashboard-btn dashboard-btn-outline">
                        Cancel
                    </button>

                    <button type="button"
                            id="confirmAssignmentDeleteBtn"
                            class="dashboard-btn add-course-btn">
                        Delete
                    </button>
                </div>
            </div>
        </div>
    `);
}