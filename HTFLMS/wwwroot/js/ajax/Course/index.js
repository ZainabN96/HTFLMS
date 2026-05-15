let deleteCourseId = null;

$(document).ready(function () {

    showStoredSuccessMessage();
    loadCourses();
    bindCourseSearch();
    bindDeleteModalActions();
});

function showStoredSuccessMessage() {
    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showSuccessPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }
}

function bindCourseSearch() {
    $('#courseSearchInput').on('keyup', function () {
        var searchText = $(this).val().toLowerCase();

        $('.trainer-courses-table-row').each(function () {
            var rowText = $(this).text().toLowerCase();
            $(this).toggle(rowText.includes(searchText));
        });
    });
}

function loadCourses() {
    $.ajax({
        url: '/api/Course',
        type: 'GET',

        success: function (courses) {
            renderCourses(courses);
            updateCourseStats(courses);
        },

        error: function () {
            $('#coursesTableBody').html(`
                <div class="dashboard-table-row courses-table-row trainer-courses-table-row">
                    <div class="text-danger">Failed to load courses.</div>
                </div>
            `);
        }
    });
}

function renderCourses(courses) {
    var body = $('#coursesTableBody');
    body.empty();

    if (!courses || courses.length === 0) {
        body.html(`
            <div class="dashboard-table-row courses-table-row trainer-courses-table-row">
                <div>No courses found.</div>
            </div>
        `);

        $('#courseCountText').text('0 course(s)');
        return;
    }

    $('#courseCountText').text(courses.length + ' course(s)');

    $.each(courses, function (index, course) {
        body.append(buildCourseRow(course));
    });
}

function buildCourseRow(course) {
    var title = course.title || 'Untitled Course';
    var category = course.category || 'N/A';
    var description = course.description || '';
    var students = course.totalStudents || 0;
    var imagePath = course.courseImagePath || '/img/course/course-1.webp';
    var status = course.isPublished ? 'Active' : 'Draft';
    var statusClass = course.isPublished ? 'pill-green' : 'pill-yellow';
    var createdDate = formatDisplayDate(course.createdAt);

    return `
        <div class="dashboard-table-row courses-table-row trainer-courses-table-row">
            <div class="courses-coursecell dashboard-table-cell-ellipsis" title="${title}">
                <div class="courses-title-wrap">
                    <div class="course-thumb">
                        <img src="${imagePath}" alt="${title}" />
                    </div>

                    <div class="courses-title-block">
                        <div class="courses-title" title="${title}">${title}</div>

                        <div class="courses-sub-text dashboard-table-cell-ellipsis" title="${description}">
                            ${description}
                        </div>
                    </div>
                </div>
            </div>

            <div class="dashboard-table-cell-ellipsis" title="${category}">
                <span class="trainer-course-chip">${category}</span>
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

            <div class="trainer-courses-actions">
                <a href="/Trainer/Courses/Edit/${course.id}" class="dashboard-btn dashboard-btn-outline">
                    Edit
                </a>

                <button class="dashboard-btn trainer-delete-soft-btn trainer-course-action-btn"
                        onclick="deleteCourse(${course.id})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function formatDisplayDate(dateValue) {
    if (!dateValue) return 'N/A';

    return new Date(dateValue).toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric'
    });
}

function updateCourseStats(courses) {
    var total = courses.length;
    var active = courses.filter(c => c.isPublished === true).length;
    var draft = total - active;

    var students = courses.reduce(function (sum, course) {
        return sum + (course.totalStudents || 0);
    }, 0);

    $('#totalCoursesCount').text(total);
    $('#activeCoursesCount').text(active);
    $('#draftCoursesCount').text(draft);
    $('#totalStudentsCount').text(students);
}

function deleteCourse(courseId) {
    deleteCourseId = courseId;
    $('#deleteConfirmModal').addClass('show');
}

function bindDeleteModalActions() {
    $('#cancelDeleteBtn').on('click', closeDeleteModal);

    $('#confirmDeleteBtn').on('click', function () {
        if (!deleteCourseId) return;

        $.ajax({
            url: '/api/Course/delete/' + deleteCourseId,
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

                var msg = getErrorMessage(xhr);
                alert(msg);
            }
        });
    });
}

function closeDeleteModal() {
    $('#deleteConfirmModal').removeClass('show');
    deleteCourseId = null;
}

function getErrorMessage(xhr) {
    var err = xhr.responseJSON;

    if (!err) {
        return 'Something went wrong. Please try again.';
    }

    return err.errorMessage || err.message || err.title || 'Something went wrong. Please try again.';
}