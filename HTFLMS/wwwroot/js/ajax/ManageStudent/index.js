let deleteStudentId = null;
let allStudents = [];

$(document).ready(function () {
    showStoredSuccessMessage();
    loadStudents();
    loadCourseFilter();
    bindFilters();
    bindDeleteModalActions();
});

function isTrainerArea() {
    return window.location.pathname.toLowerCase().includes('/trainer/');
}

function getAreaBaseUrl() {
    return isTrainerArea() ? '/Trainer/Students' : '/Admin/Students';
}

function showStoredSuccessMessage() {
    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showSuccessPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }
}

function loadStudents() {
    $.ajax({
        url: '/api/manage-student',
        type: 'GET',

        success: function (students) {
            allStudents = students || [];
            applyFilters();
            updateStudentStats(allStudents);
        },

        error: function () {
            $('#studentsTableBody').html(`
                <div class="dashboard-table-row ${getTableRowClass()} admin-students-data-row">
                    <div class="text-danger">Failed to load students.</div>
                </div>
            `);
        }
    });
}

function loadCourseFilter() {
    var url = isTrainerArea() ? '/api/Course' : '/api/Course/admin/all';

    $.ajax({
        url: url,
        type: 'GET',

        success: function (courses) {
            var filter = $('#courseFilter');
            filter.html('<option value="">All courses</option>');

            $.each(courses || [], function (index, course) {
                if (course.isActive === true && course.isPublished === true) {
                    filter.append(`<option value="${course.title}">${course.title}</option>`);
                }
            });
        }
    });
}

function bindFilters() {
    $('#studentSearchInput, #courseFilter, #statusFilter').on('keyup change', function () {
        applyFilters();
    });
}

function applyFilters() {
    var searchText = ($('#studentSearchInput').val() || '').toLowerCase();
    var selectedCourse = $('#courseFilter').val();
    var selectedStatus = $('#statusFilter').val();

    var filteredStudents = allStudents.filter(function (student) {
        var rowText = (
            (student.name || '') + ' ' +
            (student.email || '') + ' ' +
            ((student.enrolledCourses || []).join(' '))
        ).toLowerCase();

        var matchesSearch = rowText.includes(searchText);

        var matchesStatus = !selectedStatus ||
            (student.status || '') === selectedStatus;

        var matchesCourse = !selectedCourse ||
            ((student.enrolledCourses || []).includes(selectedCourse));

        return matchesSearch && matchesStatus && matchesCourse;
    });

    renderStudents(filteredStudents);
}

function renderStudents(students) {
    var body = $('#studentsTableBody');
    body.empty();

    if (!students || students.length === 0) {
        body.html(`
            <div class="dashboard-table-row ${getTableRowClass()} admin-students-data-row">
                <div>No students found.</div>
            </div>
        `);

        $('#studentCountText').text('0 student(s)');
        return;
    }

    $('#studentCountText').text(students.length + ' student(s)');

    $.each(students, function (index, student) {
        body.append(buildStudentRow(student));
    });
}

function buildStudentRow(student) {
    var name = student.name || 'Unnamed Student';
    var email = student.email || 'N/A';
    var status = student.status || 'Inactive';
    var statusClass = status === 'Active' ? 'status-active' : 'status-inactive';
    var joined = formatDisplayDate(student.createdAt);
    var coursesCount = student.enrolledCoursesCount || 0;
    var coursesTitle = (student.enrolledCourses || []).join(', ');
    var avgGrade = student.averageGrade || 0;
    var initial = name.charAt(0).toUpperCase();

    var rowClass = getTableRowClass();
    var studentMainClass = isTrainerArea() ? 'trainer-student-main' : 'admin-student-main';
    var avatarClass = isTrainerArea() ? 'trainer-student-avatar trainer-avatar-blue' : 'admin-student-avatar admin-avatar-blue';
    var nameWrapClass = isTrainerArea() ? 'trainer-student-name-wrap' : 'admin-student-name-wrap';
    var nameClass = isTrainerArea() ? 'trainer-student-name' : 'admin-student-name';
    var actionsClass = isTrainerArea() ? 'trainer-courses-actions' : 'admin-courses-actions';
    var actionBtnClass = isTrainerArea() ? 'trainer-course-action-btn' : 'admin-course-action-btn';
    var deleteBtnClass = isTrainerArea() ? 'trainer-delete-soft-btn' : 'admin-delete-soft-btn';

    return `
        <div class="dashboard-table-row ${rowClass} admin-students-data-row">
            <div class="${studentMainClass}" title="${name}">
                <div class="${avatarClass}">${initial}</div>
                <div class="${nameWrapClass}">
                    <div class="${nameClass}">${name}</div>
                </div>
            </div>

            <div class="dashboard-table-cell-ellipsis" title="${email}">
                ${email}
            </div>

            <div title="${coursesTitle || 'No courses'}">
                ${coursesCount} course(s)
            </div>

            <div class="${getGradeClass(avgGrade)}" title="${avgGrade}%">
                ${avgGrade}%
            </div>

            <div title="${joined}">
                ${joined}
            </div>

            <div title="${status}">
                <span class="dashboard-status ${statusClass}">${status}</span>
            </div>

            <div class="${actionsClass}">
                <a href="${getAreaBaseUrl()}/Edit/${student.id}"
                   class="dashboard-btn dashboard-btn-outline ${actionBtnClass}">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn ${deleteBtnClass} ${actionBtnClass}"
                        onclick="deleteStudent(${student.id})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function updateStudentStats(students) {
    var total = students.length;
    var active = students.filter(x => x.status === 'Active').length;
    var inactive = total - active;

    var enrollments = students.reduce(function (sum, student) {
        return sum + (student.enrolledCoursesCount || 0);
    }, 0);

    $('#totalStudentsCount').text(total);
    $('#activeStudentsCount').text(active);
    $('#inactiveStudentsCount').text(inactive);
    $('#totalEnrollmentsCount').text(enrollments);
}

function deleteStudent(studentId) {
    deleteStudentId = studentId;
    $('#deleteStudentModal').addClass('show');
}

function bindDeleteModalActions() {
    $('#cancelDeleteStudentBtn').on('click', closeDeleteModal);

    $('#confirmDeleteStudentBtn').on('click', function () {
        if (!deleteStudentId) return;

        $.ajax({
            url: '/api/manage-student/delete/' + deleteStudentId,
            type: 'DELETE',

            success: function (response) {
                closeDeleteModal();

                if (response && response.message) {
                    sessionStorage.setItem("successMessage", response.message);
                }

                location.reload();
            },

            error: function (xhr) {
                closeDeleteModal();
                alert(getErrorMessage(xhr));
            }
        });
    });
}

function closeDeleteModal() {
    $('#deleteStudentModal').removeClass('show');
    deleteStudentId = null;
}

function getTableRowClass() {
    return isTrainerArea() ? 'trainer-students-table-row' : 'admin-students-table-row';
}

function formatDisplayDate(dateValue) {
    if (!dateValue) return 'N/A';

    return new Date(dateValue).toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric'
    });
}

function getGradeClass(avgGrade) {
    if (isTrainerArea()) {
        if (avgGrade >= 80) return 'trainer-grade-good';
        if (avgGrade >= 60) return 'trainer-grade-warn';
        return 'trainer-grade-danger';
    }

    if (avgGrade >= 80) return 'admin-grade-good';
    if (avgGrade >= 60) return 'admin-grade-warn';
    return 'admin-grade-danger';
}

function getErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}