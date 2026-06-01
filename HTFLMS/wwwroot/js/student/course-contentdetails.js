document.addEventListener('DOMContentLoaded', function () {
    initStudentCourseVideos();
    initStudentNotes();
    initStudentCourseTabsAndModules();
});

function initStudentCourseVideos() {
    const videoToggleButtons = document.querySelectorAll('.js-video-toggle');

    videoToggleButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            const card = this.closest('.student-material-card');

            if (!card) {
                return;
            }

            card.classList.toggle('video-open');

            const isOpen = card.classList.contains('video-open');

            this.innerHTML = isOpen
                ? '<i class="bi bi-x-circle"></i> Close Video'
                : '<i class="bi bi-play-circle"></i> Watch Video';
        });
    });
}

function initStudentNotes() {
    const openNoteFormBtn = document.getElementById('openNoteFormBtn');
    const closeNoteFormBtn = document.getElementById('closeNoteFormBtn');
    const cancelNoteFormBtn = document.getElementById('cancelNoteFormBtn');
    const studentNoteModal = document.getElementById('studentNoteModal');
    const studentNoteModalBackdrop = document.getElementById('studentNoteModalBackdrop');
    const studentNoteForm = document.getElementById('studentNoteForm');
    const editButtons = document.querySelectorAll('.js-edit-note-btn');

    const modalTitle = document.getElementById('studentNoteModalTitle');
    const submitBtn = document.getElementById('studentNoteSubmitBtn');
    const editingNoteIndex = document.getElementById('editingNoteIndex');

    const noteModule = document.getElementById('noteModule');
    const noteLesson = document.getElementById('noteLesson');
    const noteTitle = document.getElementById('noteTitle');
    const noteContent = document.getElementById('noteContent');
    const notePinned = document.getElementById('notePinned');

    let currentEditingCard = null;

    function openModal() {
        if (studentNoteModal) {
            studentNoteModal.style.display = 'block';
            document.body.style.overflow = 'hidden';
        }
    }

    function closeModal() {
        if (studentNoteModal) {
            studentNoteModal.style.display = 'none';
            document.body.style.overflow = '';
        }
    }

    function resetFormForAdd() {
        currentEditingCard = null;

        if (modalTitle) {
            modalTitle.textContent = 'Add New Note';
        }

        if (submitBtn) {
            submitBtn.innerHTML = '<i class="bi bi-floppy"></i> Save Note';
        }

        if (editingNoteIndex) {
            editingNoteIndex.value = '';
        }

        if (noteModule) {
            noteModule.value = '';
        }

        if (noteLesson) {
            noteLesson.value = '';
        }

        if (noteTitle) {
            noteTitle.value = '';
        }

        if (noteContent) {
            noteContent.value = '';
        }

        if (notePinned) {
            notePinned.checked = false;
        }
    }

    function fillFormForEdit(card) {
        if (!card) {
            return;
        }

        currentEditingCard = card;

        if (modalTitle) {
            modalTitle.textContent = 'Edit Note';
        }

        if (submitBtn) {
            submitBtn.innerHTML = '<i class="bi bi-check2"></i> Update Note';
        }

        if (noteModule) {
            noteModule.value = card.getAttribute('data-module') || '';
        }

        if (noteLesson) {
            noteLesson.value = card.getAttribute('data-lesson') || '';
        }

        if (noteTitle) {
            noteTitle.value = card.getAttribute('data-title') || '';
        }

        if (noteContent) {
            noteContent.value = card.getAttribute('data-note') || '';
        }

        if (notePinned) {
            notePinned.checked = card.getAttribute('data-pinned') === 'true';
        }
    }

    function updateNoteCard(card) {
        if (!card || !noteTitle || !noteContent || !noteModule || !noteLesson || !notePinned) {
            return;
        }

        const titleText = noteTitle.value.trim();
        const contentText = noteContent.value.trim();
        const moduleText = noteModule.value;
        const lessonText = noteLesson.value;
        const isPinned = notePinned.checked;

        const titleEl = card.querySelector('.student-note-title');
        const contentEl = card.querySelector('.student-note-content');
        const metaEl = card.querySelector('.student-note-meta');
        const titleRow = card.querySelector('.student-note-title-row');
        let pinnedBadge = card.querySelector('.student-note-badge.pinned');

        if (titleEl) {
            titleEl.textContent = titleText;
        }

        if (contentEl) {
            contentEl.textContent = contentText;
        }

        if (metaEl) {
            metaEl.innerHTML = `
                <span>${escapeHtml(moduleText)}</span>
                <span>•</span>
                <span>Lesson: ${escapeHtml(lessonText)}</span>
                <span>•</span>
                <span>Updated: Just now</span>
            `;
        }

        card.setAttribute('data-module', moduleText);
        card.setAttribute('data-lesson', lessonText);
        card.setAttribute('data-title', titleText);
        card.setAttribute('data-note', contentText);
        card.setAttribute('data-pinned', isPinned ? 'true' : 'false');

        if (isPinned) {
            card.classList.add('pinned');

            if (!pinnedBadge && titleRow) {
                pinnedBadge = document.createElement('span');
                pinnedBadge.className = 'student-note-badge pinned';
                pinnedBadge.innerHTML = '<i class="bi bi-pin-angle-fill"></i> Pinned';
                titleRow.appendChild(pinnedBadge);
            }
        } else {
            card.classList.remove('pinned');

            if (pinnedBadge) {
                pinnedBadge.remove();
            }
        }
    }

    if (openNoteFormBtn) {
        openNoteFormBtn.addEventListener('click', function () {
            resetFormForAdd();
            openModal();
        });
    }

    editButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            const card = this.closest('.student-note-card');
            fillFormForEdit(card);
            openModal();
        });
    });

    if (closeNoteFormBtn) {
        closeNoteFormBtn.addEventListener('click', closeModal);
    }

    if (cancelNoteFormBtn) {
        cancelNoteFormBtn.addEventListener('click', closeModal);
    }

    if (studentNoteModalBackdrop) {
        studentNoteModalBackdrop.addEventListener('click', closeModal);
    }

    if (studentNoteForm) {
        studentNoteForm.addEventListener('submit', function (e) {
            e.preventDefault();

            if (currentEditingCard) {
                updateNoteCard(currentEditingCard);
            }

            closeModal();
        });
    }
}

