$(document).ready(function () {
    var successMessage = sessionStorage.getItem("successMessage");
    var restoreAfterCancel = sessionStorage.getItem("restoreSelectionAfterCancel") === "true";
    var shouldRestoreCourse = !!successMessage || restoreAfterCancel;

    if (successMessage) {
        showPopup(successMessage);
        sessionStorage.removeItem("successMessage");
    }

    sessionStorage.removeItem("restoreSelectionAfterCancel");

    createDeleteConfirmModal();
    loadAdminCourses(shouldRestoreCourse);

    $(document).on('click', '#addModuleBtn, #addLessonBtn, #addQuizBtn', function (e) {
        var courseId = $('#courseSelect').val();
        var moduleId = $('#moduleSelect').val();

        if (!isValidId(courseId)) {
            e.preventDefault();
            showPopup('Please select a course first.');
            return;
        }

        storeSelectedCourse(courseId);
        storeSelectedModule(moduleId);
    });

    $('#courseSelect').on('change', function () {
        var courseId = $(this).val();

        $('#selectedCourseId').val(courseId);
        storeSelectedCourse(courseId);
        sessionStorage.removeItem("selectedModuleId");

        updateCreateLinks(courseId);

        if (isValidId(courseId)) {
            loadModules(courseId);
            return;
        }

        resetStats();
        resetCourseContent('Please select a course.');
        $('#moduleSelect').html(`<option value="0">Select a Course First to view modules list</option>`);
    });

    $('#moduleSelect').on('change', function () {
        var moduleId = $(this).val();
        var courseId = $('#courseSelect').val();

        storeSelectedModule(moduleId);
        updateModuleBasedLinks(courseId, moduleId);
        filterModuleContent(moduleId);
    });

    $(document).on('change', '.module-lock-toggle', function () {
        updateAccessToggle({
            checkbox: $(this),
            url: '/api/Module/toggle-access/' + $(this).data('id')
        });
    });

    $(document).on('change', '.quiz-lock-toggle', function () {
        updateAccessToggle({
            checkbox: $(this),
            url: '/api/Quiz/toggle-access/' + $(this).data('id')
        });
    });
});

let deleteItem = {
    id: null,
    type: '',
    container: ''
};

let stats = {
    modules: 0,
    lessons: 0,
    quizzes: 0,
    published: false
};

function showPopup(message) {
    if (typeof showSuccessPopup === 'function') {
        showSuccessPopup(message);
        return;
    }

    alert(message);
}

function isValidId(id) {
    return id && id !== '0' && id !== '';
}

function getValue(obj, camelName, pascalName) {
    return obj[camelName] ?? obj[pascalName];
}

function getMessage(xhr) {
    var err = xhr.responseJSON;

    if (err?.errors) {
        return Object.values(err.errors).flat().join('<br>');
    }

    return err?.errorMessage || err?.innerError || err?.title || err?.message || xhr.responseText || 'Something went wrong.';
}

function setEmptyState(selector, message) {
    $(selector).html(`<div class="dashboard-empty-state">${message}</div>`);
}

function resetCourseContent(message) {
    setEmptyState('#modulesContainer', message);
    setEmptyState('#lessonsContainer', message);
    setEmptyState('#quizzesContainer', message);
}

function storeSelectedCourse(courseId) {
    if (isValidId(courseId)) {
        sessionStorage.setItem("selectedCourseId", courseId);
    } else {
        sessionStorage.removeItem("selectedCourseId");
    }
}

function storeSelectedModule(moduleId) {
    if (isValidId(moduleId)) {
        sessionStorage.setItem("selectedModuleId", moduleId);
    } else {
        sessionStorage.removeItem("selectedModuleId");
    }
}

function updateCreateLinks(courseId) {
    if (!isValidId(courseId)) {
        $('#addModuleBtn').attr('href', '#');
        $('#addLessonBtn').attr('href', '#');
        $('#addQuizBtn').attr('href', '#');
        return;
    }

    $('#addModuleBtn').attr('href', '/Admin/ModulesLessons/CreateModule?courseId=' + courseId);
    $('#addLessonBtn').attr('href', '/Admin/ModulesLessons/CreateLesson?courseId=' + courseId);
    $('#addQuizBtn').attr('href', '/Admin/ModulesLessons/CreateQuiz?courseId=' + courseId);
}

