
$(document).ready(function () {
    function getDefaultTitlePrefix(gender) {
        switch (gender) {
            case 'Male':
                return 'Mr.';

            case 'Female':
            case 'Other':
                return 'Ms.';

            default:
                return '';
        }
    }

    function updateTitlePrefix() {
        const gender = $('#Gender').val();
        const titlePrefix = getDefaultTitlePrefix(gender);

        $('#TitlePrefix').val(titlePrefix);
    }

    $('#Gender').on('change', updateTitlePrefix);

    updateTitlePrefix();

    $('#registerForm').on('submit', function (e) {

        e.preventDefault();

        var form = $(this);
        var btn = $('#submitBtn');
        var errorBox = $('.error-box');

        errorBox.empty();

        if (!form.valid()) {
            return;
        }

        var payload = {
            email: $('#Email').val(),
            password: $('#Password').val(),
            gender: $('#Gender').val(),
            titlePrefix: $('#TitlePrefix').val(),
            name: $('#Name').val(),
            qualification: $('#Qualification').val(),
            cnic: $('#CNIC').val(),
            address: $('#Address').val(),
            country: $('#Country').val(),
            city: $('#City').val(),
            mobileNumber: $('#MobileNumber').val(),
            linkedIn: $('#LinkedIn').val(),
            employmentStatus: $('#EmploymentStatus').val()
        };

        btn.prop('disabled', true).text('Submitting...');

        $.ajax({
            url: '/api/User/register',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),

            success: function (res) {

                localStorage.setItem(
                    "loginSuccessMessage",
                    res.message || "Registered successfully! Please login."
                );

                localStorage.setItem(
                    "prefillLoginEmail",
                    res.email || $('#Email').val()
                );

                var returnUrl = $('#returnUrl').val();
                var loginUrl = '/Account/Login';

                if (returnUrl) {
                    loginUrl += '?returnUrl=' +
                        encodeURIComponent(returnUrl);
                }

                window.location.href = loginUrl;
            },

            error: function (xhr) {

                var err = xhr.responseJSON;

                var msg = 'Registration failed. Please try again.';

                if (err && err.message) {
                    msg = err.message;
                }
                else if (err && err.errorMessage) {
                    msg = err.errorMessage;
                }
                else if (err && err.errors) {

                    var messages = [];

                    Object.keys(err.errors).forEach(function (key) {
                        messages = messages.concat(err.errors[key]);
                    });

                    if (messages.length > 0) {
                        msg = messages.join(' ');
                    }
                }

                errorBox.text(msg);
            },

            complete: function () {

                btn.prop('disabled', false)
                    .text('Submit Your Application Today!');
            }
        });
    });
});
