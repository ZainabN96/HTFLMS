$(document).ready(function () {
    var moduleId = $('#moduleId').val();
    var isEdit = isValidEditId(moduleId);

    if (isEdit) {
        setModuleEditMode();
        loadModule(moduleId);
    }

    $('#moduleCreateForm').on('submit', function (e) {
        e.preventDefault();

        $('.error-box').html('');

        var courseId = $('#courseId').val();

        if (!isValidEditId(courseId)) {
            showErrorText('Please select a course first.');
            return;
        }

        var btn = $('#saveModuleBtn');
        setButtonLoading(btn, isEdit);

        $.ajax({
            url: getModuleSaveUrl(moduleId, isEdit),
            type: isEdit ? 'PUT' : 'POST',
            data: buildModuleFormData(),
            processData: false,
            contentType: false,

            success: function (response) {
                sessionStorage.setItem("selectedCourseId", $('#courseId').val());
                sessionStorage.removeItem("selectedModuleId");
                sessionStorage.setItem("restoreSelectionAfterCancel", "true");

                storeSuccessMessage(response);

                window.location.href = "/Admin/ModulesLessons/Index/" + $('#courseId').val();
            },

            error: handleAjaxError,

            complete: function () {
                resetModuleSaveButton(btn, isEdit);
            }
        });
    });

    $(document).on('click', '#cancelBtn', function (e) {
        e.preventDefault();

        var courseId = $('#courseId').val();

        if (isValidEditId(courseId)) {
            sessionStorage.setItem("selectedCourseId", courseId);
        }

        if (isEdit) {
            sessionStorage.setItem("selectedModuleId", moduleId);
        } else {
            sessionStorage.removeItem("selectedModuleId");
        }

        sessionStorage.setItem("restoreSelectionAfterCancel", "true");

        window.location.href = "/Admin/ModulesLessons/Index/" + courseId;
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

function setModuleEditMode() {
    $('.dashboard-h1').text('Edit Module');
    $('.dashboard-sub').text('Update the module details.');
    $('.dashboard-muted-small').text('Update the basic information for this module.');
    $('#saveModuleBtn').html('<i class="bi bi-check-circle"></i> Update Module');
}

function getModuleSaveUrl(moduleId, isEdit) {
    return isEdit
        ? '/api/Module/edit/' + moduleId
        : '/api/Module/create';
}

function buildModuleFormData() {
    var formData = new FormData();

    formData.append('CourseId', $('#courseId').val());
    formData.append('Title', $('#title').val());
    formData.append('Description', $('#description').val());
    formData.append('DisplayOrder', $('#displayOrder').val() || 1);
    formData.append('IsActive', $('#isActive').val());
    formData.append('IsAccessible', $('#isAccessible').is(':checked'));

    return formData;
}

function setButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');
}

function resetModuleSaveButton(btn, isEdit) {
    btn.prop('disabled', false).html(
        isEdit
            ? '<i class="bi bi-check-circle"></i> Update Module'
            : '<i class="bi bi-plus-circle"></i> Save Module'
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

function loadModule(moduleId) {
    $.ajax({
        url: '/api/Module/' + moduleId,
        type: 'GET',

        success: function (module) {
            $('#courseId').val(module.courseId);
            $('#title').val(module.title);
            $('#description').val(module.description);
            $('#displayOrder').val(module.displayOrder);
            $('#isActive').val(module.isActive ? 'true' : 'false');
            $('#isAccessible').prop('checked', module.isAccessible);
        },

        error: handleAjaxError
    });
}