$(document).ready(function () {

    var pageState = getMaterialPageState();

    loadModulesForMaterial(pageState.courseId, pageState.moduleId);

    if (pageState.isEdit) {
        setupEditMode();
        loadMaterial(pageState.materialId);
    }

    $('#contentType').on('change', toggleMaterialFields);

    $('#materialCreateForm').on('submit', function (e) {
        e.preventDefault();
        saveMaterial(pageState);
    });

    toggleMaterialFields();
});

function getMaterialPageState() {
    var materialId = $('#materialId').val();

    return {
        courseId: $('#courseId').val(),
        moduleId: $('#moduleId').val(),
        lessonId: $('#lessonId').val(),
        materialId: materialId,
        isEdit: materialId && materialId !== '' && materialId !== '0'
    };
}

function setupEditMode() {
    $('.dashboard-h1').text('Edit Material');
    $('.dashboard-sub').text('Update slides, files, uploaded videos, or lecture links for students.');
    $('#saveMaterialBtn').text('Update Material');
}

function saveMaterial(pageState) {
    var selectedModuleId = $('#moduleSelect').val();
    var btn = $('#saveMaterialBtn');

    setMaterialButtonLoading(btn, pageState.isEdit);

    $.ajax({
        url: pageState.isEdit
            ? '/api/Material/edit/' + pageState.materialId
            : '/api/Material/create',

        type: pageState.isEdit ? 'PUT' : 'POST',
        data: buildMaterialFormData(pageState, selectedModuleId),
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
            showMaterialError(getMaterialErrorMessage(xhr));
        },

        complete: function () {
            resetMaterialButton(btn, pageState.isEdit);
        }
    });
}

function buildMaterialFormData(pageState, selectedModuleId) {
    var formData = new FormData();

    formData.append('CourseId', pageState.courseId);

    if (selectedModuleId && selectedModuleId !== '0') {
        formData.append('ModuleId', selectedModuleId);
    }

    if (pageState.lessonId && pageState.lessonId !== '0') {
        formData.append('LessonId', pageState.lessonId);
    }

    formData.append('Title', $('#title').val());
    formData.append('ContentType', $('#contentType').val());
    formData.append('ExternalUrl', $('#externalUrl').val());
    formData.append('IsActive', $('#isActive').val() === 'true');
    formData.append('Pages', $('#pages').val());
    formData.append('Slides', $('#slides').val());
    formData.append('Minutes', $('#minutes').val());

    appendMaterialFileIfSelected(formData);

    return formData;
}

function appendMaterialFileIfSelected(formData) {
    var file = $('#file')[0].files[0];

    if (file) {
        formData.append('File', file);
    }
}

function setMaterialButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Creating...');
}

function resetMaterialButton(btn, isEdit) {
    btn.prop('disabled', false).text(isEdit ? 'Update Material' : 'Create Material');
}

function toggleMaterialFields() {
    var type = $('#contentType').val();

    hideMaterialSpecificFields();

    if (type === 'PDF') {
        $('#fileUploadField').show();
        $('#pagesField').show();
    }

    if (type === 'PPTX') {
        $('#fileUploadField').show();
        $('#slidesField').show();
    }

    if (type === 'MP4 Upload') {
        $('#fileUploadField').show();
        $('#minutesField').show();
    }

    if (type === 'Video Link') {
        $('#videoLinkField').show();
        $('#minutesField').show();
    }
}

function hideMaterialSpecificFields() {
    $('#fileUploadField').hide();
    $('#videoLinkField').hide();
    $('#pagesField').hide();
    $('#slidesField').hide();
    $('#minutesField').hide();
}

function loadModulesForMaterial(courseId, selectedModuleId) {
    $.ajax({
        url: '/api/Module/course/' + courseId,
        type: 'GET',

        success: function (modules) {
            populateModuleSelectForMaterial(modules, selectedModuleId);
        }
    });
}

function populateModuleSelectForMaterial(modules, selectedModuleId) {
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

function loadMaterial(materialId) {
    $.ajax({
        url: '/api/Material/' + materialId,
        type: 'GET',

        success: function (material) {
            populateMaterialForm(material);
        },

        error: function (xhr) {
            showMaterialError(getMaterialErrorMessage(xhr));
        }
    });
}

function populateMaterialForm(material) {
    $('#courseId').val(material.courseId);
    $('#moduleId').val(material.moduleId);
    $('#lessonId').val(material.lessonId);
    $('#title').val(material.title);
    $('#contentType').val(material.contentType);
    $('#externalUrl').val(material.externalUrl);
    $('#isActive').val(material.isActive ? 'true' : 'false');
    $('#pages').val(material.pages);
    $('#slides').val(material.slides);
    $('#minutes').val(material.minutes);
    $('#moduleSelect').val(material.moduleId || '0');
    if (material.filePath) {
        $('#materialFileName').text(getFileNameFromPath(material.filePath));
    }
    toggleMaterialFields();
}

function getFileNameFromPath(filePath) {
    return filePath.split('/').pop();
}

function getMaterialErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    if (err.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}

function showMaterialError(message) {
    $('.error-box').html('<div>' + message + '</div>');
}

function showMaterialFileName(event) {
    const input = event.target;
    const fileNameText = document.getElementById('materialFileName');

    if (input.files && input.files[0]) {
        fileNameText.textContent = input.files[0].name;
    } else {
        fileNameText.textContent = 'Choose PDF / PPT / MP4 file';
    }
}