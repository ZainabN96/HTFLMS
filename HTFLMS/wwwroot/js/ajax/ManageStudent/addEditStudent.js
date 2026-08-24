let selectedCourseIdsFromEdit = [];

$(document).ready(function () {
    var studentId = $('#studentId').val();
    var isEdit = studentId && studentId !== '0' && studentId !== '';

    if (isEdit) {
        setupEditMode();
        loadStudent(studentId);
    }

    loadCourses();

    $('#studentForm').on('submit', function (e) {
        e.preventDefault();
        saveStudent(studentId, isEdit);
    });

    $('#titlePrefix, #name, #email, #password, #status, #joinDate').on('input change', function () {
        clearError();
    });

    $(document).on('change', '.student-course-checkbox', function () {
        clearError();
    });
});

function setupEditMode() {
    $('#pageTitle').text('Edit Student');
    $('#pageSub').text('Update student details and enrollments.');
    $('#saveStudentBtn').text('Update Student');
    $('#password').attr('placeholder', 'Leave empty to keep old password');
}

function loadStudent(studentId) {
    $.ajax({
        url: '/api/manage-student/' + studentId,
        type: 'GET',

        success: function (student) {
            populateStudentForm(student);
        },

        error: function (xhr) {
            showError(getErrorMessage(xhr));
        }
    });
}

function populateStudentForm(student) {
    $('#titlePrefix').val(student.titlePrefix || '');
    $('#name').val(student.name || '');
    $('#email').val(student.email || '');
    $('#status').val(student.status || 'Active');
    $('#joinDate').val(formatDate(student.joinDate));

    selectedCourseIdsFromEdit = student.courseIds || [];

    markSelectedCourses();
}

function loadCourses() {
    var courseUrl = getCourseListUrl();

    $.ajax({
        url: courseUrl,
        type: 'GET',

        success: function (courses) {
            renderCourseCheckboxes(courses);
            markSelectedCourses();
        },

        error: function () {
            $('#courseSelectList').html(`
                <div class="text-danger">Failed to load courses.</div>
            `);
        }
    });
}

function getCourseListUrl() {
    var path = window.location.pathname.toLowerCase();

    if (path.includes('/admin/')) {
        return '/api/Course/admin/all';
    }

    return '/api/Course';
}

function renderCourseCheckboxes(courses) {
    var list = $('#courseSelectList');
    list.empty();

    if (!courses || courses.length === 0) {
        list.html('<div class="dashboard-muted-small">No courses available.</div>');
        return;
    }

    $.each(courses, function (index, course) {
        var isAvailable = course.isActive === true && course.isPublished === true;

        if (!isAvailable) {
            return;
        }

        list.append(`
            <label class="course-select-item">
                <input type="checkbox" class="student-course-checkbox" value="${course.id}" />
                <span>${escapeHtml(course.title)}</span>
            </label>
        `);
    });

    if ($('.student-course-checkbox').length === 0) {
        list.html('<div class="dashboard-muted-small">No active published courses available.</div>');
    }
}

function markSelectedCourses() {
    if (!selectedCourseIdsFromEdit || selectedCourseIdsFromEdit.length === 0) {
        return;
    }

    $('.student-course-checkbox').each(function () {
        var courseId = parseInt($(this).val());

        if (selectedCourseIdsFromEdit.includes(courseId)) {
            $(this).prop('checked', true);
        }
    });
}

function getAreaBaseUrl() {
    return window.location.pathname.toLowerCase().includes('/trainer/')
        ? '/Trainer/Students'
        : '/Admin/Students';
}

function saveStudent(studentId, isEdit) {
    var btn = $('#saveStudentBtn');
    var payload = buildStudentPayload();

    if (!validateStudentPayload(payload, isEdit)) {
        return;
    }

    setButtonLoading(btn, isEdit);

    $.ajax({
        url: isEdit ? '/api/manage-student/edit/' + studentId : '/api/manage-student/save',
        type: isEdit ? 'PUT' : 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),

        success: function (response) {
            if (response && response.message) {
                sessionStorage.setItem("successMessage", response.message);
            }

            window.location.href = getAreaBaseUrl() + '/Index';
        },

        error: function (xhr) {
            showError(getErrorMessage(xhr));
        },

        complete: function () {
            resetButton(btn, isEdit);
        }
    });
}

function buildStudentPayload() {
    return {
        titlePrefix: $('#titlePrefix').val(),
        name: $('#name').val(),
        email: $('#email').val(),
        password: $('#password').val(),
        status: $('#status').val(),
        joinDate: $('#joinDate').val() || null,
        courseIds: getSelectedCourseIds()
    };
}

function validateStudentPayload(payload, isEdit) {
    clearError();

    if (!payload.titlePrefix || (payload.titlePrefix !== 'Mr.' && payload.titlePrefix !== 'Ms.')) {
        showError('Please select title prefix.');
        $('#titlePrefix').focus();
        return false;
    }

    if (!payload.name || !payload.name.trim()) {
        showError('Please enter student name.');
        $('#name').focus();
        return false;
    }

    if (!payload.email || !payload.email.trim()) {
        showError('Please enter email address.');
        $('#email').focus();
        return false;
    }

    if (!isEdit && (!payload.password || !payload.password.trim())) {
        showError('Password is required for new student.');
        $('#password').focus();
        return false;
    }

    if (!payload.status || !payload.status.trim()) {
        showError('Please select status.');
        $('#status').focus();
        return false;
    }

    if (!payload.courseIds || payload.courseIds.length === 0) {
        showError('Please select at least one course.');
        return false;
    }

    return true;
}

function getSelectedCourseIds() {
    var ids = [];

    $('.student-course-checkbox:checked').each(function () {
        ids.push(parseInt($(this).val()));
    });

    return ids;
}

function setButtonLoading(btn, isEdit) {
    btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');
}

function resetButton(btn, isEdit) {
    btn.prop('disabled', false).text(isEdit ? 'Update Student' : 'Save Student');
}

function formatDate(dateValue) {
    if (!dateValue) return '';

    if (typeof dateValue === 'string') {
        return dateValue.substring(0, 10);
    }

    var year = dateValue.getFullYear();
    var month = String(dateValue.getMonth() + 1).padStart(2, '0');
    var day = String(dateValue.getDate()).padStart(2, '0');

    return year + '-' + month + '-' + day;
}

function getErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    if (err.errors) {
        var messages = [];

        Object.keys(err.errors).forEach(function (key) {
            var fieldErrors = err.errors[key];

            if (Array.isArray(fieldErrors)) {
                fieldErrors.forEach(function (item) {
                    messages.push(item);
                });
            }
        });

        if (messages.length > 0) {
            return messages.join(' ');
        }
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}

function showError(message) {
    $('.error-box')
        .empty()
        .append($('<div>').text(message || 'Something went wrong. Please try again.'));
}

function clearError() {
    $('.error-box').empty();
}

function escapeHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}