const comments = {
    _attachments: [],

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
            this._attachments = [];
            this.renderAttachmentList();
        }
    },

    initAttachmentUpload: function () {
        const self = this;
        const fileInput = document.getElementById('comment-attachment');
        if (fileInput) {
            fileInput.addEventListener('change', async function (e) {
                const files = e.target.files;
                if (!files.length) return;

                for (let i = 0; i < files.length; i++) {
                    await self.uploadFile(files[i]);
                }
                // Reset file input so the same file can be selected again
                fileInput.value = '';
            });
        }
    },

    uploadFile: async function (file) {
        const formData = new FormData();
        formData.append('files', file);

        try {
            const response = await fetch('/Admin/Media/Upload?path=comments&extensions=.jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                if (result.files && result.files.length > 0) {
                    for (const f of result.files) {
                        this._attachments.push({
                            name: f.name,
                            path: f.path || f.url,
                            size: f.size,
                            type: f.type || file.type
                        });
                    }
                    this.renderAttachmentList();
                }
            } else {
                alert('Failed to upload file: ' + file.name);
            }
        } catch (err) {
            alert('Upload error: ' + err.message);
        }
    },

    removeAttachment: function (index) {
        this._attachments.splice(index, 1);
        this.renderAttachmentList();
    },

    renderAttachmentList: function () {
        const list = document.getElementById('comment-attachment-list');
        if (!list) return;

        list.innerHTML = '';
        for (let i = 0; i < this._attachments.length; i++) {
            const att = this._attachments[i];
            const badge = document.createElement('span');
            badge.className = 'badge bg-light text-dark d-inline-flex align-items-center gap-1';
            badge.innerHTML = '<i class="fas fa-paperclip"></i> ' + this.escapeHtml(att.name) +
                ' <button type="button" class="btn-close btn-close-sm ms-1" onclick="comments.removeAttachment(' + i + ')" aria-label="Remove"></button>';
            list.appendChild(badge);
        }
    },

    escapeHtml: function (text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
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

        const attachmentPaths = this._attachments.map(a => a.path);

        try {
            const response = await fetch('/api/comments/create', {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({
                    commentedOn: commentedOn,
                    repliedOn: repliedOn || null,
                    text: text,
                    attachments: attachmentPaths
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

// Initialize attachment upload when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    comments.initAttachmentUpload();
});
