// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    var placeholderElement = $('#modal-placeholder');
    $('button[data-toggle="ajax-modal"]').click(function (event) {

        // Get Url
        var url = $(this).data('url');

        // Get request to pull the modal and place it in the place holder
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        })
    });

    // attach click event handler to an element
    // which is located inside #modal-placeholder
    // and has data-save attribute equal to modal
    placeholderElement.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();

        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');
        var dataToSend = form.serialize();

        $.post(actionUrl, dataToSend).done(function (data) {
            var newBody = $('.modal-body', data);
            placeholderElement.find('.modal-body').replaceWith(newBody);

            // find IsValid input field and check it's value
            // if it's valid then hide modal window
            var isValid = newBody.find('[name="IsValid"]').val() == 'True';
            if (isValid) {
                placeholderElement.find('.modal').modal('hide');

                // This is not jQuery but simple plain ol' JS
                window.location.reload();
            }
        });
    });

    // With the element initially shown, we can hide it slowly:
    $(".Type").click(function () {
        alert("sdfsdfsdfs");
        var type = $("#Type").val();
        alert(type);
        if (type == "1") {
            $("#MemberName").hide();
            alert(1);
        }
        else if (type == "2") {
            $("#GeneralName").hide();
            alert(2);
        }
    });
});