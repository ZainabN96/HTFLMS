//$(document).ready(function () {

//    var courseId = $('#courseId').val();

//    loadCourseDetail(courseId);

//    $('#openEnrollmentModalBtn').on('click', function () {
//        $('#enrollmentConfirmModal').show();
//        $('body').css('overflow', 'hidden');
//    });

//    $('#closeEnrollmentModalBtn, #cancelEnrollmentBtn, #enrollmentModalBackdrop').on('click', function () {
//        closeEnrollmentModal();
//    });

//    $('#confirmEnrollmentBtn').on('click', function () {
//        enrollInCourse(courseId);
//    });
//});

//function loadCourseDetail(courseId) {
//    clearEnrollmentAlert();

//    $.ajax({
//        url: '/api/student/courses/' + courseId,
//        type: 'GET',

//        success: function (course) {

//            var title = course.title || 'Untitled Course';
//            var category = course.category || '-';
//            var description = course.description || 'Course description coming soon.';
//            var trainerName = course.trainerName || 'No Trainer';
//            var certificateText = course.certificateIncluded ? 'Certificate Included' : 'Certificate Not Included';
//            var imagePath = course.courseImagePath || '/img/course/course-1.webp';

//            $('#courseTitle').text(title);
//            $('#overviewTitle').text(title);
//            $('#courseCategory').text(category);
//            $('#courseDescription').text(description);
//            $('#aboutCourse').text(description);

//            $('#trainerName').text(trainerName);
//            $('#sideTrainerName').text(trainerName);

//            $('#certificateIncluded').text(certificateText);
//            $('#sideCertificateIncluded').text(course.certificateIncluded ? 'Included' : 'Not Included');

//            $('#sideCategory').text(category);
//            $('#batchNumber').text(course.batchNumber || '-');
//            $('#batchStartDate').text(formatDisplayDate(course.batchStartDate));
//            $('#batchEndDate').text(course.batchEndDate ? formatDisplayDate(course.batchEndDate) : 'Not decided');
//            $('#durationText').text(course.durationText || '-');

//            $('#courseImage').attr('src', imagePath);
//            $('#sideCourseImage').attr('src', imagePath);

//            if (course.handbookFilePath) {
//                $('#handbookBtn').attr('href', course.handbookFilePath);
//                $('#handbookBlock').show();
//            }
//            else {
//                $('#handbookBlock').hide();
//            }
//        },

//        error: function (xhr) {
//            var msg = getErrorMessage(xhr, 'Course detail could not be loaded.');

//            showEnrollmentAlert(
//                'danger',
//                'Course Detail Error',
//                msg
//            );

//            $('#openEnrollmentModalBtn').prop('disabled', true);
//        }
//    });
//}

//function enrollInCourse(courseId) {

//    clearEnrollmentAlert();

//    var btn = $('#confirmEnrollmentBtn');
//    btn.prop('disabled', true).text('Enrolling...');

//    $.ajax({
//        url: '/api/student/enrollment/enroll',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify({
//            courseId: parseInt(courseId)
//        }),

//        success: function (response) {
//            sessionStorage.setItem('successMessage', response.message || 'Enrollment completed successfully.');
//            window.location.href = '/Student/Student/Index';
//        },

//        error: function (xhr) {
//            btn.prop('disabled', false).text('Yes, Enroll Me');

//            var msg = getErrorMessage(xhr, 'Enrollment failed.');

//            closeEnrollmentModal();

//            if (msg === 'You are already enrolled in this course.') {
//                showEnrollmentAlert(
//                    'success',
//                    'Already Enrolled',
//                    msg
//                );

//                $('#openEnrollmentModalBtn').hide();
//            }
//            else if (msg === 'You are already enrolled in another active course.') {
//                showEnrollmentAlert(
//                    'warning',
//                    'Enrollment Not Allowed',
//                    msg
//                );

//                $('#openEnrollmentModalBtn').hide();
//            }
//            else {
//                showEnrollmentAlert(
//                    'danger',
//                    'Enrollment Failed',
//                    msg
//                );
//            }
//        }
//    });
//}

