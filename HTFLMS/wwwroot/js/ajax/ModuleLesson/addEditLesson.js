$(document).ready(function () {
    var courseId = $('#courseId').val();
    var moduleId = getInitialModuleId();
    var lessonId = $('#lessonId').val();
    var isEdit = isValidEditId(lessonId);

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

        var selectedModuleId = $('#moduleSelect').val();
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

                if (isValidEditId(selectedModuleId)) {
                    sessionStorage.setItem("selectedModuleId", selectedModuleId);
                } else {
                    sessionStorage.removeItem("selectedModuleId");
                }

                storeSuccessMessage(response);
                window.location.href = "/Trainer/ModulesLessons/Index/" + courseId;
            },

            error: handleAjaxError,

            complete: function () {
                resetLessonSaveButton(btn, isEdit);
            }
        });
    });

    $(document).on('click', '#cancelBtn', function (e) {
        e.preventDefault();

        var courseId = $('#courseId').val();
        var moduleId = $('#moduleSelect').val() || $('#moduleId').val();

        if (courseId && courseId !== '0') {
            sessionStorage.setItem("selectedCourseId", courseId);
        }

        if (moduleId && moduleId !== '0') {
            sessionStorage.setItem("selectedModuleId", moduleId);
        }

        sessionStorage.setItem("restoreSelectionAfterCancel", "true");

        window.location.href = "/Trainer/ModulesLessons/Index/" + courseId;
    });
});

function isValidEditId(id) {
    return id && id !== '' && id !== '0';
}

function storeSuccessMessage(response) {
    if (response?.message) {
        sessionStorage.setItem("successMessage", response.message);
    }
}

function getInitialModuleId() {
    var moduleId = $('#moduleId').val();

    if (!isValidEditId(moduleId)) {
        moduleId = '0';
        $('#moduleId').val(moduleId);
    }

    return moduleId;
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
    showErrorMessage(xhr);
}

function getAjaxMessage(xhr) {
    var err = xhr.responseJSON;

    if (err?.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err?.errorMessage || err?.title || err?.message || xhr.responseText;
}

function showErrorMessage(xhr) {
    $('.error-box').html('<div>' + getAjaxMessage(xhr) + '</div>');
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