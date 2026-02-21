// Global site functions

// Show loading spinner
function showLoading() {
    $('.spinner-overlay').css('display', 'flex');
}

// Hide loading spinner
function hideLoading() {
    $('.spinner-overlay').fadeOut();
}

// Auto-dismiss alerts after 5 seconds
$(document).ready(function() {
    setTimeout(function() {
        $('.alert').fadeOut('slow');
    }, 5000);
});

// Format currency
function formatCurrency(amount) {
    return 'Rs. ' + parseFloat(amount).toFixed(2);
}

// Confirm action
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function() {
        alert('Copied to clipboard!');
    });
}

// Toggle password visibility
function togglePassword(inputId, iconId) {
    var input = $('#' + inputId);
    var icon = $('#' + iconId);
    
    if (input.attr('type') === 'password') {
        input.attr('type', 'text');
        icon.removeClass('fa-eye').addClass('fa-eye-slash');
    } else {
        input.attr('type', 'password');
        icon.removeClass('fa-eye-slash').addClass('fa-eye');
    }
}