function updateModuleBasedLinks(courseId, moduleId) {
    if (!isValidId(courseId)) {
        updateCreateLinks(courseId);
        return;
    }

    $('#addModuleBtn').attr('href', '/Admin/ModulesLessons/CreateModule?courseId=' + courseId);

    if (isValidId(moduleId)) {
        $('#addLessonBtn').attr('href', '/Admin/ModulesLessons/CreateLesson?courseId=' + courseId + '&moduleId=' + moduleId);
        $('#addQuizBtn').attr('href', '/Admin/ModulesLessons/CreateQuiz?courseId=' + courseId + '&moduleId=' + moduleId);
        return;
    }

    $('#addLessonBtn').attr('href', '/Admin/ModulesLessons/CreateLesson?courseId=' + courseId);
    $('#addQuizBtn').attr('href', '/Admin/ModulesLessons/CreateQuiz?courseId=' + courseId);
}

function filterModuleContent(moduleId) {
    $('.module-row').removeClass('selected-module-row');

    if (isValidId(moduleId)) {
        highlightSelectedModule(moduleId);

        $('.admin-lessons-table-row[data-lesson-module-id]').hide();
        $('.admin-lessons-table-row[data-lesson-module-id="' + moduleId + '"]').show();

        $('.admin-quiz-table-row[data-quiz-module-id]').hide();
        $('.admin-quiz-table-row[data-quiz-module-id="' + moduleId + '"]').show();

        return;
    }

    $('.admin-lessons-table-row[data-lesson-module-id]').show();
    $('.admin-quiz-table-row[data-quiz-module-id]').show();
}

function updateAccessToggle(config) {
    var checkbox = config.checkbox;
    var isAccessible = checkbox.is(':checked');

    $.ajax({
        url: config.url,
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({ isAccessible: isAccessible }),

        success: function (response) {
            storeSelectedCourse($('#courseSelect').val());
            storeSelectedModule($('#moduleSelect').val());

            if (response?.message) {
                sessionStorage.setItem("successMessage", response.message);
            }

            location.reload();
        },

        error: function (xhr) {
            console.log(xhr.responseText);
            checkbox.prop('checked', !isAccessible);
            showPopup(getMessage(xhr));
        }
    });
}

function resetStats() {
    stats.modules = 0;
    stats.lessons = 0;
    stats.quizzes = 0;
    stats.published = false;
    updateStatsCards();
}

function updateStatsCards() {
    $('#totalModulesCount').text(stats.modules);
    $('#totalLessonsCount').text(stats.lessons);
    $('#totalQuizzesCount').text(stats.quizzes);
    $('#publishedStatus').text(stats.published ? 'Yes' : 'No');
}

function loadAdminCourses(shouldRestoreCourse) {
    $.ajax({
        url: '/api/Course/admin/all',
        type: 'GET',

        success: function (courses) {
            resetStats();

            var courseSelect = $('#courseSelect');
            var selectedCourseId = $('#selectedCourseId').val();
            var savedCourseId = sessionStorage.getItem("selectedCourseId");

            courseSelect.html(`<option value="0">Select Course</option>`);
            $('#moduleSelect').html(`<option value="0">Select a Course First to view modules list</option>`);
            resetCourseContent('Please select a course.');

            if (!courses || courses.length === 0) {
                updateCreateLinks('0');
                setEmptyState('#modulesContainer', 'No courses found.');
                return;
            }

            $.each(courses, function (_, course) {
                courseSelect.append(`
                    <option value="${getValue(course, 'id', 'Id')}">
                        ${getValue(course, 'title', 'Title')}
                    </option>
                `);
            });

            var courseIdToSelect = getCourseIdToSelect(courses, selectedCourseId, savedCourseId, shouldRestoreCourse);

            $('#selectedCourseId').val(courseIdToSelect);
            courseSelect.val(courseIdToSelect);
            updateCreateLinks(courseIdToSelect);

            if (isValidId(courseIdToSelect)) {
                loadModules(courseIdToSelect);
            }
        },

        error: function (xhr) {
            console.log(xhr.responseText);

            resetStats();
            updateCreateLinks('0');
            $('#moduleSelect').html(`<option value="0">No modules found</option>`);

            setEmptyState('#modulesContainer', getMessage(xhr));
            setEmptyState('#lessonsContainer', getMessage(xhr));
            setEmptyState('#quizzesContainer', getMessage(xhr));
        }
    });
}

function getCourseIdToSelect(courses, selectedCourseId, savedCourseId, shouldRestoreCourse) {
    var courseIds = courses.map(function (course) {
        return getValue(course, 'id', 'Id').toString();
    });

    if (shouldRestoreCourse && isValidId(savedCourseId) && courseIds.includes(savedCourseId)) {
        return savedCourseId;
    }

    if (isValidId(selectedCourseId) && courseIds.includes(selectedCourseId)) {
        return selectedCourseId;
    }

    return '0';
}