function initStudentCourseTabsAndModules() {
    const tabs = document.querySelectorAll('.student-course-tab');
    const panels = document.querySelectorAll('.student-course-tab-panel');

    function openStudentCourseTab(tabName) {
        if (!tabName) {
            return;
        }

        tabs.forEach(function (tab) {
            tab.classList.remove('active');
        });

        panels.forEach(function (panel) {
            panel.classList.remove('active');
        });

        const selectedTab = document.querySelector(`.student-course-tab[data-tab="${tabName}"]`);
        const selectedPanel = document.getElementById('tab-' + tabName);

        if (selectedTab) {
            selectedTab.classList.add('active');
        }

        if (selectedPanel) {
            selectedPanel.classList.add('active');
        }
    }

    function scrollToAssignmentFromUrl() {
        const params = new URLSearchParams(window.location.search);
        const tabName = params.get('tab');
        const assignmentId = params.get('assignmentId');

        if (tabName) {
            openStudentCourseTab(tabName);
        }

        if (!assignmentId) {
            return;
        }

        setTimeout(function () {
            const assignmentCard = document.querySelector(`[data-assignment-id="${assignmentId}"]`);

            if (!assignmentCard) {
                return;
            }

            assignmentCard.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });

            assignmentCard.classList.add('student-assignment-highlight');

            setTimeout(function () {
                assignmentCard.classList.remove('student-assignment-highlight');
            }, 3500);
        }, 300);
    }

    tabs.forEach(function (tab) {
        tab.addEventListener('click', function () {
            const target = this.getAttribute('data-tab');
            openStudentCourseTab(target);
        });
    });

    initStudentModules();

    scrollToAssignmentFromUrl();
}