//function showEnrollmentAlert(type, title, message) {
//    var alertClass = 'alert-info';
//    var iconClass = 'bi-info-circle-fill';

//    if (type === 'success') {
//        alertClass = 'alert-success';
//        iconClass = 'bi-check-circle-fill';
//    }
//    else if (type === 'warning') {
//        alertClass = 'alert-warning';
//        iconClass = 'bi-exclamation-triangle-fill';
//    }
//    else if (type === 'danger') {
//        alertClass = 'alert-danger';
//        iconClass = 'bi-x-circle-fill';
//    }

//    $('#enrollmentErrorBox').html(
//        '<div class="alert ' + alertClass + ' d-flex align-items-start gap-2 mb-0" role="alert">' +
//        '<i class="bi ' + iconClass + ' mt-1"></i>' +
//        '<div>' +
//        '<strong>' + encodeHtml(title) + '</strong><br />' +
//        encodeHtml(message) +
//        '</div>' +
//        '</div>'
//    );
//}

//function clearEnrollmentAlert() {
//    $('#enrollmentErrorBox').html('');
//}

//function closeEnrollmentModal() {
//    $('#enrollmentConfirmModal').hide();
//    $('body').css('overflow', '');
//}

//function formatDisplayDate(dateValue) {
//    if (!dateValue) return '-';

//    var date = new Date(dateValue);

//    return date.toLocaleDateString('en-GB', {
//        day: '2-digit',
//        month: 'short',
//        year: 'numeric'
//    });
//}

//function getErrorMessage(xhr, defaultMessage) {
//    if (xhr.responseJSON) {
//        return xhr.responseJSON.errorMessage ||
//            xhr.responseJSON.message ||
//            xhr.responseJSON.title ||
//            defaultMessage;
//    }

//    if (xhr.responseText) {
//        return xhr.responseText;
//    }

//    return defaultMessage;
//}

//function encodeHtml(value) {
//    return $('<div/>').text(value || '').html();
//}



$(document).ready(function () {

    var courseId = $('#courseId').val();

    loadCourseDetail(courseId);

    $('#openEnrollmentModalBtn').on('click', function () {
        $('#enrollmentConfirmModal').show();
        $('body').css('overflow', 'hidden');
    });

    $('#closeEnrollmentModalBtn, #cancelEnrollmentBtn, #enrollmentModalBackdrop').on('click', function () {
        closeEnrollmentModal();
    });

    $('#confirmEnrollmentBtn').on('click', function () {
        enrollInCourse(courseId);
    });
});

function loadCourseDetail(courseId) {
    clearEnrollmentAlert();

    $.ajax({
        url: '/api/student/courses/' + courseId,
        type: 'GET',

        success: function (course) {

            var title = course.title || 'Untitled Course';
            var category = course.category || '-';
            var description = course.description || 'Course description coming soon.';
            var trainerName = course.trainerName || 'No Trainer';
            var certificateText = course.certificateIncluded ? 'Certificate Included' : 'Certificate Not Included';
            var imagePath = course.courseImagePath || '/img/course/course-1.webp';

            $('#courseTitle').text(title);
            $('#overviewTitle').text(title);
            $('#courseCategory').text(category);
            $('#courseDescription').text(description);
            $('#aboutCourse').text(description);

            $('#trainerName').text(trainerName);
            $('#sideTrainerName').text(trainerName);

            $('#certificateIncluded').text(certificateText);
            $('#sideCertificateIncluded').text(course.certificateIncluded ? 'Included' : 'Not Included');

            $('#sideCategory').text(category);
            $('#batchNumber').text(course.batchNumber || '-');
            $('#batchStartDate').text(formatDisplayDate(course.batchStartDate));
            $('#batchEndDate').text(course.batchEndDate ? formatDisplayDate(course.batchEndDate) : 'Not decided');
            $('#durationText').text(course.durationText || '-');

            $('#courseImage').attr('src', imagePath);
            $('#sideCourseImage').attr('src', imagePath);

            if (course.handbookFilePath) {
                $('#handbookBtn').attr('href', course.handbookFilePath);
                $('#handbookBlock').show();
            }
            else {
                $('#handbookBlock').hide();
            }
        },

        error: function (xhr) {
            var msg = getErrorMessage(xhr, 'Course detail could not be loaded.');

            showEnrollmentAlert(
                'danger',
                'Course Detail Error',
                msg
            );

            $('#openEnrollmentModalBtn').prop('disabled', true);
        }
    });
}

