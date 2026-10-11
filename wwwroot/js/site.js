// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Loading Page

    document.querySelector("form").addEventListener("submit", function () {
        const button = document.getElementById("signupButton");
        button.disabled = true;
        button.textContent = "Otp is on the way";
    });
