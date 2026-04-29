$(document).ready(function () {

    $('#courseCreateForm').on('submit', function (e) {
        e.preventDefault();

        var formData = new FormData();

        formData.append('title', $('#title').val());
        formData.append('category', $('#category').val());
        formData.append('description', $('#description').val());
        formData.append('batchNumber', $('#batchNumber').val());
        formData.append('batchStartDate', $('#batchStartDate').val());
        formData.append('batchEndDate', $('#batchEndDate').val());
        formData.append('certificateIncluded', $('#certificateIncluded').val());
        formData.append('status', $('#status').val());

        // temporary trainer id
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
        btn.prop('disabled', true).text('Saving...');

        $.ajax({
            url: '/api/Course/create',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,

            success: function (res) {
                sessionStorage.setItem("successMessage", "Course created successfully!");
                window.location.href = "/Trainer/Courses/Index";
            },

            error: function (xhr) {
                var err = xhr.responseJSON;
                var msg = err?.errorMessage || err?.title || 'Course creation failed. Please try again.';
                $('.error-box').html('<div>' + msg + '</div>');
            },

            complete: function () {
                btn.prop('disabled', false).text('Save Course');
            }
        });
    });
});

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

