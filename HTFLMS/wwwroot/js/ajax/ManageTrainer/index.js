$(document).ready(function () {
    createDeleteConfirmModal();
    loadTrainers();

    var successMessage = sessionStorage.getItem("successMessage");

    if (successMessage) {
        showPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }

    $('#trainerSearchInput').on('input', function () {
        var search = $(this).val().toLowerCase();

        $('.manage-trainer-table-row').each(function () {
            var text = $(this).text().toLowerCase();
            $(this).toggle(text.includes(search));
        });
    });
});

let trainerToDelete = null;

function showPopup(message) {
    if (typeof showSuccessPopup === 'function') {
        showSuccessPopup(message);
        return;
    }

    alert(message);
}

function getValue(obj, camelName, pascalName) {
    return obj[camelName] ?? obj[pascalName];
}

function getMessage(xhr) {
    var err = xhr.responseJSON;

    if (err?.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err?.message || err?.errorMessage || err?.title || xhr.responseText || 'Something went wrong.';
}

function loadTrainers() {
    $.ajax({
        url: '/api/ManageTrainer/admin/all',
        type: 'GET',

        success: function (trainers) {
            renderStats(trainers);
            renderTrainers(trainers);
        },

        error: function (xhr) {
            $('#trainersContainer').html(`
                <div class="dashboard-empty-state">
                    ${getMessage(xhr)}
                </div>
            `);
        }
    });
}

function renderStats(trainers) {
    trainers = trainers || [];

    var total = trainers.length;
    var active = trainers.filter(t => getValue(t, 'isActive', 'IsActive') === true).length;
    var inactive = total - active;

    var assignedCourses = trainers.reduce(function (sum, trainer) {
        return sum + (getValue(trainer, 'assignedCourseCount', 'AssignedCourseCount') || 0);
    }, 0);

    $('#totalTrainersCount').text(total);
    $('#activeTrainersCount').text(active);
    $('#inactiveTrainersCount').text(inactive);
    $('#assignedCoursesCount').text(assignedCourses);
    $('#trainerTableCount').text(total + ' trainer(s)');
}

function renderTrainers(trainers) {
    var container = $('#trainersContainer');
    container.html('');

    if (!trainers || trainers.length === 0) {
        container.html(`
            <div class="dashboard-empty">
                <div>
                    <div class="dashboard-empty-icon">
                        <i class="bi bi-people"></i>
                    </div>
                    <div class="dashboard-empty-title">No trainers found</div>
                    <div class="dashboard-empty-sub">Add your first trainer to get started.</div>
                </div>
            </div>
        `);
        return;
    }

    $.each(trainers, function (index, trainer) {
        container.append(buildTrainerRow(trainer));
    });
}

function buildTrainerRow(trainer) {
    var id = getValue(trainer, 'id', 'Id');
    var name = getValue(trainer, 'name', 'Name') || '';
    var email = getValue(trainer, 'email', 'Email') || '';
    var designation = getValue(trainer, 'designation', 'Designation') || '';
    var isActive = getValue(trainer, 'isActive', 'IsActive') === true;
    var createdAt = getValue(trainer, 'createdAt', 'CreatedAt');
    var assignedCourseCount = getValue(trainer, 'assignedCourseCount', 'AssignedCourseCount') || 0;

    var courseText = assignedCourseCount === 0
        ? 'No Course Assigned'
        : assignedCourseCount === 1
            ? '1 Course'
            : assignedCourseCount + ' Courses';

    var dateText = createdAt
        ? new Date(createdAt).toLocaleDateString('en-US', {
            month: 'short',
            day: '2-digit',
            year: 'numeric'
        })
        : '';

    var statusBadge = isActive
        ? '<span class="pill pill-green">Active</span>'
        : '<span class="pill pill-yellow">Inactive</span>';

    return `
        <div class="dashboard-table-row courses-table-row trainer-courses-table-row manage-trainer-table-row">
            <div class="manage-trainer-name-text manage-trainer-cell-ellipsis" title="${name}">
                ${name}
            </div>

            <div class="manage-trainer-cell-ellipsis" title="${email}">
                ${email}
            </div>

            <div class="manage-trainer-cell-ellipsis" title="${designation}">
                ${designation}
            </div>

            <div class="manage-trainer-cell-ellipsis" title="${courseText}">
                <span class="manage-trainer-assigned-text">${courseText}</span>
            </div>

            <div title="${isActive ? 'Active' : 'Inactive'}">
                ${statusBadge}
            </div>

            <div class="manage-trainer-cell-ellipsis" title="${dateText}">
                ${dateText}
            </div>

            <div class="trainer-courses-actions manage-trainer-actions">
                <a href="/Admin/ManageTrainers/EditTrainer/${id}"
                   class="dashboard-btn dashboard-btn-outline trainer-course-action-btn">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn trainer-delete-soft-btn trainer-course-action-btn"
                        onclick="deleteTrainer(${id})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function deleteTrainer(id) {
    trainerToDelete = id;

    $('#deleteConfirmModal h3').text('Delete Trainer');
    $('#deleteConfirmModal p').text('Are you sure you want to delete this trainer?');
    $('#deleteConfirmModal').addClass('show');
}

$(document).on('click', '#cancelDeleteBtn', function (e) {
    e.preventDefault();
    closeDeleteModal();
});

$(document).on('click', '#confirmDeleteBtn', function (e) {
    e.preventDefault();

    if (!trainerToDelete) {
        return;
    }

    $.ajax({
        url: '/api/ManageTrainer/' + trainerToDelete,
        type: 'DELETE',

        success: function (response) {
            closeDeleteModal();

            if (response?.message) {
                showPopup(response.message);
            }

            loadTrainers();
        },

        error: function (xhr) {
            showPopup(getMessage(xhr));
        }
    });
});

function closeDeleteModal() {
    $('#deleteConfirmModal').removeClass('show');
    trainerToDelete = null;
}

function createDeleteConfirmModal() {
    if ($('#deleteConfirmModal').length > 0) {
        return;
    }

    $('body').append(`
        <div id="deleteConfirmModal" class="custom-modal">
            <div class="custom-modal-backdrop"></div>

            <div class="custom-modal-box">
                <h3>Delete Trainer</h3>
                <p>Are you sure you want to delete this trainer?</p>

                <div class="custom-modal-actions">
                    <button type="button"
                            id="cancelDeleteBtn"
                            class="dashboard-btn dashboard-btn-outline">
                        Cancel
                    </button>

                    <button type="button"
                            id="confirmDeleteBtn"
                            class="dashboard-btn add-course-btn">
                        Delete
                    </button>
                </div>
            </div>
        </div>
    `);
}