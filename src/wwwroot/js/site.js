// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const openInitialForm = () => {
    let _url = new URL(window.location.href);
    if (_url.pathname !== '/') {
        window.location.href = '/';
    }
    $.post("Home/InitialForm")
        .done(result => {
            mdlA.id = "formWin";
            mdlA.title = "Form";
            mdlA.content = result;
            mdlA.modal(mdlA.size.default);
        })
        .fail(xhr => {
            alert("Something went wrong"); console.error(xhr.responseText)
        });
}