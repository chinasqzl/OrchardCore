const comments = {
    getAntiForgeryToken: function () {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    },

    getHeaders: function () {
        return {
            'RequestVerificationToken': this.getAntiForgeryToken(),
            'Content-Type': 'application/json'
        };
    },

    toggleForm: function (repliedOn) {
        const form = document.getElementById('comment-form');
        const repliedOnInput = document.getElementById('comment-replied-on');

        if (form.style.display === 'none') {
            form.style.display = 'block';
            if (repliedOn) {
                repliedOnInput.value = repliedOn;
            } else {
                repliedOnInput.value = '';
            }
            document.getElementById('comment-text').focus();
        } else {
            form.style.display = 'none';
            repliedOnInput.value = '';
            document.getElementById('comment-text').value = '';
        }
    },

    submit: async function () {
        const section = document.querySelector('.comments-section');
        const commentedOn = section.dataset.commentedOn;
        const text = document.getElementById('comment-text').value.trim();
        const repliedOn = document.getElementById('comment-replied-on').value;

        if (!text) {
            alert('Please enter a comment.');
            return;
        }

        try {
            const response = await fetch('/api/comments/create', {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({
                    commentedOn: commentedOn,
                    repliedOn: repliedOn || null,
                    text: text,
                    attachments: []
                })
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    window.location.reload();
                }
            } else if (response.status === 403) {
                alert('You are not authorized to comment.');
            } else {
                alert('Failed to submit comment.');
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    },

    reply: function (commentId) {
        this.toggleForm(commentId);
    },

    deleteComment: async function (commentId) {
        if (!confirm('Are you sure you want to delete this comment?')) return;

        try {
            const response = await fetch('/api/comments/' + commentId + '/delete', {
                method: 'POST',
                headers: this.getHeaders()
            });

            if (response.ok) {
                window.location.reload();
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    },

    updateStatus: async function (commentId, status) {
        try {
            const response = await fetch('/api/comments/' + commentId + '/status', {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({ status: status })
            });

            if (response.ok) {
                window.location.reload();
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    },

    edit: async function (commentId) {
        const newText = prompt('Edit your comment:');
        if (!newText) return;

        try {
            const response = await fetch('/api/comments/' + commentId + '/update', {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({ text: newText })
            });

            if (response.ok) {
                window.location.reload();
            }
        } catch (err) {
            alert('Network error: ' + err.message);
        }
    }
};
