let selectedCourseId = null;

$(document).ready(function () {
    showStoredSuccessMessage();
    loadAdminCourses();
    bindAdminCourseSearch();
    bindToggleCourseModalActions();
});

function showStoredSuccessMessage() {
    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showSuccessPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }
}

function loadAdminCourses() {
    $.ajax({
        url: '/api/Course/admin/all',
        type: 'GET',

        success: function (courses) {
            renderAdminCourses(courses);
            updateAdminCourseStats(courses);
        },

        error: function () {
            $('#coursesTableBody').html(`
                <div class="dashboard-table-row courses-table-row admin-courses-table-row">
                    <div class="text-danger">Failed to load courses.</div>
                </div>
            `);
        }
    });
}

function renderAdminCourses(courses) {
    var body = $('#coursesTableBody');
    body.empty();

    if (!courses || courses.length === 0) {
        body.html(`
            <div class="dashboard-table-row courses-table-row admin-courses-table-row">
                <div>No courses found.</div>
            </div>
        `);

        $('#courseCountText').text('0 course(s)');
        return;
    }

    $('#courseCountText').text(courses.length + ' course(s)');

    $.each(courses, function (index, course) {
        body.append(buildAdminCourseRow(course));
    });
}

function buildAdminCourseRow(course) {
    var title = course.title || 'Untitled Course';
    var category = course.category || 'N/A';
    var students = course.totalStudents || 0;
    var trainerName = course.trainerName || 'No Trainer';
    var imagePath = course.courseImagePath || '/img/course/course-1.webp';
    var createdDate = formatDisplayDate(course.createdAt);

    var status = getCourseStatus(course);
    var statusClass = getCourseStatusClass(course);
    var toggleText = course.isActive ? 'Deactivate' : 'Activate';

    return `
        <div class="dashboard-table-row courses-table-row admin-courses-table-row">
            <div class="courses-coursecell dashboard-table-cell-ellipsis" title="${title}">
                <div class="courses-title-wrap">
                    <div class="course-thumb">
                        <img src="${imagePath}" alt="${title}" />
                    </div>

                    <div class="courses-title-block">
                        <div class="courses-title" title="${title}">${title}</div>
                        <div class="courses-sub-text dashboard-table-cell-ellipsis" title="${trainerName}">
                            Assigned to: ${trainerName}
                        </div>
                    </div>
                </div>
            </div>

            <div class="dashboard-table-cell-ellipsis" title="${category}">
                <span class="admin-course-chip">${category}</span>
            </div>

            <div>${students}</div>

            <div>
                <span class="pill ${statusClass}">${status}</span>
            </div>

            <div>${createdDate}</div>

            <div class="admin-courses-actions">
                <a href="/Admin/Courses/Edit/${course.id}" class="dashboard-btn dashboard-btn-outline admin-course-action-btn">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn admin-delete-soft-btn admin-course-action-btn"
                        onclick="openToggleCourseModal(${course.id}, '${toggleText}')">
                    ${toggleText}
                </button>
            </div>
        </div>
    `;
}

function getCourseStatus(course) {
    if (course.isActive === false) return 'Inactive';
    return course.isPublished ? 'Active' : 'Draft';
}

function getCourseStatusClass(course) {
    if (course.isActive === false) return 'pill-red';
    return course.isPublished ? 'pill-green' : 'pill-yellow';
}

function updateAdminCourseStats(courses) {
    var total = courses.length;
    var active = courses.filter(c => c.isActive === true && c.isPublished === true).length;
    var draft = courses.filter(c => c.isActive === true && c.isPublished === false).length;

    var students = courses.reduce(function (sum, course) {
        return sum + (course.totalStudents || 0);
    }, 0);

    $('#totalCoursesCount').text(total);
    $('#activeCoursesCount').text(active);
    $('#draftCoursesCount').text(draft);
    $('#totalStudentsCount').text(students);
}

function bindAdminCourseSearch() {
    $('#courseSearchInput').on('keyup', function () {
        var searchText = $(this).val().toLowerCase();

        $('.admin-courses-table-row').each(function () {
            var rowText = $(this).text().toLowerCase();
            $(this).toggle(rowText.includes(searchText));
        });
    });
}

function openToggleCourseModal(courseId, actionText) {
    selectedCourseId = courseId;

    $('#toggleCourseModalTitle').text(actionText + ' Course');
    $('#toggleCourseModalText').text('Are you sure you want to ' + actionText.toLowerCase() + ' this course?');
    $('#confirmToggleCourseBtn').text(actionText);

    $('#toggleCourseConfirmModal').addClass('show');
}

function bindToggleCourseModalActions() {
    $('#cancelToggleCourseBtn').on('click', closeToggleModal);

    $('#confirmToggleCourseBtn').on('click', function () {
        if (!selectedCourseId) return;

        $.ajax({
            url: '/api/Course/admin/toggle-active/' + selectedCourseId,
            type: 'PUT',

            success: function (response) {
                closeToggleModal();
                sessionStorage.setItem("successMessage", response.message || "Course status updated successfully.");
                location.reload();
            },

            error: function (xhr) {
                closeToggleModal();
                alert(getErrorMessage(xhr));
            }
        });
    });
}

function closeToggleModal() {
    $('#toggleCourseConfirmModal').removeClass('show');
    selectedCourseId = null;
}

function formatDisplayDate(dateValue) {
    if (!dateValue) return 'N/A';

    return new Date(dateValue).toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric'
    });
}

function getErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}