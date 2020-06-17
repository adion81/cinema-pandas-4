// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


// attach validation error styling to any input that is invalid
$(".input-validation-error").each(function(){
    $(this).addClass("is-invalid");
});

// autofocus the first invalid input 
$(".input-validation-error").first().focus();