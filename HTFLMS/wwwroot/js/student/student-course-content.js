$(document).ready(function () {
    initStudentCourseContentPage();
});

var studentCourseNotesCache = [];

function initStudentCourseContentPage() {
    bindStudentCourseContentTabs();

    ensureStudentAssignmentSubmissionModal();
    ensureStudentAssignmentUnsubmitModal();
    ensureStudentMessageModal();

    ensureStudentNoteModal();
    ensureStudentNoteDeleteModal();

    bindStudentAssignmentSubmissionEvents();
    bindStudentCourseNotesEvents();

    var courseId = getStudentCourseIdFromUrl();

    if (!courseId) {
        showStudentCourseContentError('Invalid course selected.');
        return;
    }

    loadStudentCourseContentHeader(courseId);
    loadStudentCourseContentInfo(courseId);
    loadStudentCourseContentModules(courseId);
    loadStudentCourseContentMaterialsAssignments(courseId);
    loadStudentCourseContentNotes(courseId);
}

function getStudentCourseIdFromUrl() {
    var parts = window.location.pathname.split('/');
    var lastPart = parts[parts.length - 1];

    var courseId = parseInt(lastPart);

    if (isNaN(courseId) || courseId <= 0) {
        return null;
    }

    return courseId;
}

/* =========================
   HEADER / HERO
========================= */

function loadStudentCourseContentHeader(courseId) {
    $.ajax({
        url: '/api/student/course-content/' + courseId + '/header',
        type: 'GET',
        success: function (header) {
            $('#studentCourseDetailLoader').hide();
            $('#studentCourseDetailContent').show();

            renderStudentCourseContentHero(header);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Course header could not be loaded.'
            );

            showStudentCourseContentError(message);
        }
    });
}

function renderStudentCourseContentHero(course) {
    var title = course.title || 'Untitled Course';
    var image = course.courseImagePath || '/img/course/course-4.webp';
    var trainer = course.trainerName || 'No Trainer';
    var progress = Number(course.progressPercentage || 0);

    if (progress < 0) {
        progress = 0;
    }

    if (progress > 100) {
        progress = 100;
    }

    $('#breadcrumbCourseTitle').text(title);

    $('#studentCourseHero').html(`
        <div class="dashboard-panel student-course-hero">
            <div class="student-course-hero-header">
                <div class="student-course-hero-title-wrap">
                    <div class="student-course-hero-thumb">
                        <img src="${escapeAttribute(image)}" alt="${escapeAttribute(title)}" />
                    </div>

                    <div class="student-course-hero-content">
                        <h1 class="student-course-hero-title">${escapeHtml(title)}</h1>

                        <div class="student-course-hero-meta-row">
                            <span>
                                <i class="bi bi-person"></i>
                                Instructor: <strong>${escapeHtml(trainer)}</strong>
                            </span>

                            <span>
                                <i class="bi bi-award"></i>
                                ${course.certificateIncluded ? 'Certificate Included' : 'Certificate Not Included'}
                            </span>
                        </div>
                    </div>
                </div>

                <a href="/Student/Courses/Index" class="dashboard-btn dashboard-btn-outline">
                    <i class="bi bi-arrow-left"></i> Back to My Courses
                </a>
            </div>

            <div class="student-course-hero-progress-wrap">
                <div class="student-course-hero-progress-top">
                    <span>Overall Progress</span>
                    <span>${progress}% Completed</span>
                </div>

                <div class="student-course-hero-progress">
                    <div class="student-course-hero-progress-fill" style="width:${progress}%;"></div>
                </div>
            </div>
        </div>
    `);
}

/* =========================
   COURSE INFO TAB
========================= */

