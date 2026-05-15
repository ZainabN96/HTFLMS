$(document).ready(function () {

    var pageState = getAssignmentPageState();

    loadModulesForAssignment(pageState.courseId, pageState.moduleId);

    if (pageState.isEdit) {
        setupAssignmentEditMode();
        loadAssignment(pageState.assignmentId);
    }

    $('#assignmentCreateForm').on('submit', function (e) {
        e.preventDefault();
        saveAssignment(pageState);
    });
});

function getAssignmentPageState() {
    var assignmentId = $('#assignmentId').val();

    return {
        courseId: $('#courseId').val(),
        moduleId: $('#moduleId').val(),
        assignmentId: assignmentId,
        isEdit: assignmentId && assignmentId !== '' && assignmentId !== '0'
    };
}

function setupAssignmentEditMode() {
    $('.dashboard-h1').text('Edit Assignment');
    $('.dashboard-sub').text('Update assignment details, due date, marks, instructions, or attachment.');
    $('#saveAssignmentBtn').text('Update Assignment');
}

function saveAssignment(pageState) {
    var selectedModuleId = $('#moduleSelect').val();
    var btn = $('#saveAssignmentBtn');

    setAssignmentButtonLoading(btn, pageState.isEdit);

    $.ajax({
        url: pageState.isEdit
            ? '/api/Assignment/edit/' + pageState.assignmentId
            : '/api/Assignment/create',

        type: pageState.isEdit ? 'PUT' : 'POST',
        data: buildAssignmentFormData(pageState, selectedModuleId),
        processData: false,
        contentType: false,

        success: function (response) {
            if (response && response.message) {
                sessionStorage.setItem('successMessage', response.message);
            }

            window.location.href =
                '/Trainer/SlidesAssignments?courseId=' +
                pageState.courseId +
                '&moduleId=' +
                (selectedModuleId || 0);
        },

        error: function (xhr) {
            showAssignmentError(getAssignmentErrorMessage(xhr));
        },

        complete: function () {
            resetAssignmentButton(btn, pageState.isEdit);
        }
    });
}

function buildAssignmentFormData(pageState, selectedModuleId) {
    var formData = new FormData();

    formData.append('CourseId', pageState.courseId);

    if (selectedModuleId && selectedModuleId !== '0') {
        formData.append('ModuleId', selectedModuleId);
    }

    formData.append('Title', $('#title').val());
    formData.append('Description', $('#description').val());
    formData.append('Marks', $('#marks').val());
    formData.append('DueDateTime', buildDueDateTime());
    formData.append('IsActive', $('#isActive').val() === 'true');

    appendAssignmentFileIfSelected(formData);

    return formData;
}

function buildDueDateTime() {
    var dueDate = $('#dueDate').val();
    var dueTime = $('#dueTime').val();

    if (!dueDate || !dueTime) {
        return '';
    }

    return dueDate + 'T' + dueTime;
}

function appendAssignmentFileIfSelected(formData) {
    var file = $('#file')[0].files[0];

    if (file) {
        formData.append('File', file);
    }
}

function setAssignmentButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Creating...');
}

function resetAssignmentButton(btn, isEdit) {
    btn.prop('disabled', false).text(isEdit ? 'Update Assignment' : 'Create Assignment');
}

function loadModulesForAssignment(courseId, selectedModuleId) {
    $.ajax({
        url: '/api/Module/course/' + courseId,
        type: 'GET',

        success: function (modules) {
            populateModuleSelectForAssignment(modules, selectedModuleId);
        }
    });
}

function populateModuleSelectForAssignment(modules, selectedModuleId) {
    var moduleSelect = $('#moduleSelect');

    moduleSelect.html('<option value="0">Select Module</option>');

    if (modules && modules.length > 0) {
        $.each(modules, function (index, module) {
            var moduleId = module.id ?? module.Id;
            var moduleTitle = module.title ?? module.Title;

            moduleSelect.append(`
                <option value="${moduleId}">
                    ${moduleTitle}
                </option>
            `);
        });
    }

    if (selectedModuleId && selectedModuleId !== '0') {
        moduleSelect.val(selectedModuleId);
    }
}

function loadAssignment(assignmentId) {
    $.ajax({
        url: '/api/Assignment/' + assignmentId,
        type: 'GET',

        success: function (assignment) {
            populateAssignmentForm(assignment);
        },

        error: function (xhr) {
            showAssignmentError(getAssignmentErrorMessage(xhr));
        }
    });
}

function populateAssignmentForm(assignment) {
    $('#courseId').val(assignment.courseId);
    $('#moduleId').val(assignment.moduleId);
    $('#title').val(assignment.title);
    $('#description').val(assignment.description || assignment.instructions);
    $('#marks').val(assignment.marks);
    $('#isActive').val(assignment.isActive ? 'true' : 'false');
    $('#moduleSelect').val(assignment.moduleId || '0');

    setDueDateTimeFields(assignment.dueDateTime);

    if (assignment.filePath) {
        $('#assignmentFileName').text(getFileNameFromPath(assignment.filePath));
    }
}

function setDueDateTimeFields(dueDateTime) {
    if (!dueDateTime) return;

    var dateObj = new Date(dueDateTime);

    if (isNaN(dateObj.getTime())) return;

    var date = dateObj.toISOString().split('T')[0];

    var hours = dateObj.getHours().toString().padStart(2, '0');
    var minutes = dateObj.getMinutes().toString().padStart(2, '0');

    $('#dueDate').val(date);
    $('#dueTime').val(hours + ':' + minutes);
}

function getFileNameFromPath(filePath) {
    return filePath.split('/').pop();
}

function getAssignmentErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    if (err.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}

function showAssignmentError(message) {
    $('.error-box').html('<div>' + message + '</div>');
}

function showAssignmentFileName(event) {
    const input = event.target;
    const fileNameText = document.getElementById('assignmentFileName');

    if (input.files && input.files[0]) {
        fileNameText.textContent = input.files[0].name;
    } else {
        fileNameText.textContent = 'Choose assignment file';
    }
}