$(document).ready(function () {
    initStudentCourseContentPage();
});

function initStudentCourseContentPage() {
    bindStudentCourseContentTabs();

    var courseId = getStudentCourseIdFromUrl();

    if (!courseId) {
        showStudentCourseContentError('Invalid course selected.');
        return;
    }

    loadStudentCourseContentHeader(courseId);
    loadStudentCourseContentInfo(courseId);
    loadStudentCourseContentModules(courseId);
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
            },
            error: function (xhr) {
                var message = getStudentCourseContentErrorMessage(
                    xhr,
                    'Lesson could not be marked as completed.'
                );

                alert(message);
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
    var summaryText = review.isPassed
        ? 'Passed'
        : 'Failed';

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
function parseStudentServerDate(value) {
    if (!value) {
        return null;
    }

    var text = String(value);

    // If backend sends UTC DateTime without Z, treat it as UTC.
    // Example: 2026-06-16T20:28:00 -> 2026-06-16T20:28:00Z
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