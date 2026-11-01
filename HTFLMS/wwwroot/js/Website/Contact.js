(function () {
    'use strict';

    var form = document.getElementById('htfContactForm');
    if (!form) return;

    var msg = document.getElementById('htfMessage');
    var btn = form.querySelector('.htf-contact__send');

    // Message box type karne par khud barh jaye
    function grow() {
        if (!msg) return;
        msg.style.height = 'auto';
        msg.style.height = msg.scrollHeight + 'px';
    }
    if (msg) {
        msg.addEventListener('input', grow);
        grow();
    }

    // Double submit se bachne ke liye
    form.addEventListener('submit', function () {
        if (!form.checkValidity()) return;
        if (btn) {
            btn.disabled = true;
            btn.textContent = 'Sending...';
        }
    });
})();