function loadModules(courseId) {
    $.ajax({
        url: '/api/Module/course/' + courseId,
        type: 'GET',

        success: function (modules) {
            resetStats();

            var container = $('#modulesContainer');
            var moduleSelect = $('#moduleSelect');

            container.html('');
            moduleSelect.html(`<option value="0">Select Module</option>`);
            $('#lessonsContainer').html('');
            $('#quizzesContainer').html('');

            updateModuleBasedLinks(courseId, '0');

            if (!modules || modules.length === 0) {
                sessionStorage.removeItem("selectedModuleId");

                moduleSelect.html(`<option value="0">No modules found</option>`);
                setEmptyState('#modulesContainer', 'No modules found.');
                setEmptyState('#lessonsContainer', 'No lessons found.');
                setEmptyState('#quizzesContainer', 'No quizzes found.');
                return;
            }

            stats.modules = modules.length;
            stats.published = modules.some(function (module) {
                return getValue(module, 'isActive', 'IsActive') === true;
            });

            updateStatsCards();

            $.each(modules, function (_, module) {
                appendModuleOption(moduleSelect, module);
                container.append(buildModuleRow(module));

                var moduleId = getValue(module, 'id', 'Id');
                var moduleCourseId = getValue(module, 'courseId', 'CourseId');
                var moduleTitle = getValue(module, 'title', 'Title');

                loadLessonsForModule(moduleId, moduleCourseId, moduleTitle);
                loadQuizzesForModule(moduleId, moduleCourseId, moduleTitle);
            });

            restoreSelectedModule(courseId, moduleSelect);
        },

        error: function (xhr) {
            console.log(xhr.responseText);

            resetStats();
            $('#moduleSelect').html(`<option value="0">No modules found</option>`);

            setEmptyState('#modulesContainer', getMessage(xhr));
            setEmptyState('#lessonsContainer', getMessage(xhr));
            setEmptyState('#quizzesContainer', getMessage(xhr));
        }
    });
}

function restoreSelectedModule(courseId, moduleSelect) {
    var savedModuleId = sessionStorage.getItem("selectedModuleId");

    if (isValidId(savedModuleId) && moduleSelect.find('option[value="' + savedModuleId + '"]').length > 0) {
        moduleSelect.val(savedModuleId);
        updateModuleBasedLinks(courseId, savedModuleId);
        filterModuleContent(savedModuleId);
        return;
    }

    moduleSelect.val('0');
    updateModuleBasedLinks(courseId, '0');
    sessionStorage.removeItem("selectedModuleId");
}

function appendModuleOption(moduleSelect, module) {
    moduleSelect.append(`
        <option value="${getValue(module, 'id', 'Id')}">
            ${getValue(module, 'title', 'Title')}
        </option>
    `);
}

function getStatusBadge(isActive) {
    return isActive
        ? '<span class="pill pill-green">Published</span>'
        : '<span class="pill pill-yellow">Draft</span>';
}