function loadStudentCourseContentInfo(courseId) {
    $.ajax({
        url: '/api/student/course-content/' + courseId + '/info',
        type: 'GET',
        success: function (info) {
            renderStudentCourseContentInfo(info);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Course info could not be loaded.'
            );

            $('#studentCourseInfoContainer').html(
                '<div class="dashboard-panel text-center p-4 text-danger">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentInfo(course) {
    var image = course.courseImagePath || '/img/course/course-4.webp';
    var title = course.title || 'Untitled Course';

    $('#studentCourseInfoContainer').html(`
        <div class="student-course-info-layout">
            <div class="dashboard-panel student-course-info-side-panel">
                <div class="student-course-side-image-wrap">
                    <img src="${escapeAttribute(image)}"
                         alt="${escapeAttribute(title)}"
                         class="student-course-side-image" />
                </div>

                <div class="student-course-side-block">
                    <div class="info-h">Instructor</div>

                    <div class="taught-by student-course-instructor-card">
                        <div>
                            <div class="teacher-name">${escapeHtml(course.trainerName || 'No Trainer')}</div>
                            <div class="muted">Course Instructor</div>
                        </div>
                    </div>
                </div>

                <div class="student-course-side-block">
                    <div class="info-h">Course details</div>

                    <div class="info-table student-course-details-table">
                        <div class="info-row">
                            <div class="info-key">Category</div>
                            <div class="info-val">${escapeHtml(course.category || 'N/A')}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Total Modules</div>
                            <div class="info-val">${course.totalModules || 0}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Total Lessons</div>
                            <div class="info-val">${course.totalLessons || 0}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Duration</div>
                            <div class="info-val">${escapeHtml(course.durationText || 'N/A')}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Batch Start</div>
                            <div class="info-val">${formatStudentCourseDate(course.batchStartDate)}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Batch End</div>
                            <div class="info-val">${formatStudentCourseDate(course.batchEndDate)}</div>
                        </div>

                        <div class="info-row">
                            <div class="info-key">Certificate</div>
                            <div class="info-val">${course.certificateIncluded ? 'Included' : 'Not Included'}</div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="dashboard-panel student-course-info-main-panel">
                <div class="course-info-hero">
                    <div class="student-course-info-label">Course Overview</div>

                    <h3 class="page-h2 big-title">${escapeHtml(title)}</h3>

                    <p class="student-course-info-subtext">
                        Explore the full details of this course including description, instructor,
                        structure, and learning information.
                    </p>
                </div>

                <div class="student-course-info-section">
                    <div class="info-h">About this course</div>

                    <p class="student-course-info-text">
                        ${escapeHtml(course.description || 'No description added.')}
                    </p>
                </div>
            </div>
        </div>
    `);
}

/* =========================
   MODULES & LESSONS TAB
========================= */

function loadStudentCourseContentModules(courseId) {
    $('#studentModulesContainer').html(
        '<div class="dashboard-panel text-center p-4">Loading modules...</div>'
    );

    $.ajax({
        url: '/api/student/course-content/' + courseId + '/modules-lessons',
        type: 'GET',
        success: function (data) {
            renderStudentCourseContentModules(data.modules || []);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Modules and lessons could not be loaded.'
            );

            $('#studentModulesContainer').html(
                '<div class="dashboard-panel text-center p-4 text-danger">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentModules(modules) {
    var container = $('#studentModulesContainer');
    container.empty();

    if (!modules || modules.length === 0) {
        container.html(
            '<div class="dashboard-panel text-center p-4">No modules added yet.</div>'
        );
        return;
    }

    modules.forEach(function (module, index) {
        var lessons = module.lessons || [];
        var quizzes = module.quizzes || [];
        var total = module.totalLessons || lessons.length;
        var completed = module.completedLessons || 0;
        var percent = module.progressPercentage || 0;
        var locked = module.isAccessible === false;

        var itemsHtml = '';

        lessons.forEach(function (lesson, lessonIndex) {
            var doneClass = lesson.isCompleted ? 'done' : '';
            var titleClass = lesson.isCompleted ? 'done-text' : '';
            var buttonText = lesson.isCompleted ? 'Completed' : 'Mark as done';
            var buttonClass = lesson.isCompleted ? 'done' : '';
            var disabled = lesson.isCompleted ? 'disabled' : '';

            itemsHtml += `
                <div class="student-lesson-item js-lesson ${doneClass}">
                    <div class="student-lesson-main">
                        <button type="button" class="student-lesson-toggle js-lesson-toggle">
                            <div class="student-lesson-left">
                                <div class="student-lesson-type-icon">
                                    <i class="bi bi-file-earmark-text"></i>
                                </div>

                                <div>
                                    <div class="student-lesson-title ${titleClass}">
                                        ${escapeHtml(lesson.title)}
                                    </div>
                                    <div class="student-lesson-meta">Lesson ${lessonIndex + 1}</div>
                                </div>
                            </div>

                            <span class="student-lesson-chevron">
                                <i class="bi bi-chevron-down"></i>
                            </span>
                        </button>

                        <div class="student-lesson-side">
                            <button type="button"
                                    class="student-done-btn js-done-btn ${buttonClass}"
                                    data-lesson-id="${lesson.id}"
                                    ${disabled}>
                                ${buttonText}
                            </button>
                        </div>
                    </div>

                    <div class="student-lesson-dropdown">
                        <p>${escapeHtml(lesson.description || 'No description added.')}</p>
                    </div>
                </div>
            `;
        });

        quizzes.forEach(function (quiz) {
            itemsHtml += renderStudentCourseContentQuizItem(quiz, module.id);
        });

        container.append(`
            <div class="student-module-card ${index === 0 ? 'expanded' : ''} ${locked ? 'locked' : ''} js-module"
                 data-total="${total}">
                <button type="button" class="student-module-head">
                    <div class="student-module-head-left">
                        <div class="student-module-icon ${locked ? 'muted-lock' : module.isCompleted ? 'success' : 'primary'} js-module-icon">
                            <i class="bi ${locked ? 'bi-lock' : module.isCompleted ? 'bi-check-lg' : 'bi-book'}"></i>
                        </div>

                        <div>
                            <div class="student-module-title">${escapeHtml(module.title)}</div>
                            <div class="student-module-desc">${escapeHtml(module.description || '')}</div>
                        </div>
                    </div>

                    <div class="student-module-chevron">
                        <i class="bi bi-chevron-down"></i>
                    </div>
                </button>

                <div class="student-module-progress-row">
                    <span class="js-module-count">${completed}/${total} lessons completed</span>

                    <div class="student-module-progress-right">
                        <span class="js-module-percent">${percent}%</span>
                        <span class="student-status-badge ${locked ? 'muted' : module.isCompleted ? 'dark' : completed > 0 ? 'light' : 'muted'} js-module-status">
                            ${locked ? 'Locked' : escapeHtml(module.statusText || 'Not Started')}
                        </span>
                    </div>
                </div>

                <div class="student-module-progress">
                    <div class="student-module-progress-fill js-module-fill" style="width:${percent}%;"></div>
                </div>

                ${locked ? '' : '<div class="student-lesson-list">' + itemsHtml + '</div>'}
            </div>
        `);
    });

    bindStudentCourseContentModuleEvents();
}

function renderStudentCourseContentQuizItem(quiz, moduleId) {
    var statusText = quiz.statusText || 'Start Quiz';
    var isCompleted = quiz.isPassed === true;
    var isLocked = quiz.isLocked === true;

    var quizClass = isCompleted ? 'done' : '';
    var titleClass = isCompleted ? 'done-text' : '';

    var statusBadgeClass = isCompleted
        ? 'student-quiz-status-completed'
        : isLocked
            ? 'student-quiz-status-locked'
            : 'student-quiz-status-pending';

    var meta = isLocked && quiz.lockedUntil
        ? 'Locked until ' + formatStudentCourseDateTime(quiz.lockedUntil)
        : (quiz.questionsCount || 0) + ' Questions • Passing: 60% • Attempts left: ' + (quiz.attemptsLeft ?? 3);

    var actionButtons = '';

    if (isLocked) {
        actionButtons += `
            <button type="button"
                    class="dashboard-btn dashboard-btn-outline js-view-quiz-review"
                    data-quiz-id="${quiz.id}">
                <i class="bi bi-eye"></i> View Attempt
            </button>
        `;
    } else if (isCompleted) {
        actionButtons += `
            <button type="button"
                    class="dashboard-btn dashboard-btn-outline js-view-quiz-review"
                    data-quiz-id="${quiz.id}">
                <i class="bi bi-eye"></i> View Quiz
            </button>
        `;
    } else if (quiz.canViewAttempt && quiz.canRetake) {
        actionButtons += `
            <button type="button"
                    class="dashboard-btn dashboard-btn-outline js-view-quiz-review"
                    data-quiz-id="${quiz.id}">
                <i class="bi bi-eye"></i> View Attempt
            </button>

            <button type="button"
                    class="dashboard-btn dashboard-btn-primary js-start-quiz"
                    data-module-id="${moduleId}"
                    data-quiz-id="${quiz.id}">
                <i class="bi bi-arrow-repeat"></i> Retake Quiz
            </button>
        `;
    } else if (quiz.canViewAttempt && !quiz.canRetake) {
        actionButtons += `
            <button type="button"
                    class="dashboard-btn dashboard-btn-outline js-view-quiz-review"
                    data-quiz-id="${quiz.id}">
                <i class="bi bi-eye"></i> View Attempt
            </button>
        `;
    } else {
        actionButtons += `
            <button type="button"
                    class="dashboard-btn dashboard-btn-primary js-start-quiz"
                    data-module-id="${moduleId}"
                    data-quiz-id="${quiz.id}">
                <i class="bi bi-play-circle"></i> Start Quiz
            </button>
        `;
    }

    return `
        <div class="student-lesson-item js-lesson js-quiz-lesson ${quizClass}"
             data-module-id="${moduleId}"
             data-quiz-id="${quiz.id}">
            <div class="student-lesson-main">
                <button type="button" class="student-lesson-toggle js-lesson-toggle">
                    <div class="student-lesson-left">
                        <div class="student-lesson-type-icon">
                            <i class="bi bi-patch-question"></i>
                        </div>

                        <div>
                            <div class="student-lesson-title ${titleClass}">
                                ${escapeHtml(quiz.title)}
                            </div>
                            <div class="student-lesson-meta">
                                ${escapeHtml(meta)}
                            </div>
                        </div>
                    </div>

                    <span class="student-lesson-chevron">
                        <i class="bi bi-chevron-down"></i>
                    </span>
                </button>

                <div class="student-lesson-side">
                    <span class="student-quiz-status-badge ${statusBadgeClass}">
                        ${escapeHtml(statusText)}
                    </span>
                </div>
            </div>

            <div class="student-lesson-dropdown student-course-quiz-dropdown">
                <div class="student-quiz-box">
                    <div class="student-quiz-instructions">
                        <strong>Instructions:</strong>
                        ${escapeHtml(quiz.instructions || 'Complete this quiz to unlock the next module.')}
                    </div>

                    <div class="student-quiz-small-meta">
                        Score: ${quiz.lastScorePercentage === null || quiz.lastScorePercentage === undefined ? 'N/A' : quiz.lastScorePercentage + '%'}
                        • Attempts used: ${quiz.attemptsUsed || 0}/3
                    </div>

                    <div class="student-quiz-actions">
                        ${actionButtons}
                    </div>

                    <div class="student-quiz-result js-quiz-result"></div>
                    <div class="student-quiz-dynamic-area js-quiz-dynamic-area"></div>
                </div>
            </div>
        </div>
    `;
}

function bindStudentCourseContentModuleEvents() {
    $(document).off('click', '.student-module-head').on('click', '.student-module-head', function () {
        var module = $(this).closest('.student-module-card');

        if (!module.hasClass('locked')) {
            module.toggleClass('expanded');
        }
    });

    $(document).off('click', '.js-lesson-toggle').on('click', '.js-lesson-toggle', function (e) {
        e.stopPropagation();
        $(this).closest('.js-lesson').toggleClass('open');
    });

    $(document).off('click', '.js-done-btn').on('click', '.js-done-btn', function (e) {
        e.stopPropagation();

        var button = $(this);
        var lessonId = button.data('lesson-id');

        if (!lessonId || button.prop('disabled')) {
            return;
        }

        button.prop('disabled', true).text('Saving...');

        $.ajax({
            url: '/api/student/course-content/lessons/' + lessonId + '/mark-done',
            type: 'POST',
            success: function () {
                var courseId = getStudentCourseIdFromUrl();

                loadStudentCourseContentModules(courseId);
                loadStudentCourseContentHeader(courseId);
                loadStudentCourseContentInfo(courseId);
                loadStudentCourseContentMaterialsAssignments(courseId);
            },
            error: function (xhr) {
                var message = getStudentCourseContentErrorMessage(
                    xhr,
                    'Lesson could not be marked as completed.'
                );

                showStudentMessageModal('Error', message);
                button.prop('disabled', false).text('Mark as done');
            }
        });
    });

    $(document).off('click', '.js-start-quiz').on('click', '.js-start-quiz', function (e) {
        e.stopPropagation();

        var button = $(this);
        var moduleId = button.data('module-id');
        var quizItem = button.closest('.js-quiz-lesson');

        loadStudentCourseContentQuiz(moduleId, quizItem);
    });

    $(document).off('click', '.js-submit-quiz').on('click', '.js-submit-quiz', function (e) {
        e.stopPropagation();

        var button = $(this);
        var moduleId = button.data('module-id');
        var quizId = button.data('quiz-id');
        var quizItem = button.closest('.js-quiz-lesson');

        submitStudentCourseContentQuiz(moduleId, quizId, quizItem);
    });

    $(document).off('click', '.js-view-quiz-review').on('click', '.js-view-quiz-review', function (e) {
        e.stopPropagation();

        var quizId = $(this).data('quiz-id');
        var quizItem = $(this).closest('.js-quiz-lesson');

        loadStudentCourseContentQuizReview(quizId, quizItem);
    });
}

/* =========================
   QUIZ
========================= */

function loadStudentCourseContentQuiz(moduleId, quizItem) {
    var dynamicArea = quizItem.find('.js-quiz-dynamic-area');
    var resultBox = quizItem.find('.js-quiz-result');

    resultBox.removeClass('success error').text('');
    dynamicArea.html('<div class="student-panel-sub">Loading quiz...</div>');

    $.ajax({
        url: '/api/student/course-content/modules/' + moduleId + '/quiz',
        type: 'GET',
        success: function (quiz) {
            renderStudentCourseContentQuizForm(quiz, quizItem);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Quiz could not be loaded.'
            );

            dynamicArea.html(
                '<div class="student-quiz-message error">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentQuizForm(quiz, quizItem) {
    var dynamicArea = quizItem.find('.js-quiz-dynamic-area');

    if (quiz.isPassed) {
        dynamicArea.html(
            '<div class="student-quiz-message success">You have already passed this quiz. You can view your quiz review.</div>'
        );
        return;
    }

    if (quiz.isLocked) {
        dynamicArea.html(
            '<div class="student-quiz-message error">Quiz is locked until ' +
            formatStudentCourseDateTime(quiz.lockedUntil) +
            '.</div>'
        );
        return;
    }

    var questions = quiz.questions || [];

    if (questions.length === 0) {
        dynamicArea.html(
            '<div class="student-quiz-message error">No questions are added in this quiz yet.</div>'
        );
        return;
    }

    var questionsHtml = questions.map(function (question, index) {
        var optionsHtml = (question.options || []).map(function (option) {
            return `
                <label class="student-quiz-option">
                    <input type="radio"
                           name="quiz_${quiz.quizId}_question_${question.questionId}"
                           value="${option.optionId}">
                    <span>${escapeHtml(option.optionText)}</span>
                </label>
            `;
        }).join('');

        return `
            <div class="student-quiz-question" data-question-id="${question.questionId}">
                <p class="student-quiz-question-title">
                    ${index + 1}. ${escapeHtml(question.questionText)}
                </p>
                <div class="student-quiz-options">
                    ${optionsHtml}
                </div>
            </div>
        `;
    }).join('');

    dynamicArea.html(`
        <div class="student-quiz-form"
             data-module-id="${quiz.moduleId}"
             data-quiz-id="${quiz.quizId}">
            <div class="student-quiz-small-meta">
                Passing: ${quiz.passingPercentage}% • Attempts left: ${quiz.attemptsLeft}/${quiz.attemptsAllowed}
            </div>

            ${questionsHtml}

            <div class="student-quiz-actions">
                <button type="button"
                        class="dashboard-btn dashboard-btn-primary js-submit-quiz"
                        data-module-id="${quiz.moduleId}"
                        data-quiz-id="${quiz.quizId}">
                    <i class="bi bi-check2-circle"></i> Submit Quiz
                </button>
            </div>
        </div>
    `);
}

function submitStudentCourseContentQuiz(moduleId, quizId, quizItem) {
    var resultBox = quizItem.find('.js-quiz-result');
    var dynamicArea = quizItem.find('.js-quiz-dynamic-area');
    var answers = [];
    var allAnswered = true;

    dynamicArea.find('.student-quiz-question').each(function () {
        var question = $(this);
        var questionId = parseInt(question.data('question-id'));
        var selected = question.find('input[type="radio"]:checked');

        question.removeClass('student-quiz-question-error');

        if (selected.length === 0) {
            allAnswered = false;
            question.addClass('student-quiz-question-error');
            return;
        }

        answers.push({
            questionId: questionId,
            selectedOptionId: parseInt(selected.val())
        });
    });

    if (!allAnswered) {
        resultBox
            .removeClass('success')
            .addClass('error')
            .text('Please answer all questions first.');
        return;
    }

    resultBox.removeClass('success error').text('Submitting quiz...');

    $.ajax({
        url: '/api/student/course-content/modules/' + moduleId + '/quiz/submit',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            moduleId: moduleId,
            quizId: quizId,
            answers: answers
        }),
        success: function (result) {
            resultBox
                .removeClass('success error')
                .addClass(result.isPassed ? 'success' : 'error')
                .text(result.message || 'Quiz submitted.');

            var actionHtml = `
                <div class="student-quiz-actions mt-2">
                    <button type="button"
                            class="dashboard-btn dashboard-btn-outline js-view-quiz-review"
                            data-quiz-id="${quizId}">
                        <i class="bi bi-eye"></i> View Attempt
                    </button>
            `;

            if (result.canRetake) {
                actionHtml += `
                    <button type="button"
                            class="dashboard-btn dashboard-btn-primary js-start-quiz"
                            data-module-id="${moduleId}"
                            data-quiz-id="${quizId}">
                        <i class="bi bi-arrow-repeat"></i> Retake Quiz
                    </button>
                `;
            }

            actionHtml += '</div>';

            dynamicArea.html(actionHtml);

            var courseId = getStudentCourseIdFromUrl();
            loadStudentCourseContentModules(courseId);
            loadStudentCourseContentHeader(courseId);
            loadStudentCourseContentInfo(courseId);
            loadStudentCourseContentMaterialsAssignments(courseId);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Quiz could not be submitted.'
            );

            resultBox
                .removeClass('success')
                .addClass('error')
                .text(message);
        }
    });
}

function loadStudentCourseContentQuizReview(quizId, quizItem) {
    var dynamicArea = quizItem.find('.js-quiz-dynamic-area');
    var resultBox = quizItem.find('.js-quiz-result');

    resultBox.removeClass('success error').text('');
    dynamicArea.html('<div class="student-panel-sub">Loading quiz review...</div>');

    $.ajax({
        url: '/api/student/course-content/quizzes/' + quizId + '/review',
        type: 'GET',
        success: function (review) {
            renderStudentCourseContentQuizReview(review, quizItem);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Quiz review could not be loaded.'
            );

            dynamicArea.html(
                '<div class="student-quiz-message error">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentQuizReview(review, quizItem) {
    var dynamicArea = quizItem.find('.js-quiz-dynamic-area');

    var summaryClass = review.isPassed ? 'success' : 'error';
    var summaryText = review.isPassed ? 'Passed' : 'Failed';

    var questionsHtml = (review.questions || []).map(function (question, index) {
        var optionsHtml = (question.options || []).map(function (option) {
            var optionClass = '';

            if (option.isSelected && option.isSelectedCorrect) {
                optionClass = 'student-quiz-option-correct';
            } else if (option.isSelected && !option.isSelectedCorrect) {
                optionClass = 'student-quiz-option-wrong';
            } else if (option.isCorrectAnswer) {
                optionClass = 'student-quiz-option-correct-answer';
            }

            var label = '';

            if (option.isSelected) {
                label += '<span class="student-quiz-answer-label">Your answer</span>';
            }

            if (option.isCorrectAnswer && review.revealCorrectAnswers) {
                label += '<span class="student-quiz-answer-label correct">Correct answer</span>';
            }

            return `
                <div class="student-quiz-review-option ${optionClass}">
                    <span>${escapeHtml(option.optionText)}</span>
                    ${label}
                </div>
            `;
        }).join('');

        return `
            <div class="student-quiz-review-question">
                <p class="student-quiz-question-title">
                    ${index + 1}. ${escapeHtml(question.questionText)}
                </p>
                <div class="student-quiz-options">
                    ${optionsHtml}
                </div>
            </div>
        `;
    }).join('');

    dynamicArea.html(`
        <div class="student-quiz-review">
            <div class="student-quiz-review-summary ${summaryClass}">
                <strong>${summaryText}</strong>
                <span>Score: ${review.scorePercentage}%</span>
                <span>Correct: ${review.correctAnswers}/${review.totalQuestions}</span>
                <span>Submitted: ${formatStudentCourseDateTime(review.submittedAt)}</span>
            </div>

            ${!review.revealCorrectAnswers ? `
                <div class="student-quiz-message warning">
                    Correct answers are hidden until you pass the quiz.
                </div>
            ` : ''}

            ${questionsHtml}
        </div>
    `);
}

/* =========================
   SLIDES & ASSIGNMENTS TAB
========================= */

function loadStudentCourseContentMaterialsAssignments(courseId) {
    $('#studentSlidesAssignmentsContainer').html(
        '<div class="dashboard-panel text-center p-4">Loading slides and assignments...</div>'
    );

    $.ajax({
        url: '/api/student/course-content/' + courseId + '/materials-assignments',
        type: 'GET',
        success: function (data) {
            renderStudentCourseContentMaterialsAssignments(data);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Slides and assignments could not be loaded.'
            );

            $('#studentSlidesAssignmentsContainer').html(
                '<div class="dashboard-panel text-center p-4 text-danger">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentMaterialsAssignments(data) {
    var container = $('#studentSlidesAssignmentsContainer');
    var materials = data.materials || [];
    var assignments = data.assignments || [];

    container.empty();

    if (!materials.length && !assignments.length) {
        container.html(
            '<div class="dashboard-panel text-center p-4">No materials or assignments added yet.</div>'
        );
        return;
    }

    container.append(`
        <div class="dashboard-panel">
            <div class="student-module-section-block">
                <div class="dashboard-panel-head student-panel-head">
                    <div>
                        <h4 class="page-h2 student-panel-title">Slides &amp; Videos</h4>
                        <p class="student-panel-sub">Students can view course material uploaded by the trainer.</p>
                    </div>

                    <div class="student-count-pill">Total: ${materials.length} files</div>
                </div>

                <div class="student-material-list">
                    ${renderStudentCourseContentMaterialsList(materials)}
                </div>
            </div>
        </div>

        <div class="dashboard-panel">
            <div class="student-module-section-block">
                <div class="dashboard-panel-head student-panel-head">
                    <div>
                        <h4 class="page-h2 student-panel-title">Assignments</h4>
                        <p class="student-panel-sub">Students can view assignments for this course.</p>
                    </div>

                    <div class="student-count-pill">Total: ${assignments.length} assignments</div>
                </div>

                <div class="student-assignment-list">
                    ${renderStudentCourseContentAssignmentsList(assignments)}
                </div>
            </div>
        </div>
    `);
}

function renderStudentCourseContentMaterialsList(materials) {
    if (!materials || !materials.length) {
        return '<p class="student-panel-sub">No slides or videos added.</p>';
    }

    return materials.map(function (m) {
        var isLocked = m.isLocked === true;
        var moduleTitle = m.moduleTitle || 'Module not specified';
        var iconClass = isLocked ? 'muted-lock' : getMaterialClass(m.contentType, m.filePath);
        var icon = isLocked ? 'bi-lock' : getMaterialIcon(m.contentType, m.filePath);
        var lockStatus = isLocked
            ? '<span class="student-status-badge muted">Locked</span>'
            : '';

        return `
            <div class="student-material-card ${isLocked ? 'locked' : ''}">
                <div class="student-material-main">
                    <div class="student-material-left">
                        <div class="student-material-icon ${iconClass}">
                            <i class="bi ${icon}"></i>
                        </div>

                        <div class="student-material-content">
                            <h4 class="student-material-title">
                                ${escapeHtml(m.title)}
                                ${lockStatus}
                            </h4>

                            <div class="student-material-meta">
                                <span>Module: ${escapeHtml(moduleTitle)}</span>
                                <span>•</span>
                                <span>Type: ${escapeHtml(m.contentType || getStudentFileExtension(m.filePath) || 'File')}</span>
                                ${getMaterialExtra(m)}
                            </div>
                        </div>
                    </div>

                    <div class="student-material-right">
                        ${renderStudentMaterialActions(m)}
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function renderStudentMaterialActions(m) {
    if (m.isLocked) {
        return `
            <button type="button" class="student-material-btn" disabled>
                <i class="bi bi-lock"></i> Locked
            </button>
        `;
    }

    var html = '';

    if (m.filePath) {
        if (isStudentFileViewable(m.filePath, m.contentType)) {
            html += `
                <a href="${escapeAttribute(m.filePath)}" target="_blank" class="student-material-btn">
                    <i class="bi bi-eye"></i> View File
                </a>

                <a href="${escapeAttribute(m.filePath)}" download class="student-material-btn download">
                    <i class="bi bi-download"></i> Download
                </a>
            `;
        } else {
            html += `
                <a href="${escapeAttribute(m.filePath)}" download class="student-material-btn download">
                    <i class="bi bi-download"></i> Download
                </a>
            `;
        }
    }

    if (m.externalUrl) {
        html += `
            <a href="${escapeAttribute(m.externalUrl)}" target="_blank" class="student-material-btn">
                <i class="bi bi-box-arrow-up-right"></i> Open Link
            </a>
        `;
    }

    if (!html) {
        html = `
            <button type="button" class="student-material-btn" disabled>
                <i class="bi bi-file-earmark-x"></i> No file
            </button>
        `;
    }

    return html;
}

function renderStudentCourseContentAssignmentsList(assignments) {
    if (!assignments || !assignments.length) {
        return '<p class="student-panel-sub">No assignments added.</p>';
    }

    return assignments.map(function (a) {
        var isLocked = a.isLocked === true;
        var moduleTitle = a.moduleTitle || 'Module not specified';
        var due = a.dueDateTime ? formatStudentAssignmentDateTime(a.dueDateTime) : 'No due date';
        var status = getStudentAssignmentStatusInfo(a);

        return `
            <div class="student-assignment-card ${isLocked ? 'locked' : ''}" data-assignment-id="${a.id}">
                <div class="student-assignment-main">
                    <div class="student-assignment-left">
                        <h4 class="student-assignment-title">
                            ${escapeHtml(a.title)}
                        </h4>

                        <div class="student-assignment-meta">
                            <span>Module: ${escapeHtml(moduleTitle)}</span>
                            <span>•</span>
                            <span>Marks: ${a.marks || 0}</span>
                            <span>•</span>
                            <span>Due: ${due}</span>
                        </div>

                        <p class="student-panel-sub">${escapeHtml(a.description || '')}</p>

                        ${a.isSubmissionClosed ? `
                            <p class="student-panel-sub">Submission closed. Due date has passed.</p>
                        ` : ''}

                        ${a.isSubmitted && a.submittedAt ? `
                            <p class="student-panel-sub">Submitted: ${formatStudentCourseDateTime(a.submittedAt)}</p>
                        ` : ''}

                        ${a.isGraded ? `
                            <p class="student-panel-sub">
                                Marks: ${a.obtainedMarks ?? 0}/${a.marks || 0}
                                ${a.feedback ? ` • Feedback: ${escapeHtml(a.feedback)}` : ''}
                            </p>
                        ` : ''}
                    </div>

                    <div class="student-assignment-right">
                        <span class="${status.className}">
                            ${escapeHtml(status.text)}
                        </span>

                        ${a.filePath ? `
                            <a href="${escapeAttribute(a.filePath)}"
                               target="_blank"
                               class="student-assignment-link">
                                <i class="bi bi-download"></i> View Assignment
                            </a>
                        ` : ''}

                        ${renderStudentAssignmentAction(a)}
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function getStudentAssignmentStatusInfo(a) {
    if (a.isLocked) {
        return {
            text: 'Locked',
            className: 'student-status-badge muted'
        };
    }

    if (a.isSubmissionClosed && !a.isSubmitted) {
        return {
            text: 'Submission Closed',
            className: 'student-status-badge muted'
        };
    }

    if (a.isGraded) {
        return {
            text: 'Graded',
            className: 'student-status-badge dark'
        };
    }

    if (a.isSubmitted) {
        return {
            text: 'Submitted',
            className: 'student-status-badge dark'
        };
    }

    return {
        text: 'Pending Submission',
        className: 'student-assignment-status pending'
    };
}

function renderStudentAssignmentAction(a) {
    if (a.isLocked) {
        return `
            <span class="student-assignment-link">
                <i class="bi bi-lock"></i> Locked
            </span>
        `;
    }

    if (a.isSubmitted) {
        var viewButton = renderStudentSubmittedSolutionButton(a);
        var unsubmitButton = '';

        if (!a.isSubmissionClosed && !a.isGraded) {
            unsubmitButton = `
                <button type="button"
                        class="student-assignment-link js-open-unsubmit-assignment"
                        data-assignment-id="${a.id}"
                        data-assignment-title="${escapeAttribute(a.title)}">
                    <i class="bi bi-x-circle"></i> Unsubmit
                </button>
            `;
        }

        return viewButton + unsubmitButton;
    }

    if (a.isSubmissionClosed) {
        return `
            <span class="student-assignment-link">
                <i class="bi bi-lock"></i> Submission Closed
            </span>
        `;
    }

    return `
        <button type="button"
                class="dashboard-btn dashboard-btn-outline student-submit-btn js-open-assignment-submit"
                data-assignment-id="${a.id}"
                data-assignment-title="${escapeAttribute(a.title)}">
            <i class="bi bi-upload"></i> Submit Work
        </button>
    `;
}

function renderStudentSubmittedSolutionButton(a) {
    if (a.submittedFilePath) {
        if (isStudentFileViewable(a.submittedFilePath, '')) {
            return `
                <a href="${escapeAttribute(a.submittedFilePath)}"
                   target="_blank"
                   class="student-assignment-link">
                    <i class="bi bi-eye"></i> View Submitted File
                </a>
            `;
        }

        return `
            <a href="${escapeAttribute(a.submittedFilePath)}"
               download
               class="student-assignment-link">
                <i class="bi bi-download"></i> View Submitted File
            </a>
        `;
    }

    if (a.submittedText) {
        return `
            <a href="${escapeAttribute(a.submittedText)}"
               target="_blank"
               class="student-assignment-link">
                <i class="bi bi-box-arrow-up-right"></i> View Submitted Link
            </a>
        `;
    }

    return '';
}

/* =========================
   ASSIGNMENT SUBMISSION
========================= */

function ensureStudentAssignmentSubmissionModal() {
    $('#studentAssignmentSubmitModal').remove();
    $('#studentAssignmentSubmitModalBackdrop').remove();

    $('body').append(`
        <div id="studentAssignmentSubmitModalBackdrop"
             style="display:none; position:fixed; inset:0; background:rgba(0,0,0,0.45); z-index:1040;"></div>

        <div id="studentAssignmentSubmitModal"
             style="display:none; position:fixed; inset:0; z-index:1050; overflow:auto;">
            <div style="min-height:100%; display:flex; align-items:center; justify-content:center; padding:20px;">
                <div class="dashboard-panel" style="width:100%; max-width:560px;">
                    <form id="studentAssignmentSubmitForm">
                        <div class="dashboard-panel-head student-panel-head">
                            <div>
                                <h4 class="page-h2 student-panel-title">Submit Assignment</h4>
                                <p class="student-panel-sub">Upload your solution file or paste a solution link.</p>
                            </div>

                            <button type="button"
                                    class="dashboard-btn dashboard-btn-outline js-close-assignment-modal">
                                <i class="bi bi-x-lg"></i>
                            </button>
                        </div>

                        <input type="hidden" id="studentSubmitAssignmentId" />

                        <div class="mb-3">
                            <label class="form-label">Assignment</label>
                            <input type="text" id="studentSubmitAssignmentTitle" class="form-control" readonly />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Upload solution file</label>
                            <input type="file"
                                   id="studentAssignmentSolutionFile"
                                   class="form-control"
                                   accept=".pdf,.doc,.docx,.ppt,.pptx,.xls,.xlsx,.jpg,.jpeg,.png,.zip,.rar,.mp4,.webm" />
                            <div class="form-text">
                                Max size: 50 MB. Allowed: PDF, Word, PPT, Excel, images, ZIP/RAR, MP4, WEBM.
                            </div>
                        </div>

                        <div class="text-center text-muted mb-3">OR</div>

                        <div class="mb-3">
                            <label class="form-label">Paste solution link</label>
                            <input type="url"
                                   id="studentAssignmentSolutionLink"
                                   class="form-control"
                                   placeholder="https://drive.google.com/..." />
                            <div class="form-text">
                                Use this option if file/video size exceeds 50 MB.
                            </div>
                        </div>

                        <div id="studentAssignmentSubmitMessage"
                             class="student-quiz-message error"
                             style="display:none;"></div>

                        <div class="student-quiz-actions mt-3">
                            <button type="button"
                                    class="dashboard-btn dashboard-btn-outline js-close-assignment-modal">
                                Cancel
                            </button>

                            <button type="submit"
                                    class="dashboard-btn dashboard-btn-primary"
                                    id="studentAssignmentSubmitBtn">
                                <i class="bi bi-upload"></i> Submit
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    `);
}

function ensureStudentAssignmentUnsubmitModal() {
    $('#studentAssignmentUnsubmitModal').remove();
    $('#studentAssignmentUnsubmitModalBackdrop').remove();

    $('body').append(`
        <div id="studentAssignmentUnsubmitModalBackdrop"
             style="display:none; position:fixed; inset:0; background:rgba(0,0,0,0.45); z-index:1040;"></div>

        <div id="studentAssignmentUnsubmitModal"
             style="display:none; position:fixed; inset:0; z-index:1050; overflow:auto;">
            <div style="min-height:100%; display:flex; align-items:center; justify-content:center; padding:20px;">
                <div class="dashboard-panel" style="width:100%; max-width:520px;">
                    <input type="hidden" id="studentUnsubmitAssignmentId" />

                    <div class="dashboard-panel-head student-panel-head">
                        <div>
                            <h4 class="page-h2 student-panel-title">Unsubmit Assignment</h4>
                            <p class="student-panel-sub" id="studentUnsubmitAssignmentTitleText">
                                Are you sure you want to unsubmit this assignment solution?
                            </p>
                        </div>

                        <button type="button"
                                class="dashboard-btn dashboard-btn-outline js-close-unsubmit-modal">
                            <i class="bi bi-x-lg"></i>
                        </button>
                    </div>

                    <p class="student-panel-sub">
                        Your submitted solution will be removed. You can submit again before the due date.
                    </p>

                    <div class="student-quiz-actions mt-3">
                        <button type="button"
                                class="dashboard-btn dashboard-btn-outline js-close-unsubmit-modal">
                            Cancel
                        </button>

                        <button type="button"
                                class="dashboard-btn dashboard-btn-primary"
                                id="studentAssignmentUnsubmitConfirmBtn">
                            <i class="bi bi-x-circle"></i> Unsubmit
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `);
}

function ensureStudentMessageModal() {
    $('#studentMessageModal').remove();
    $('#studentMessageModalBackdrop').remove();

    $('body').append(`
        <div id="studentMessageModalBackdrop"
             style="display:none; position:fixed; inset:0; background:rgba(0,0,0,0.45); z-index:1060;"></div>

        <div id="studentMessageModal"
             style="display:none; position:fixed; inset:0; z-index:1070; overflow:auto;">
            <div style="min-height:100%; display:flex; align-items:center; justify-content:center; padding:20px;">
                <div class="dashboard-panel" style="width:100%; max-width:480px;">
                    <div class="dashboard-panel-head student-panel-head">
                        <div>
                            <h4 class="page-h2 student-panel-title" id="studentMessageModalTitle">Message</h4>
                            <p class="student-panel-sub" id="studentMessageModalText"></p>
                        </div>
                    </div>

                    <div class="student-quiz-actions mt-3">
                        <button type="button"
                                class="dashboard-btn dashboard-btn-primary js-close-message-modal">
                            OK
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `);
}

function bindStudentAssignmentSubmissionEvents() {
    $(document).off('click', '.js-open-assignment-submit').on('click', '.js-open-assignment-submit', function () {
        var assignmentId = $(this).data('assignment-id');
        var title = $(this).data('assignment-title') || 'Assignment';

        openStudentAssignmentSubmitModal(assignmentId, title);
    });

    $(document).off('click', '.js-close-assignment-modal').on('click', '.js-close-assignment-modal', function () {
        closeStudentAssignmentSubmitModal();
    });

    $(document).off('click', '#studentAssignmentSubmitModalBackdrop').on('click', '#studentAssignmentSubmitModalBackdrop', function () {
        closeStudentAssignmentSubmitModal();
    });

    $(document).off('submit', '#studentAssignmentSubmitForm').on('submit', '#studentAssignmentSubmitForm', function (e) {
        e.preventDefault();
        submitStudentAssignmentSolution();
    });

    $(document).off('click', '.js-open-unsubmit-assignment').on('click', '.js-open-unsubmit-assignment', function () {
        var assignmentId = $(this).data('assignment-id');
        var title = $(this).data('assignment-title') || 'this assignment';

        openStudentAssignmentUnsubmitModal(assignmentId, title);
    });

    $(document).off('click', '.js-close-unsubmit-modal').on('click', '.js-close-unsubmit-modal', function () {
        closeStudentAssignmentUnsubmitModal();
    });

    $(document).off('click', '#studentAssignmentUnsubmitModalBackdrop').on('click', '#studentAssignmentUnsubmitModalBackdrop', function () {
        closeStudentAssignmentUnsubmitModal();
    });

    $(document).off('click', '#studentAssignmentUnsubmitConfirmBtn').on('click', '#studentAssignmentUnsubmitConfirmBtn', function () {
        var assignmentId = $('#studentUnsubmitAssignmentId').val();

        if (!assignmentId) {
            return;
        }

        unsubmitStudentAssignmentSolution(assignmentId);
    });

    $(document).off('click', '.js-close-message-modal').on('click', '.js-close-message-modal', function () {
        closeStudentMessageModal();
    });

    $(document).off('click', '#studentMessageModalBackdrop').on('click', '#studentMessageModalBackdrop', function () {
        closeStudentMessageModal();
    });

    $(document).off('change', '#studentAssignmentSolutionFile').on('change', '#studentAssignmentSolutionFile', function () {
        var file = this.files && this.files.length ? this.files[0] : null;

        if (!file) {
            return;
        }

        var message = validateStudentAssignmentFile(file);

        if (message) {
            showStudentAssignmentSubmitMessage(message);
            $(this).val('');
        } else {
            hideStudentAssignmentSubmitMessage();
        }
    });
}

function openStudentAssignmentSubmitModal(assignmentId, title) {
    $('#studentSubmitAssignmentId').val(assignmentId);
    $('#studentSubmitAssignmentTitle').val(title);
    $('#studentAssignmentSolutionFile').val('');
    $('#studentAssignmentSolutionLink').val('');
    hideStudentAssignmentSubmitMessage();

    $('#studentAssignmentSubmitModalBackdrop').show();
    $('#studentAssignmentSubmitModal').show();

    $('body').css('overflow', 'hidden');
}

function closeStudentAssignmentSubmitModal() {
    $('#studentAssignmentSubmitModal').hide();
    $('#studentAssignmentSubmitModalBackdrop').hide();

    $('body').css('overflow', '');
}

function openStudentAssignmentUnsubmitModal(assignmentId, title) {
    $('#studentUnsubmitAssignmentId').val(assignmentId);
    $('#studentUnsubmitAssignmentTitleText').text(
        'Are you sure you want to unsubmit "' + title + '"?'
    );

    $('#studentAssignmentUnsubmitModalBackdrop').show();
    $('#studentAssignmentUnsubmitModal').show();

    $('body').css('overflow', 'hidden');
}

function closeStudentAssignmentUnsubmitModal() {
    $('#studentAssignmentUnsubmitModal').hide();
    $('#studentAssignmentUnsubmitModalBackdrop').hide();

    $('body').css('overflow', '');
}

function showStudentMessageModal(title, message) {
    $('#studentMessageModalTitle').text(title || 'Message');
    $('#studentMessageModalText').text(message || '');

    $('#studentMessageModalBackdrop').show();
    $('#studentMessageModal').show();

    $('body').css('overflow', 'hidden');
}

function closeStudentMessageModal() {
    $('#studentMessageModal').hide();
    $('#studentMessageModalBackdrop').hide();

    $('body').css('overflow', '');
}

function submitStudentAssignmentSolution() {
    var assignmentId = $('#studentSubmitAssignmentId').val();
    var fileInput = document.getElementById('studentAssignmentSolutionFile');
    var file = fileInput && fileInput.files && fileInput.files.length ? fileInput.files[0] : null;
    var solutionLink = ($('#studentAssignmentSolutionLink').val() || '').trim();

    hideStudentAssignmentSubmitMessage();

    if (!assignmentId) {
        showStudentAssignmentSubmitMessage('Invalid assignment selected.');
        return;
    }

    if (!file && !solutionLink) {
        showStudentAssignmentSubmitMessage('Please upload a file or paste a solution link.');
        return;
    }

    if (file && solutionLink) {
        showStudentAssignmentSubmitMessage('Please submit either a file or a link, not both.');
        return;
    }

    if (file) {
        var fileMessage = validateStudentAssignmentFile(file);

        if (fileMessage) {
            showStudentAssignmentSubmitMessage(fileMessage);
            return;
        }
    }

    if (solutionLink && !isValidStudentHttpUrl(solutionLink)) {
        showStudentAssignmentSubmitMessage('Please enter a valid http or https link.');
        return;
    }

    var formData = new FormData();

    if (file) {
        formData.append('file', file);
    }

    if (solutionLink) {
        formData.append('solutionLink', solutionLink);
    }

    var button = $('#studentAssignmentSubmitBtn');
    button.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Submitting...');

    $.ajax({
        url: '/api/student/course-content/assignments/' + assignmentId + '/submit',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (result) {
            closeStudentAssignmentSubmitModal();

            var courseId = getStudentCourseIdFromUrl();
            loadStudentCourseContentMaterialsAssignments(courseId);

            showStudentMessageModal(
                'Submitted',
                result?.message || 'Assignment solution submitted successfully.'
            );
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Assignment solution could not be submitted.'
            );

            showStudentAssignmentSubmitMessage(message);
        },
        complete: function () {
            button.prop('disabled', false).html('<i class="bi bi-upload"></i> Submit');
        }
    });
}

function unsubmitStudentAssignmentSolution(assignmentId) {
    var button = $('#studentAssignmentUnsubmitConfirmBtn');
    button.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Removing...');

    $.ajax({
        url: '/api/student/course-content/assignments/' + assignmentId + '/unsubmit',
        type: 'POST',
        success: function (result) {
            closeStudentAssignmentUnsubmitModal();

            var courseId = getStudentCourseIdFromUrl();
            loadStudentCourseContentMaterialsAssignments(courseId);

            showStudentMessageModal(
                'Removed',
                result?.message || 'Assignment solution removed successfully.'
            );
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Assignment solution could not be removed.'
            );

            closeStudentAssignmentUnsubmitModal();
            showStudentMessageModal('Error', message);
        },
        complete: function () {
            button.prop('disabled', false).html('<i class="bi bi-x-circle"></i> Unsubmit');
        }
    });
}

function validateStudentAssignmentFile(file) {
    var maxSize = 50 * 1024 * 1024;
    var extension = getStudentFileExtension(file.name);
    var allowed = [
        'pdf',
        'doc',
        'docx',
        'ppt',
        'pptx',
        'xls',
        'xlsx',
        'jpg',
        'jpeg',
        'png',
        'zip',
        'rar',
        'mp4',
        'webm'
    ];

    if (file.size > maxSize) {
        return 'File size limit exceeded. Please upload your file/video to Google Drive, OneDrive, or any cloud storage and paste the link here.';
    }

    if (!extension || allowed.indexOf(extension) === -1) {
        return 'This file type is not allowed. Please upload PDF, Word, PPT, Excel, image, ZIP/RAR, MP4, WEBM, or paste a valid link.';
    }

    return '';
}

function showStudentAssignmentSubmitMessage(message) {
    $('#studentAssignmentSubmitMessage')
        .show()
        .text(message);
}

function hideStudentAssignmentSubmitMessage() {
    $('#studentAssignmentSubmitMessage')
        .hide()
        .text('');
}

function isValidStudentHttpUrl(value) {
    try {
        var url = new URL(value);
        return url.protocol === 'http:' || url.protocol === 'https:';
    } catch (e) {
        return false;
    }
}

/* =========================
   NOTES TAB
========================= */

function loadStudentCourseContentNotes(courseId) {
    $('#studentNotesContainer').html(
        '<div class="dashboard-panel text-center p-4">Loading notes...</div>'
    );

    $.ajax({
        url: '/api/student/course-content/' + courseId + '/notes',
        type: 'GET',
        success: function (data) {
            studentCourseNotesCache = data.notes || [];
            renderStudentCourseContentNotes(data);
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Notes could not be loaded.'
            );

            $('#studentNotesContainer').html(
                '<div class="dashboard-panel text-center p-4 text-danger">' +
                escapeHtml(message) +
                '</div>'
            );
        }
    });
}

function renderStudentCourseContentNotes(data) {
    var notes = data.notes || [];

    $('#studentNotesContainer').html(`
        <div class="student-tab-stack">
            <div class="dashboard-stats">
                <div class="student-summary-card">
                    <div class="dashboard-stat-title">Total Notes</div>
                    <div class="dashboard-stat-value">${data.totalNotes || 0}</div>
                    <div class="dashboard-stat-meta">Saved for this course</div>
                </div>

                <div class="student-summary-card">
                    <div class="dashboard-stat-title">Course Notes</div>
                    <div class="dashboard-stat-value">${data.totalNotes || 0}</div>
                    <div class="dashboard-stat-meta">Course-level personal notes</div>
                </div>

                <div class="student-summary-card">
                    <div class="dashboard-stat-title">Recent Activity</div>
                    <div class="dashboard-stat-value">${escapeHtml(data.recentActivityText || 'No activity yet')}</div>
                    <div class="dashboard-stat-meta">Last note activity</div>
                </div>

                <div class="student-summary-card">
                    <div class="dashboard-stat-title">Last Updated</div>
                    <div class="dashboard-stat-value">${escapeHtml(data.lastUpdatedText || 'No notes yet')}</div>
                    <div class="dashboard-stat-meta">Latest note update</div>
                </div>
            </div>

            <div class="dashboard-panel">
                <div class="dashboard-panel-head student-panel-head">
                    <div>
                        <h3 class="page-h2">My Notes</h3>
                        <p class="student-panel-sub">Save your personal notes while studying this course.</p>
                    </div>

                    <div class="student-notes-actions">
                        <div class="dashboard-search student-notes-searchbar">
                            <i class="bi bi-search"></i>
                            <input type="text" id="studentNotesSearchInput" placeholder="Search notes..." />
                        </div>

                        <button type="button"
                                class="dashboard-btn dashboard-btn-outline student-add-note-btn"
                                id="openNoteFormBtn">
                            <i class="bi bi-plus-circle"></i>
                            <span>Add Note</span>
                        </button>
                    </div>
                </div>

                <div class="student-note-list" id="studentNoteList">
                    ${renderStudentCourseNotesList(notes)}
                </div>
            </div>
        </div>
    `);
}

function renderStudentCourseNotesList(notes) {
    if (!notes || !notes.length) {
        return `
            <div class="student-note-card">
                <div class="student-note-content">
                    No notes saved yet. Click Add Note to create your first course note.
                </div>
            </div>
        `;
    }

    return notes.map(function (note) {
        var updatedText = note.updatedAt
            ? 'Updated: ' + formatStudentCourseDateTime(note.updatedAt)
            : 'Created: ' + formatStudentCourseDateTime(note.createdAt);

        return `
            <div class="student-note-card"
                 data-note-id="${note.id}"
                 data-title="${escapeAttribute(note.title)}"
                 data-description="${escapeAttribute(note.description)}">
                <div class="student-note-card-top">
                    <div class="student-note-card-left">
                        <div class="student-note-title-row">
                            <h4 class="student-note-title">${escapeHtml(note.title)}</h4>
                        </div>

                        <div class="student-note-meta">
                            <span>Course Note</span>
                            <span>•</span>
                            <span>${escapeHtml(updatedText)}</span>
                        </div>
                    </div>

                    <div class="student-note-card-right">
                        <button type="button"
                                class="student-note-action-btn js-edit-note-btn"
                                data-note-id="${note.id}">
                            <i class="bi bi-pencil-square"></i> Edit
                        </button>

                        <button type="button"
                                class="student-note-action-btn js-open-delete-note"
                                data-note-id="${note.id}"
                                data-note-title="${escapeAttribute(note.title)}">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                    </div>
                </div>

                <div class="student-note-content">
                    ${escapeHtml(note.description)}
                </div>

                <div class="student-note-footer">
                    <span class="student-note-tag">Course Note</span>
                </div>
            </div>
        `;
    }).join('');
}

function ensureStudentNoteModal() {
    $('#studentNoteModal').remove();

    $('body').append(`
        <div id="studentNoteModal" class="student-note-modal" style="display:none;">
            <div class="student-note-modal-backdrop js-close-note-modal"></div>

            <div class="student-note-modal-dialog">
                <div class="student-note-form-head">
                    <h3 class="student-note-form-title" id="studentNoteModalTitle">Add Note</h3>

                    <button type="button" class="student-note-form-close js-close-note-modal">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>

                <form id="studentNoteForm" class="student-note-form">
                    <input type="hidden" id="studentNoteId" />

                    <div class="student-note-field">
                        <label class="course-label">Note Title</label>
                        <input type="text"
                               id="studentNoteTitle"
                               class="course-input"
                               maxlength="200"
                               placeholder="Enter note title" />
                    </div>

                    <div class="student-note-field">
                        <label class="course-label">Your Note</label>
                        <textarea id="studentNoteDescription"
                                  class="course-input student-note-textarea"
                                  placeholder="Write your note here..."></textarea>
                    </div>

                    <div id="studentNoteFormMessage"
                         class="student-quiz-message error"
                         style="display:none;"></div>

                    <div class="student-note-form-actions">
                        <button type="button"
                                class="dashboard-btn dashboard-btn-outline js-close-note-modal">
                            Cancel
                        </button>

                        <button type="submit"
                                class="dashboard-btn dashboard-btn-primary add-course-btn"
                                id="studentNoteSaveBtn">
                            <i class="bi bi-save"></i> Save Note
                        </button>
                    </div>
                </form>
            </div>
        </div>
    `);
}

function ensureStudentNoteDeleteModal() {
    $('#studentNoteDeleteModal').remove();

    $('body').append(`
        <div id="studentNoteDeleteModal" class="student-note-modal" style="display:none;">
            <div class="student-note-modal-backdrop js-close-delete-note-modal"></div>

            <div class="student-note-modal-dialog">
                <input type="hidden" id="studentDeleteNoteId" />

                <div class="student-note-form-head">
                    <h3 class="student-note-form-title">Delete Note</h3>

                    <button type="button" class="student-note-form-close js-close-delete-note-modal">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>

                <p class="student-panel-sub" id="studentDeleteNoteText">
                    Are you sure you want to delete this note?
                </p>

                <div class="student-note-form-actions">
                    <button type="button"
                            class="dashboard-btn dashboard-btn-outline js-close-delete-note-modal">
                        Cancel
                    </button>

                    <button type="button"
                            class="dashboard-btn dashboard-btn-primary add-course-btn"
                            id="studentConfirmDeleteNoteBtn">
                        <i class="bi bi-trash"></i> Delete
                    </button>
                </div>
            </div>
        </div>
    `);
}

function bindStudentCourseNotesEvents() {
    $(document).off('click', '#openNoteFormBtn').on('click', '#openNoteFormBtn', function () {
        openStudentNoteModalForCreate();
    });

    $(document).off('click', '.js-close-note-modal').on('click', '.js-close-note-modal', function () {
        closeStudentNoteModal();
    });

    $(document).off('submit', '#studentNoteForm').on('submit', '#studentNoteForm', function (e) {
        e.preventDefault();
        saveStudentCourseNote();
    });

    $(document).off('click', '.js-edit-note-btn').on('click', '.js-edit-note-btn', function () {
        var noteId = parseInt($(this).data('note-id'));
        openStudentNoteModalForEdit(noteId);
    });

    $(document).off('click', '.js-open-delete-note').on('click', '.js-open-delete-note', function () {
        var noteId = parseInt($(this).data('note-id'));
        var title = $(this).data('note-title') || 'this note';

        openStudentNoteDeleteModal(noteId, title);
    });

    $(document).off('click', '.js-close-delete-note-modal').on('click', '.js-close-delete-note-modal', function () {
        closeStudentNoteDeleteModal();
    });

    $(document).off('click', '#studentConfirmDeleteNoteBtn').on('click', '#studentConfirmDeleteNoteBtn', function () {
        var noteId = parseInt($('#studentDeleteNoteId').val());

        if (!noteId) {
            return;
        }

        deleteStudentCourseNote(noteId);
    });

    $(document).off('input', '#studentNotesSearchInput').on('input', '#studentNotesSearchInput', function () {
        var keyword = ($(this).val() || '').toLowerCase().trim();

        var filteredNotes = studentCourseNotesCache.filter(function (note) {
            return String(note.title || '').toLowerCase().includes(keyword) ||
                String(note.description || '').toLowerCase().includes(keyword);
        });

        $('#studentNoteList').html(renderStudentCourseNotesList(filteredNotes));
    });
}

function openStudentNoteModalForCreate() {
    $('#studentNoteModalTitle').text('Add Note');
    $('#studentNoteId').val('');
    $('#studentNoteTitle').val('');
    $('#studentNoteDescription').val('');
    hideStudentNoteFormMessage();

    $('#studentNoteModal').show();
    $('body').css('overflow', 'hidden');
}

function openStudentNoteModalForEdit(noteId) {
    var note = studentCourseNotesCache.find(function (item) {
        return item.id === noteId;
    });

    if (!note) {
        showStudentMessageModal('Error', 'Note was not found.');
        return;
    }

    $('#studentNoteModalTitle').text('Edit Note');
    $('#studentNoteId').val(note.id);
    $('#studentNoteTitle').val(note.title || '');
    $('#studentNoteDescription').val(note.description || '');
    hideStudentNoteFormMessage();

    $('#studentNoteModal').show();
    $('body').css('overflow', 'hidden');
}

function closeStudentNoteModal() {
    $('#studentNoteModal').hide();
    $('body').css('overflow', '');
}

function openStudentNoteDeleteModal(noteId, title) {
    $('#studentDeleteNoteId').val(noteId);
    $('#studentDeleteNoteText').text('Are you sure you want to delete "' + title + '"?');

    $('#studentNoteDeleteModal').show();
    $('body').css('overflow', 'hidden');
}

function closeStudentNoteDeleteModal() {
    $('#studentNoteDeleteModal').hide();
    $('body').css('overflow', '');
}

function saveStudentCourseNote() {
    var courseId = getStudentCourseIdFromUrl();
    var noteId = $('#studentNoteId').val();
    var title = ($('#studentNoteTitle').val() || '').trim();
    var description = ($('#studentNoteDescription').val() || '').trim();

    hideStudentNoteFormMessage();

    if (!title) {
        showStudentNoteFormMessage('Please enter note title.');
        return;
    }

    if (title.length > 200) {
        showStudentNoteFormMessage('Note title cannot be more than 200 characters.');
        return;
    }

    if (!description) {
        showStudentNoteFormMessage('Please write your note.');
        return;
    }

    var button = $('#studentNoteSaveBtn');
    button.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Saving...');

    var isEdit = noteId && parseInt(noteId) > 0;
    var url = isEdit
        ? '/api/student/course-content/notes/' + noteId
        : '/api/student/course-content/' + courseId + '/notes';

    var method = isEdit ? 'PUT' : 'POST';

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify({
            title: title,
            description: description
        }),
        success: function (result) {
            closeStudentNoteModal();
            loadStudentCourseContentNotes(courseId);

            showStudentMessageModal(
                isEdit ? 'Updated' : 'Saved',
                result?.message || (isEdit ? 'Note updated successfully.' : 'Note saved successfully.')
            );
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Note could not be saved.'
            );

            showStudentNoteFormMessage(message);
        },
        complete: function () {
            button.prop('disabled', false).html('<i class="bi bi-save"></i> Save Note');
        }
    });
}

function deleteStudentCourseNote(noteId) {
    var courseId = getStudentCourseIdFromUrl();
    var button = $('#studentConfirmDeleteNoteBtn');

    button.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Deleting...');

    $.ajax({
        url: '/api/student/course-content/notes/' + noteId,
        type: 'DELETE',
        success: function (result) {
            closeStudentNoteDeleteModal();
            loadStudentCourseContentNotes(courseId);

            showStudentMessageModal(
                'Deleted',
                result?.message || 'Note deleted successfully.'
            );
        },
        error: function (xhr) {
            var message = getStudentCourseContentErrorMessage(
                xhr,
                'Note could not be deleted.'
            );

            closeStudentNoteDeleteModal();
            showStudentMessageModal('Error', message);
        },
        complete: function () {
            button.prop('disabled', false).html('<i class="bi bi-trash"></i> Delete');
        }
    });
}

function showStudentNoteFormMessage(message) {
    $('#studentNoteFormMessage')
        .show()
        .text(message);
}

function hideStudentNoteFormMessage() {
    $('#studentNoteFormMessage')
        .hide()
        .text('');
}

/* =========================
   TABS
========================= */

function bindStudentCourseContentTabs() {
    $(document).off('click', '.student-course-tab').on('click', '.student-course-tab', function () {
        var target = $(this).data('tab');

        $('.student-course-tab').removeClass('active');
        $('.student-course-tab-panel').removeClass('active');

        $(this).addClass('active');
        $('#tab-' + target).addClass('active');
    });
}

/* =========================
   MATERIAL HELPERS
========================= */

function getMaterialExtra(m) {
    var parts = [];

    if (m.pages) {
        parts.push(m.pages + ' Pages');
    }

    if (m.slides) {
        parts.push(m.slides + ' Slides');
    }

    if (m.minutes) {
        parts.push(m.minutes + ' Min');
    }

    if (!parts.length) {
        return '';
    }

    return '<span>•</span><span>' + escapeHtml(parts.join(' • ')) + '</span>';
}

function getMaterialClass(type, path) {
    var value = String(type || '').toLowerCase();
    var ext = getStudentFileExtension(path);

    if (value.includes('pdf') || ext === 'pdf') {
        return 'pdf';
    }

    if (value.includes('ppt') || value.includes('slide') || ext === 'ppt' || ext === 'pptx') {
        return 'ppt';
    }

    if (value.includes('video') || ['mp4', 'mov', 'avi', 'mkv', 'webm'].includes(ext)) {
        return 'video';
    }

    return 'pdf';
}

function getMaterialIcon(type, path) {
    var value = String(type || '').toLowerCase();
    var ext = getStudentFileExtension(path);

    if (value.includes('pdf') || ext === 'pdf') {
        return 'bi-file-earmark-pdf';
    }

    if (value.includes('ppt') || value.includes('slide') || ext === 'ppt' || ext === 'pptx') {
        return 'bi-file-earmark-slides';
    }

    if (value.includes('video') || ['mp4', 'mov', 'avi', 'mkv', 'webm'].includes(ext)) {
        return 'bi-play-btn';
    }

    if (value.includes('image') || ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg'].includes(ext)) {
        return 'bi-file-earmark-image';
    }

    if (['doc', 'docx'].includes(ext)) {
        return 'bi-file-earmark-word';
    }

    if (['xls', 'xlsx', 'csv'].includes(ext)) {
        return 'bi-file-earmark-excel';
    }

    return 'bi-file-earmark-text';
}

function getStudentFileExtension(path) {
    if (!path) {
        return '';
    }

    var cleanPath = String(path).split('?')[0].split('#')[0];
    var parts = cleanPath.split('.');

    if (parts.length < 2) {
        return '';
    }

    return parts[parts.length - 1].toLowerCase();
}

function isStudentFileViewable(path, type) {
    var value = String(type || '').toLowerCase();
    var ext = getStudentFileExtension(path);

    if (value.includes('pdf') || ext === 'pdf') {
        return true;
    }

    if (value.includes('image') || ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg'].includes(ext)) {
        return true;
    }

    if (value.includes('video') || ['mp4', 'webm'].includes(ext)) {
        return true;
    }

    return false;
}

/* =========================
   HELPERS
========================= */

function showStudentCourseContentError(message) {
    $('#studentCourseDetailLoader').html(
        '<div class="text-center p-4 text-danger">' +
        escapeHtml(message) +
        '</div>'
    );

    $('#studentCourseDetailLoader').show();
    $('#studentCourseDetailContent').hide();
}

function getStudentCourseContentErrorMessage(xhr, fallbackMessage) {
    var err = xhr.responseJSON;

    return err?.errorMessage ||
        err?.message ||
        err?.title ||
        fallbackMessage;
}

function formatStudentCourseDate(value) {
    var date = parseStudentServerDate(value);

    if (!date) {
        return 'N/A';
    }

    return date.toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric'
    });
}

function formatStudentCourseDateTime(value) {
    var date = parseStudentServerDate(value);

    if (!date) {
        return 'N/A';
    }

    return date.toLocaleString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatStudentAssignmentDateTime(value) {
    var date = parseStudentLocalDate(value);

    if (!date) {
        return 'N/A';
    }

    return date.toLocaleString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function parseStudentServerDate(value) {
    if (!value) {
        return null;
    }

    var text = String(value);

    if (
        !text.endsWith('Z') &&
        !text.includes('+') &&
        !text.match(/-\d{2}:\d{2}$/)
    ) {
        text += 'Z';
    }

    var date = new Date(text);

    if (isNaN(date.getTime())) {
        return null;
    }

    return date;
}

function parseStudentLocalDate(value) {
    if (!value) {
        return null;
    }

    var date = new Date(String(value));

    if (isNaN(date.getTime())) {
        return null;
    }

    return date;
}

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return '';
    }

    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function escapeAttribute(value) {
    return escapeHtml(value).replace(/`/g, '&#096;');
}