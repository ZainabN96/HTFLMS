$(document).ready(function () {
    var trainerId = parseInt($('#trainerId').val()) || 0;

    setupImagePreview();

    if (trainerId > 0) {
        setupEditPage(trainerId);
    } else {
        setupAddPage();
    }

    $('#trainerForm').on('submit', function (e) {
        e.preventDefault();

        if (trainerId > 0) {
            updateTrainer(trainerId);
        } else {
            createTrainer();
        }
    });
});

function setupAddPage() {
    $('#pageTitle').text('Add New Trainer');
    $('#pageSubTitle').text('Create a new trainer profile and save all required details.');
    $('#saveTrainerBtn').text('Save Trainer');
}

function setupEditPage(trainerId) {
    $('#pageTitle').text('Edit Trainer');
    $('#pageSubTitle').text('Update trainer details and save changes.');
    $('#saveTrainerBtn').text('Update Trainer');

    $('#password').attr('placeholder', 'Leave empty to keep old password');
    $('#confirmPassword').attr('placeholder', 'Leave empty to keep old password');

    loadTrainer(trainerId);
}

function getTrainerFormData() {
    var formData = new FormData();

    formData.append('Name', $('#name').val() || '');
    formData.append('Email', $('#email').val() || '');
    formData.append('Password', $('#password').val() || '');
    formData.append('ConfirmPassword', $('#confirmPassword').val() || '');
    formData.append('Designation', $('#designation').val() || '');
    formData.append('CNIC', $('#cnic').val() || '');
    formData.append('MobileNumber', $('#mobileNumber').val() || '');
    formData.append('IsActive', $('#isActive').val() || '');
    formData.append('Gender', $('#gender').val() || '');
    formData.append('Qualification', $('#qualification').val() || '');
    formData.append('Address', $('#address').val() || '');

    var pictureInput = document.getElementById('Picture');

    if (pictureInput && pictureInput.files && pictureInput.files.length > 0) {
        formData.append('Picture', pictureInput.files[0]);
    }

    return formData;
}

function createTrainer() {
    clearErrors();

    $.ajax({
        url: '/api/ManageTrainer',
        type: 'POST',
        data: getTrainerFormData(),
        processData: false,
        contentType: false,

        success: function (response) {
            sessionStorage.setItem('successMessage', response?.message || 'Trainer added successfully.');
            window.location.href = '/Admin/ManageTrainers/Trainers';
        },

        error: function (xhr) {
            showErrors(xhr);
        }
    });
}

function updateTrainer(trainerId) {
    clearErrors();

    $.ajax({
        url: '/api/ManageTrainer/' + trainerId,
        type: 'PUT',
        data: getTrainerFormData(),
        processData: false,
        contentType: false,

        success: function (response) {
            sessionStorage.setItem('successMessage', response?.message || 'Trainer updated successfully.');
            window.location.href = '/Admin/ManageTrainers/Trainers';
        },

        error: function (xhr) {
            showErrors(xhr);
        }
    });
}

function loadTrainer(trainerId) {
    $.ajax({
        url: '/api/ManageTrainer/' + trainerId,
        type: 'GET',

        success: function (trainer) {
            $('#name').val(trainer.name ?? trainer.Name ?? '');
            $('#email').val(trainer.email ?? trainer.Email ?? '');
            $('#designation').val(trainer.designation ?? trainer.Designation ?? '');
            $('#cnic').val(trainer.cnic ?? trainer.CNIC ?? '');
            $('#mobileNumber').val(trainer.mobileNumber ?? trainer.MobileNumber ?? '');
            $('#gender').val(trainer.gender ?? trainer.Gender ?? '');
            $('#qualification').val(trainer.qualification ?? trainer.Qualification ?? '');
            $('#address').val(trainer.address ?? trainer.Address ?? '');

            var isActive = trainer.isActive ?? trainer.IsActive;
            $('#isActive').val(isActive === true ? 'true' : 'false');

            var picturePath = trainer.profilePicturePath ?? trainer.ProfilePicturePath;

            if (picturePath) {
                $('#trainerPicturePreview').attr('src', picturePath).show();
                $('#trainerPicturePlaceholder').hide();
            }
        },

        error: function (xhr) {
            showFormSummary(getMessage(xhr));
        }
    });
}

function clearErrors() {
    $('.field-error').text('');
    $('#formErrorSummary').html('');
}

function showErrors(xhr) {
    var err = xhr.responseJSON;

    if (err?.errors) {
        var summary = [];

        Object.keys(err.errors).forEach(function (key) {
            var fieldName = key.charAt(0).toLowerCase() + key.slice(1);
            var message = err.errors[key].join('<br>');

            $('[data-field="' + fieldName + '"]').html(message);
            summary.push(message);
        });

        $('#formErrorSummary').html(summary.join('<br>'));
        return;
    }

    showFormSummary(getMessage(xhr));
}

function showFormSummary(message) {
    $('#formErrorSummary').html(message);
}

function getMessage(xhr) {
    var err = xhr.responseJSON;

    if (err?.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err?.message || err?.errorMessage || err?.title || xhr.responseText || 'Something went wrong.';
}

function setupImagePreview() {
    const input = document.getElementById("Picture");
    const preview = document.getElementById("trainerPicturePreview");
    const placeholder = document.getElementById("trainerPicturePlaceholder");

    if (!input || !preview || !placeholder) return;

    input.addEventListener("change", function () {
        const file = this.files && this.files[0];

        if (!file) {
            preview.style.display = "none";
            preview.src = "";
            placeholder.style.display = "grid";
            return;
        }

        const reader = new FileReader();

        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = "block";
            placeholder.style.display = "none";
        };

        reader.readAsDataURL(file);
    });
}