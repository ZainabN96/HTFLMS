$(document).ready(function () {
    var courseId = getInitialCourseId();
    var moduleId = getInitialModuleId();
    var lessonId = $('#lessonId').val();
    var isEdit = isValidEditId(lessonId);

    if (!isValidEditId(courseId)) {
        showErrorText('Please go back and select a course first.');
        return;
    }

    if (isEdit) {
        setLessonEditMode();
    }

    loadModulesForLesson(courseId, moduleId, function () {
        if (isEdit) {
            loadLesson(lessonId);
        }
    });

    $('#lessonCreateForm').on('submit', function (e) {
        e.preventDefault();

        $('.error-box').html('');

        var selectedModuleId = $('#moduleSelect').val();

        if (!isValidEditId(selectedModuleId)) {
            showErrorText('Please select a module first.');
            return;
        }

        var btn = $('#saveLessonBtn');
        setButtonLoading(btn, isEdit);

        $.ajax({
            url: getLessonSaveUrl(lessonId, isEdit),
            type: isEdit ? 'PUT' : 'POST',
            data: buildLessonFormData(selectedModuleId),
            processData: false,
            contentType: false,

            success: function (response) {
                sessionStorage.setItem("selectedCourseId", courseId);
                sessionStorage.setItem("selectedModuleId", selectedModuleId);
                sessionStorage.setItem("restoreSelectionAfterCancel", "true");

                storeSuccessMessage(response);

                window.location.href = "/Admin/ModulesLessons/Index/" + courseId;
            },

            error: handleAjaxError,

            complete: function () {
                resetLessonSaveButton(btn, isEdit);
            }
        });
    });

    $(document).on('click', '#cancelBtn', function (e) {
        e.preventDefault();

        var selectedModuleId = $('#moduleSelect').val() || $('#moduleId').val();

        if (isValidEditId(courseId)) {
            sessionStorage.setItem("selectedCourseId", courseId);
        }

        if (isValidEditId(selectedModuleId)) {
            sessionStorage.setItem("selectedModuleId", selectedModuleId);
        }

        sessionStorage.setItem("restoreSelectionAfterCancel", "true");

        window.location.href = "/Admin/ModulesLessons/Index/" + courseId;
    });
});

function isValidEditId(id) {
    return id && id !== '' && id !== '0';
}

function getInitialCourseId() {
    var courseId = $('#courseId').val();

    if (!isValidEditId(courseId)) {
        courseId = sessionStorage.getItem("selectedCourseId") || '0';
        $('#courseId').val(courseId);
    }

    return courseId;
}

function getInitialModuleId() {
    var moduleId = $('#moduleId').val();

    if (!isValidEditId(moduleId)) {
        moduleId = sessionStorage.getItem("selectedModuleId") || '0';
        $('#moduleId').val(moduleId);
    }

    return moduleId;
}

function storeSuccessMessage(response) {
    if (response?.message) {
        sessionStorage.setItem("successMessage", response.message);
    }
}

function setLessonEditMode() {
    $('.dashboard-h1').text('Edit Lesson');
    $('.dashboard-sub').text('Update the lesson and keep it assigned to the correct module.');
    $('.dashboard-muted-small').text('Update lesson content for the selected course and module.');
    $('#saveLessonBtn').html('<i class="bi bi-check-circle"></i> Update Lesson');
}

function getLessonSaveUrl(lessonId, isEdit) {
    return isEdit
        ? '/api/Lesson/edit/' + lessonId
        : '/api/Lesson/create';
}

function buildLessonFormData(selectedModuleId) {
    var formData = new FormData();

    formData.append('ModuleId', selectedModuleId);
    formData.append('Title', $('#title').val());
    formData.append('Description', $('#description').val());
    formData.append('DisplayOrder', $('#displayOrder').val() || 1);
    formData.append('IsActive', $('#isActive').val());

    return formData;
}

function setButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');
}

function resetLessonSaveButton(btn, isEdit) {
    btn.prop('disabled', false).html(
        isEdit
            ? '<i class="bi bi-check-circle"></i> Update Lesson'
            : '<i class="bi bi-plus-circle"></i> Save Lesson'
    );
}

function handleAjaxError(xhr) {
    console.log(xhr.responseText);
    showErrorText(getAjaxMessage(xhr));
}

function getAjaxMessage(xhr) {
    var err = xhr.responseJSON;

    if (err?.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err?.errorMessage || err?.innerError || err?.title || err?.message || xhr.responseText || 'Something went wrong.';
}

function showErrorText(message) {
    $('.error-box').html('<div>' + message + '</div>');
}

function loadModulesForLesson(courseId, selectedModuleId, callback) {
    $.ajax({
        url: '/api/Module/course/' + courseId,
        type: 'GET',

        success: function (modules) {
            var moduleSelect = $('#moduleSelect');

            moduleSelect.html('<option value="">Select module</option>');

            if (!modules || modules.length === 0) {
                moduleSelect.html('<option value="">No modules found</option>');
                return;
            }

            $.each(modules, function (_, module) {
                moduleSelect.append(`
                    <option value="${module.id}">
                        ${module.title}
                    </option>
                `);
            });

            if (isValidEditId(selectedModuleId)) {
                moduleSelect.val(selectedModuleId);
            }

            if (callback) {
                callback();
            }
        },

        error: handleAjaxError
    });
}

function loadLesson(lessonId) {
    $.ajax({
        url: '/api/Lesson/' + lessonId,
        type: 'GET',

        success: function (lesson) {
            $('#moduleSelect').val(lesson.moduleId);
            $('#moduleId').val(lesson.moduleId);
            $('#title').val(lesson.title);
            $('#description').val(lesson.description);
            $('#displayOrder').val(lesson.displayOrder);
            $('#isActive').val(lesson.isActive ? 'true' : 'false');
        },

        error: handleAjaxError
    });
}