function initStudentModules() {
    const modules = document.querySelectorAll('.js-module');

    modules.forEach(function (module) {
        const head = module.querySelector('.student-module-head');
        const lessons = module.querySelectorAll('.js-lesson');
        const total = parseInt(module.getAttribute('data-total')) || lessons.length;

        const countEl = module.querySelector('.js-module-count');
        const percentEl = module.querySelector('.js-module-percent');
        const fillEl = module.querySelector('.js-module-fill');
        const statusEl = module.querySelector('.js-module-status');
        const iconEl = module.querySelector('.js-module-icon');

        function markLessonDone(lesson) {
            const doneBtn = lesson.querySelector('.js-done-btn');
            const title = lesson.querySelector('.student-lesson-title');
            const icon = lesson.querySelector('.student-lesson-type-icon');

            lesson.classList.add('done');

            if (doneBtn) {
                doneBtn.textContent = 'Completed';
                doneBtn.classList.add('done');
                doneBtn.disabled = true;
            }

            if (title) {
                title.classList.add('done-text');
            }

            if (icon) {
                icon.classList.remove('muted');
            }

            updateModuleState();
        }

        function updateModuleState() {
            const doneCount = module.querySelectorAll('.js-lesson.done').length;
            const percent = total > 0 ? Math.round((doneCount / total) * 100) : 0;

            if (countEl) {
                countEl.textContent = `${doneCount}/${total} lessons completed`;
            }

            if (percentEl) {
                percentEl.textContent = `${percent}%`;
            }

            if (fillEl) {
                fillEl.style.width = `${percent}%`;
            }

            if (statusEl) {
                statusEl.classList.remove('dark', 'light', 'muted');
            }

            if (iconEl) {
                iconEl.classList.remove('success', 'primary');
            }

            if (doneCount === 0) {
                if (statusEl) {
                    statusEl.textContent = 'Not Started';
                    statusEl.classList.add('muted');
                }

                if (iconEl) {
                    iconEl.classList.add('primary');
                    iconEl.innerHTML = '<i class="bi bi-book"></i>';
                }
            } else if (doneCount < total) {
                if (statusEl) {
                    statusEl.textContent = 'In Progress';
                    statusEl.classList.add('light');
                }

                if (iconEl) {
                    iconEl.classList.add('primary');
                    iconEl.innerHTML = '<i class="bi bi-book"></i>';
                }
            } else {
                if (statusEl) {
                    statusEl.textContent = 'Completed';
                    statusEl.classList.add('dark');
                }

                if (iconEl) {
                    iconEl.classList.add('success');
                    iconEl.innerHTML = '<i class="bi bi-check-lg"></i>';
                }
            }
        }

        if (head && !module.classList.contains('locked')) {
            head.addEventListener('click', function () {
                module.classList.toggle('expanded');
            });
        }

        lessons.forEach(function (lesson) {
            const toggleBtn = lesson.querySelector('.js-lesson-toggle');
            const doneBtn = lesson.querySelector('.js-done-btn');
            const isQuizLesson = lesson.classList.contains('js-quiz-lesson');

            if (toggleBtn) {
                toggleBtn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    lesson.classList.toggle('open');
                });
            }

            if (doneBtn && !isQuizLesson) {
                doneBtn.addEventListener('click', function (e) {
                    e.stopPropagation();

                    if (lesson.classList.contains('done')) {
                        return;
                    }

                    markLessonDone(lesson);
                });
            }

            if (isQuizLesson) {
                initStudentQuizLesson(lesson, markLessonDone);
            }
        });

        updateModuleState();
    });
}

function initStudentQuizLesson(lesson, markLessonDone) {
    const submitQuizBtn = lesson.querySelector('.js-submit-quiz');
    const resultEl = lesson.querySelector('.js-quiz-result');
    const attemptsLeftEl = lesson.querySelector('.js-attempts-left');
    const questionEls = lesson.querySelectorAll('.student-quiz-question');
    const maxAttempts = parseInt(lesson.getAttribute('data-max-attempts')) || 3;

    let attemptsUsed = 0;
    let passed = false;

    if (!submitQuizBtn) {
        return;
    }

    submitQuizBtn.addEventListener('click', function (e) {
        e.stopPropagation();

        if (passed || attemptsUsed >= maxAttempts) {
            return;
        }

        let allAnswered = true;
        let allCorrect = true;

        questionEls.forEach(function (question) {
            question.querySelectorAll('label').forEach(function (label) {
                label.classList.remove('quiz-wrong');
            });

            const correctAnswer = question.getAttribute('data-correct');
            const selected = question.querySelector('input[type="radio"]:checked');

            if (!selected) {
                allAnswered = false;
                allCorrect = false;
                return;
            }

            if (selected.value !== correctAnswer) {
                allCorrect = false;

                const selectedLabel = selected.closest('label');

                if (selectedLabel) {
                    selectedLabel.classList.add('quiz-wrong');
                }
            }
        });

        if (!resultEl) {
            return;
        }

        if (!allAnswered) {
            resultEl.textContent = 'Please answer all questions first.';
            resultEl.className = 'student-quiz-result js-quiz-result error';
            return;
        }

        attemptsUsed++;

        const attemptsLeft = maxAttempts - attemptsUsed;

        if (attemptsLeftEl) {
            attemptsLeftEl.textContent = attemptsLeft;
        }

        if (allCorrect) {
            passed = true;

            resultEl.textContent = 'Great job. You got all answers correct and this quiz is now completed.';
            resultEl.className = 'student-quiz-result js-quiz-result success';

            markLessonDone(lesson);
            submitQuizBtn.disabled = true;
        } else {
            if (attemptsLeft > 0) {
                resultEl.textContent = `Some answers are incorrect. Please try again. Attempts left: ${attemptsLeft}`;
                resultEl.className = 'student-quiz-result js-quiz-result error';
            } else {
                resultEl.textContent = 'You have used all attempts for this quiz.';
                resultEl.className = 'student-quiz-result js-quiz-result error';

                submitQuizBtn.disabled = true;

                const radios = lesson.querySelectorAll('input[type="radio"]');

                radios.forEach(function (radio) {
                    radio.disabled = true;
                });
            }
        }
    });
}

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return '';
    }

    return value
        .toString()
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}