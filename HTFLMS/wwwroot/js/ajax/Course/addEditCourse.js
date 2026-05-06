$(document).ready(function () {

    var courseId = $('#courseId').val();
    var isEdit = courseId && courseId !== '';
    var redirectUrl = $('#redirectUrl').val() || '/Trainer/Courses/Index';
    var isAdminPage = $('#isAdminPage').val() === 'true';

    if (isAdminPage) {
        loadTrainers();
    }

    if (isEdit) {
        $('#pageTitle').text('Edit Course');
        $('#pageSub').text('Update the course details.');
        $('#saveCourseBtn').text('Update Course');

        loadCourse(courseId);
    }

    $('#courseCreateForm').on('submit', function (e) {
        e.preventDefault();

        $('.error-box').html('');

        var formData = new FormData();

        formData.append('title', $('#title').val());
        formData.append('category', $('#category').val());
        formData.append('description', $('#description').val());
        formData.append('batchNumber', $('#batchNumber').val());
        formData.append('batchStartDate', $('#batchStartDate').val());
        formData.append('batchEndDate', $('#batchEndDate').val());
        formData.append('certificateIncluded', $('#certificateIncluded').val());
        formData.append('status', $('#status').val());
        formData.append('trainerId', $('#trainerId').val());

        var imageFile = $('#imageFile')[0].files[0];
        if (imageFile) {
            formData.append('imageFile', imageFile);
        }

        var handbookFile = $('#handbookFile')[0].files[0];
        if (handbookFile) {
            formData.append('handbookFile', handbookFile);
        }

        var btn = $('#saveCourseBtn');
        btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');

        $.ajax({
            url: isEdit ? '/api/Course/edit/' + courseId : '/api/Course/create',
            type: isEdit ? 'PUT' : 'POST',
            data: formData,
            processData: false,
            contentType: false,

            success: function () {
                sessionStorage.setItem(
                    "successMessage",
                    isEdit ? "Course updated successfully!" : "Course created successfully!"
                );

                window.location.href = redirectUrl;
            },

            error: function (xhr) {
                var err = xhr.responseJSON;
                var msg = err?.errorMessage || err?.message || err?.title || 'Something went wrong. Please try again.';
                $('.error-box').html('<div>' + msg + '</div>');
            },

            complete: function () {
                btn.prop('disabled', false).text(isEdit ? 'Update Course' : 'Save Course');
            }
        });
    });
});

function loadTrainers() {
    $.ajax({
        url: '/api/User/trainers',
        type: 'GET',

        success: function (trainers) {
            var trainerDropdown = $('#trainerId');
            trainerDropdown.empty();

            trainerDropdown.append('<option value="">Select trainer</option>');

            $.each(trainers, function (index, trainer) {
                trainerDropdown.append(
                    '<option value="' + trainer.id + '">' + trainer.name + '</option>'
                );
            });

            var selectedTrainerId = $('#selectedTrainerId').val();
            if (selectedTrainerId) {
                trainerDropdown.val(selectedTrainerId);
            }
        },

        error: function () {
            $('.error-box').html('<div>Trainers could not be loaded.</div>');
        }
    });
}

function loadCourse(courseId) {
    $.ajax({
        url: '/api/Course/' + courseId,
        type: 'GET',

        success: function (course) {
            $('#title').val(course.title);
            $('#category').val(course.category);
            $('#description').val(course.description);
            $('#batchNumber').val(course.batchNumber);
            $('#batchStartDate').val(formatDate(course.batchStartDate));
            $('#batchEndDate').val(formatDate(course.batchEndDate));
            $('#certificateIncluded').val(course.certificateIncluded ? 'true' : 'false');
            $('#status').val(course.isPublished ? 'Active' : 'Draft');

            $('#selectedTrainerId').val(course.trainerId);

            if ($('#trainerId').is('select')) {
                $('#trainerId').val(course.trainerId);
            }

            if (course.courseImagePath) {
                $('#imagePreview')
                    .attr('src', course.courseImagePath)
                    .show();

                $('#uploadPlaceholder').hide();
            }

            if (course.handbookFilePath) {
                var fileName = course.handbookFilePath.split('/').pop();
                $('#handbookFileName').text(fileName);
            }
        },

        error: function () {
            $('.error-box').html('<div>Course could not be loaded.</div>');
        }
    });
}

function formatDate(dateValue) {
    if (!dateValue) return '';

    var date = new Date(dateValue);
    return date.toISOString().split('T')[0];
}

function previewTrainerImage(event) {
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