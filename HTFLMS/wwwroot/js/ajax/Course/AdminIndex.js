$(document).ready(function () {

    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showSuccessPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }

    loadAdminCourses();

    $('#courseSearchInput').on('keyup', function () {
        var searchText = $(this).val().toLowerCase();

        $('.admin-courses-table-row').each(function () {
            var rowText = $(this).text().toLowerCase();
            $(this).toggle(rowText.includes(searchText));
        });
    });
});

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
        var title = course.title || 'Untitled Course';
        var category = course.category || 'N/A';
        var students = course.totalStudents || 0;
        var trainerName = course.trainerName || 'No Trainer';

        var imagePath = course.courseImagePath || '/img/course/course-1.webp';

        var status = '';
        var statusClass = '';

        if (course.isActive === false) {
            status = 'Inactive';
            statusClass = 'pill-red';
        } else if (course.isPublished === true) {
            status = 'Active';
            statusClass = 'pill-green';
        } else {
            status = 'Draft';
            statusClass = 'pill-yellow';
        }

        var createdDate = course.createdAt
            ? new Date(course.createdAt).toLocaleDateString('en-US', {
                month: 'short',
                day: '2-digit',
                year: 'numeric'
            })
            : 'N/A';

        var toggleButtonText = course.isActive ? 'Deactivate' : 'Activate';

        var row = `
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

                <div class="dashboard-table-cell-ellipsis" title="${students}">
                    ${students}
                </div>

                <div title="${status}">
                    <span class="pill ${statusClass}">${status}</span>
                </div>

                <div class="dashboard-table-cell-ellipsis" title="${createdDate}">
                    ${createdDate}
                </div>

                <div class="admin-courses-actions">
                    <a href="/Admin/Courses/Edit/${course.id}" class="dashboard-btn dashboard-btn-outline admin-course-action-btn">
                        Edit
                    </a>

                    <button class="dashboard-btn admin-delete-soft-btn admin-course-action-btn"
                            onclick="openToggleCourseModal(${course.id}, '${toggleButtonText}')">
                        ${toggleButtonText}
                    </button>
                </div>
            </div>
        `;

        body.append(row);
    });
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

let selectedCourseId = null;

function openToggleCourseModal(courseId, actionText) {
    selectedCourseId = courseId;

    $('#toggleCourseModalTitle').text(actionText + ' Course');
    $('#toggleCourseModalText').text('Are you sure you want to ' + actionText.toLowerCase() + ' this course?');
    $('#confirmToggleCourseBtn').text(actionText);

    $('#toggleCourseConfirmModal').addClass('show');
}

$('#cancelToggleCourseBtn').on('click', function () {
    $('#toggleCourseConfirmModal').removeClass('show');
    selectedCourseId = null;
});

$('#confirmToggleCourseBtn').on('click', function () {

    if (!selectedCourseId) return;

    $.ajax({
        url: '/api/Course/admin/toggle-active/' + selectedCourseId,
        type: 'PUT',

        success: function (response) {
            $('#toggleCourseConfirmModal').removeClass('show');

            sessionStorage.setItem("successMessage", response.message || "Course status updated successfully!");
            location.reload();
        },

        error: function (xhr) {
            $('#toggleCourseConfirmModal').removeClass('show');

            var err = xhr.responseJSON;
            var msg = err?.errorMessage || err?.message || err?.title || 'Course status update failed.';
            alert(msg);
        }
    });
});