function enrollInCourse(courseId) {

    clearEnrollmentAlert();

    var btn = $('#confirmEnrollmentBtn');
    btn.prop('disabled', true).text('Enrolling...');

    $.ajax({
        url: '/api/student/enrollment/enroll',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            courseId: parseInt(courseId)
        }),

        success: function (response) {
            sessionStorage.setItem('successMessage', response.message || 'Enrollment completed successfully.');
            window.location.href = '/Student/Student/Index';
        },

        error: function (xhr) {
            btn.prop('disabled', false).text('Yes, Enroll Me');

            var msg = getErrorMessage(xhr, 'Enrollment failed.');

            closeEnrollmentModal();

            if (msg === 'You are already enrolled in this course.') {
                showEnrollmentAlert(
                    'success',
                    'Already Enrolled',
                    msg
                );

                //$('#openEnrollmentModalBtn').hide();
            }
            else if (msg === 'You are already enrolled in another active course.') {
                showEnrollmentAlert(
                    'warning',
                    'Enrollment Not Allowed',
                    msg
                );

                //$('#openEnrollmentModalBtn').hide();
            }
            else {
                showEnrollmentAlert(
                    'danger',
                    'Enrollment Failed',
                    msg
                );
            }
        }
    });
}

function showEnrollmentAlert(type, title, message) {
    var iconClass = 'bi-info-circle-fill';

    if (type === 'success') {
        iconClass = 'bi-check-circle-fill';
    }
    else if (type === 'warning') {
        iconClass = 'bi-exclamation-triangle-fill';
    }
    else if (type === 'danger') {
        iconClass = 'bi-x-circle-fill';
    }

    $('#globalEnrollmentAlert').remove();
    $('#enrollmentErrorBox').html('');

    var alertHtml = '';

    alertHtml += '<div id="globalEnrollmentAlert" class="position-fixed top-0 start-50 translate-middle-x mt-5 pt-5 col-10 col-sm-8 col-md-6 col-lg-4">';
    alertHtml += '    <div class="alert bg-danger text-white border-0 shadow-lg rounded-5 d-flex align-items-center gap-3 mb-0" role="alert">';
    alertHtml += '        <span class="d-flex align-items-center justify-content-center rounded-circle bg-white bg-opacity-25 p-2">';
    alertHtml += '            <i class="bi ' + iconClass + ' fs-4"></i>';
    alertHtml += '        </span>';
    alertHtml += '        <div>';
    alertHtml += '            <div class="fw-bold">' + encodeHtml(title) + '</div>';
    alertHtml += '            <div>' + encodeHtml(message) + '</div>';
    alertHtml += '        </div>';
    alertHtml += '    </div>';
    alertHtml += '</div>';

    $('body').append(alertHtml);

    setTimeout(function () {
        $('#globalEnrollmentAlert').fadeOut(400, function () {
            $(this).remove();
        });
    }, 3500);
}

function clearEnrollmentAlert() {
    $('#enrollmentErrorBox').html('');
    $('#globalEnrollmentAlert').remove();
}

function closeEnrollmentModal() {
    $('#enrollmentConfirmModal').hide();
    $('body').css('overflow', '');
}

function formatDisplayDate(dateValue) {
    if (!dateValue) return '-';

    var date = new Date(dateValue);

    return date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
    });
}

function getErrorMessage(xhr, defaultMessage) {
    if (xhr.responseJSON) {
        return xhr.responseJSON.errorMessage ||
            xhr.responseJSON.message ||
            xhr.responseJSON.title ||
            defaultMessage;
    }

    if (xhr.responseText) {
        return xhr.responseText;
    }

    return defaultMessage;
}

function encodeHtml(value) {
    return $('<div/>').text(value || '').html();
}