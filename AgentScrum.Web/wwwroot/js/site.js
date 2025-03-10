// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Chat functionality
document.addEventListener('DOMContentLoaded', function() {
    // Scroll to bottom of chat on page load
    const chatMessages = document.getElementById('chat-messages');
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    // Focus on input field when page loads
    const messageInput = document.getElementById('NewMessage');
    if (messageInput) {
        messageInput.focus();
    }
    
    // Submit form on Enter key press (unless Shift is held)
    if (messageInput) {
        messageInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.form.submit();
            }
        });
    }
});