function buildModuleRow(module) {
    var moduleId = getValue(module, 'id', 'Id');
    var moduleCourseId = getValue(module, 'courseId', 'CourseId');
    var moduleTitle = getValue(module, 'title', 'Title');
    var moduleDescription = getValue(module, 'description', 'Description') ?? '';
    var moduleDisplayOrder = getValue(module, 'displayOrder', 'DisplayOrder');
    var moduleIsActive = getValue(module, 'isActive', 'IsActive');
    var moduleIsAccessible = getValue(module, 'isAccessible', 'IsAccessible');
    var lessonsCount = getValue(module, 'lessonsCount', 'LessonsCount') ?? 0;
    var quizCount = getValue(module, 'quizCount', 'QuizCount') ?? 0;
    var quizText = quizCount > 0 ? quizCount + ' Quiz' : 'No Quiz';

    return `
        <div class="dashboard-table-row admin-modules-table-row module-row" data-module-id="${moduleId}">
            <div title="${moduleTitle} - ${moduleDescription}">
                <div class="courses-title" title="${moduleTitle}">${moduleTitle}</div>
                <div class="courses-sub-text" title="${moduleDescription}">${moduleDescription}</div>
            </div>

            <div title="${lessonsCount} Lessons">${lessonsCount} Lessons</div>
            <div title="${quizText}">${quizText}</div>
            <div title="${moduleIsActive ? 'Published' : 'Draft'}">${getStatusBadge(moduleIsActive)}</div>

            <div title="Access">
                <label class="admin-toggle">
                    <input type="checkbox"
                           class="module-lock-toggle"
                           data-id="${moduleId}"
                           ${moduleIsAccessible ? 'checked' : ''} />
                </label>
            </div>

            <div title="${moduleDisplayOrder}">${moduleDisplayOrder}</div>

            <div class="admin-courses-actions">
                <a href="/Admin/ModulesLessons/EditModule?courseId=${moduleCourseId}&moduleId=${moduleId}"
                   class="dashboard-btn dashboard-btn-outline admin-course-action-btn"
                   onclick="storeSelectedCourse('${moduleCourseId}'); storeSelectedModule('${moduleId}');">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn admin-delete-soft-btn admin-course-action-btn"
                        onclick="deleteModule(${moduleId})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function loadLessonsForModule(moduleId, courseId, moduleTitle) {
    $.ajax({
        url: '/api/Lesson/module/' + moduleId,
        type: 'GET',

        success: function (lessons) {
            if (!lessons || lessons.length === 0) {
                return;
            }

            stats.lessons += lessons.length;
            updateStatsCards();

            $.each(lessons, function (_, lesson) {
                $('#lessonsContainer').append(buildLessonRow(lesson, moduleId, courseId, moduleTitle));
            });

            filterModuleContent($('#moduleSelect').val());
        },

        error: function (xhr) {
            console.log(xhr.responseText);
            setEmptyState('#lessonsContainer', getMessage(xhr));
        }
    });
}

function buildLessonRow(lesson, moduleId, courseId, moduleTitle) {
    var lessonId = getValue(lesson, 'id', 'Id');
    var lessonTitle = getValue(lesson, 'title', 'Title');
    var lessonDescription = getValue(lesson, 'description', 'Description') ?? '';
    var lessonDisplayOrder = getValue(lesson, 'displayOrder', 'DisplayOrder');
    var lessonIsActive = getValue(lesson, 'isActive', 'IsActive');

    return `
        <div class="dashboard-table-row admin-lessons-table-row"
             data-lesson-id="${lessonId}"
             data-lesson-module-id="${moduleId}">
            <div title="${lessonTitle} - ${lessonDescription}">
                <div class="courses-title" title="${lessonTitle}">${lessonTitle}</div>
                <div class="courses-sub-text" title="${lessonDescription}">${lessonDescription}</div>
            </div>

            <div title="${moduleTitle}">${moduleTitle}</div>
            <div title="Lesson"><span class="admin-course-chip">Lesson</span></div>
            <div title="${lessonIsActive ? 'Published' : 'Draft'}">${getStatusBadge(lessonIsActive)}</div>
            <div title="${lessonDisplayOrder}">${lessonDisplayOrder}</div>

            <div class="admin-courses-actions">
                <a href="/Admin/ModulesLessons/EditLesson?courseId=${courseId}&moduleId=${moduleId}&lessonId=${lessonId}"
                   class="dashboard-btn dashboard-btn-outline admin-course-action-btn"
                   onclick="storeSelectedCourse('${courseId}'); storeSelectedModule('${moduleId}');">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn admin-delete-soft-btn admin-course-action-btn"
                        onclick="deleteLesson(${lessonId})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function loadQuizzesForModule(moduleId, courseId, moduleTitle) {
    $.ajax({
        url: '/api/Quiz/module/' + moduleId,
        type: 'GET',

        success: function (quizzes) {
            if (!quizzes || quizzes.length === 0) {
                return;
            }

            stats.quizzes += quizzes.length;
            updateStatsCards();

            $.each(quizzes, function (_, quiz) {
                $('#quizzesContainer').append(buildQuizRow(quiz, moduleId, courseId, moduleTitle));
            });

            filterModuleContent($('#moduleSelect').val());
        },

        error: function (xhr) {
            console.log(xhr.responseText);
            setEmptyState('#quizzesContainer', getMessage(xhr));
        }
    });
}

