$(document).ready(function () {
    var courseId = $('#courseId').val();
    var isEdit = courseId && courseId !== '';

    loadTrainers(function () {
        if (isEdit) {
            setupEditMode();
            loadCourse(courseId);
        }
    });

    $('#courseCreateForm').on('submit', function (e) {
        e.preventDefault();
        saveCourse(courseId, isEdit);
    });
});

function setupEditMode() {
    $('#pageTitle').text('Edit Course');
    $('#pageSub').text('Update the course details.');
    $('#saveCourseBtn').text('Update Course');
}

function saveCourse(courseId, isEdit) {
    var btn = $('#saveCourseBtn');
    var formData = buildCourseFormData();

    setButtonLoading(btn, isEdit);

    $.ajax({
        url: isEdit ? '/api/Course/edit/' + courseId : '/api/Course/create',
        type: isEdit ? 'PUT' : 'POST',
        data: formData,
        processData: false,
        contentType: false,

        success: function (response) {
            sessionStorage.setItem("successMessage", response.message || "Course saved successfully.");
            window.location.href = $('#redirectUrl').val() || "/Admin/Courses/Index";
        },

        error: function (xhr) {
            showError(getErrorMessage(xhr));
        },

        complete: function () {
            resetButton(btn, isEdit);
        }
    });
}

function buildCourseFormData() {
    var formData = new FormData();

    formData.append('title', $('#title').val());
    formData.append('category', $('#category').val());
    formData.append('description', $('#description').val());
    formData.append('batchNumber', $('#batchNumber').val());
    formData.append('durationText', $('#durationText').val());
    formData.append('batchStartDate', $('#batchStartDate').val());
    formData.append('batchEndDate', $('#batchEndDate').val());
    formData.append('certificateIncluded', $('#certificateIncluded').val());
    formData.append('status', $('#status').val());
    formData.append('trainerId', $('#trainerId').val());

    appendFileIfSelected(formData, 'imageFile');
    appendFileIfSelected(formData, 'handbookFile');

    return formData;
}

function appendFileIfSelected(formData, inputId) {
    var input = $('#' + inputId)[0];

    if (input && input.files && input.files[0]) {
        formData.append(inputId, input.files[0]);
    }
}

function loadCourse(courseId) {
    $.ajax({
        url: '/api/Course/' + courseId,
        type: 'GET',

        success: function (course) {
            populateCourseForm(course);
        },

        error: function (xhr) {
            showError(getErrorMessage(xhr));
        }
    });
}

function populateCourseForm(course) {
    $('#title').val(course.title);
    $('#category').val(course.category);
    $('#description').val(course.description);
    $('#batchNumber').val(course.batchNumber);
    $('#durationText').val(course.durationText);
    $('#batchStartDate').val(formatDate(course.batchStartDate));
    $('#batchEndDate').val(formatDate(course.batchEndDate));
    $('#certificateIncluded').val(course.certificateIncluded ? 'true' : 'false');
    $('#status').val(course.isPublished ? 'Active' : 'Draft');
    $('#trainerId').val(course.trainerId);

    if (course.courseImagePath) {
        $('#imagePreview')
            .attr('src', course.courseImagePath)
            .show();

        $('#uploadPlaceholder').hide();
    }

    if (course.handbookFilePath) {
        $('#handbookFileName').text(getFileNameFromPath(course.handbookFilePath));
    }
}

function loadTrainers(callback) {
    $.ajax({
        url: '/api/User/trainers',
        type: 'GET',

        success: function (trainers) {
            var dropdown = $('#trainerId');

            dropdown.empty();
            dropdown.append(`<option value="">Select trainer</option>`);

            $.each(trainers, function (index, trainer) {
                dropdown.append(`
                    <option value="${trainer.id}">
                        ${trainer.name}
                    </option>
                `);
            });

            if (callback) callback();
        },

        error: function () {
            showError('Failed to load trainers.');
        }
    });
}

function formatDate(dateValue) {
    if (!dateValue) return '';
    return dateValue.substring(0, 10);
}

function getFileNameFromPath(filePath) {
    return filePath.split('/').pop();
}

function setButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');
}

function resetButton(btn, isEdit) {
    btn.prop('disabled', false).text(isEdit ? 'Update Course' : 'Save Course');
}

function getErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}

function showError(message) {
    $('.error-box').html('<div>' + message + '</div>');
}

function previewAdminImage(event) {
    const input = event.target;
    const preview = document.getElementById('imagePreview');
    const placeholder = document.getElementById('uploadPlaceholder');

    if (input.files && input.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = 'block';

            if (placeholder) {
                placeholder.style.display = 'none';
            }
        };

        reader.readAsDataURL(input.files[0]);
    }
}

function showHandbookFileName(event) {
    const input = event.target;
    const fileNameText = document.getElementById('handbookFileName');

    if (input.files && input.files[0]) {
        fileNameText.textContent = input.files[0].name;
    } else {
        fileNameText.textContent = 'Choose PDF / document file';
    }
}