function buildQuizRow(quiz, moduleId, courseId, moduleTitle) {
    var quizId = getValue(quiz, 'id', 'Id');
    var quizTitle = getValue(quiz, 'title', 'Title');
    var quizInstructions = getValue(quiz, 'instructions', 'Instructions') ?? '';
    var attemptsAllowed = getValue(quiz, 'attemptsAllowed', 'AttemptsAllowed') ?? 0;
    var questionsCount = getValue(quiz, 'questionsCount', 'QuestionsCount') ?? 0;
    var quizIsActive = getValue(quiz, 'isActive', 'IsActive');
    var quizIsAccessible = getValue(quiz, 'isAccessible', 'IsAccessible');

    return `
        <div class="dashboard-table-row admin-quiz-table-row"
             data-quiz-id="${quizId}"
             data-quiz-module-id="${moduleId}">
            <div title="${quizTitle} - ${quizInstructions}">
                <div class="courses-title" title="${quizTitle}">${quizTitle}</div>
                <div class="courses-sub-text" title="${quizInstructions}">${quizInstructions}</div>
            </div>

            <div title="${moduleTitle}">${moduleTitle}</div>
            <div title="${questionsCount}">${questionsCount}</div>
            <div title="${attemptsAllowed}">${attemptsAllowed}</div>
            <div title="${quizIsActive ? 'Published' : 'Draft'}">${getStatusBadge(quizIsActive)}</div>

            <div title="Access">
                <label class="admin-toggle">
                    <input type="checkbox"
                           class="quiz-lock-toggle"
                           data-id="${quizId}"
                           ${quizIsAccessible ? 'checked' : ''} />
                </label>
            </div>

            <div class="admin-courses-actions">
                <a href="/Admin/ModulesLessons/EditQuiz?courseId=${courseId}&moduleId=${moduleId}&quizId=${quizId}"
                   class="dashboard-btn dashboard-btn-outline admin-course-action-btn"
                   onclick="storeSelectedCourse('${courseId}'); storeSelectedModule('${moduleId}');">
                    Edit
                </a>

                <button type="button"
                        class="dashboard-btn admin-delete-soft-btn admin-course-action-btn"
                        onclick="deleteQuiz(${quizId})">
                    Delete
                </button>
            </div>
        </div>
    `;
}

function deleteModule(moduleId) {
    setDeleteItem(moduleId, 'module', 'Delete Module', 'Are you sure you want to delete this module?', '#modulesContainer');
}

function deleteLesson(lessonId) {
    setDeleteItem(lessonId, 'lesson', 'Delete Lesson', 'Are you sure you want to delete this lesson?', '#lessonsContainer');
}

function deleteQuiz(quizId) {
    setDeleteItem(quizId, 'quiz', 'Delete Quiz', 'Are you sure you want to delete this quiz?', '#quizzesContainer');
}

function setDeleteItem(id, type, title, message, container) {
    deleteItem = { id: id, type: type, container: container };

    $('#deleteConfirmModal h3').text(title);
    $('#deleteConfirmModal p').text(message);
    $('#deleteConfirmModal').addClass('show');
}

$(document).on('click', '#cancelDeleteBtn', function (e) {
    e.preventDefault();
    closeDeleteModal();
});

$(document).on('click', '#confirmDeleteBtn', function (e) {
    e.preventDefault();

    if (!deleteItem.id || !deleteItem.type) {
        return;
    }

    $.ajax({
        url: getDeleteUrl(deleteItem.type, deleteItem.id),
        type: 'DELETE',

        success: function (response) {
            closeDeleteModal();

            storeSelectedCourse($('#courseSelect').val());
            storeSelectedModule($('#moduleSelect').val());

            if (response?.message) {
                sessionStorage.setItem("successMessage", response.message);
            }

            location.reload();
        },

        error: function (xhr) {
            console.log(xhr.responseText);
            setEmptyState(deleteItem.container, getMessage(xhr));
        }
    });
});

function getDeleteUrl(type, id) {
    var urls = {
        module: '/api/Module/delete/',
        lesson: '/api/Lesson/delete/',
        quiz: '/api/Quiz/delete/'
    };

    return urls[type] + id;
}

function closeDeleteModal() {
    $('#deleteConfirmModal').removeClass('show');
    deleteItem = { id: null, type: '', container: '' };
}

function highlightSelectedModule(moduleId) {
    $('.module-row').removeClass('selected-module-row');
    $('.module-row[data-module-id="' + moduleId + '"]').addClass('selected-module-row');
}

function createDeleteConfirmModal() {
    if ($('#deleteConfirmModal').length > 0) {
        return;
    }

    $('body').append(`
        <div id="deleteConfirmModal" class="custom-modal">
            <div class="custom-modal-backdrop"></div>
            <div class="custom-modal-box">
                <h3>Delete Item</h3>
                <p>Are you sure you want to delete this item?</p>

                <div class="custom-modal-actions">
                    <button type="button" id="cancelDeleteBtn" class="dashboard-btn dashboard-btn-outline">
                        Cancel
                    </button>

                    <button type="button" id="confirmDeleteBtn" class="dashboard-btn add-course-btn">
                        Delete
                    </button>
                </div>
            </div>
        </div>
    `);
}