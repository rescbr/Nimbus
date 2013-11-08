﻿/*Este arquivo deve conter apenas os scripts referentes as páginas de profile*/

function SendMessageProfile(receiverId)
{
    ajaxMessage = {};    
    var text = CKEDITOR.instances.txtTextMsg.getData();
    var title = document.getElementById('inpTitleMsg').value;

    if (text != "")
    {
        ajaxMessage["Text"] = text;
        if (title != "")
            ajaxMessage["Title"] = title;
        else
            ajaxMessage["Title"] = "Sem assunto";       

        $.ajax({                        
            url: "/api/Message/SendMessageUser/"+receiverId,
            data: JSON.stringify(ajaxMessage),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                   
                    if (newData.Id > 0)
                    {
                        //fechar modal
                        document.getElementById('closeModal').click();
                        //limpar campos
                        text.value = "";
                        title.value = "";
                        //aviso
                        window.alert("Mensagem enviada com sucesso.");
                    }
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                }
            }
        });
    }
}

function EditProfile() {
}

function HidePopover(namePopover)
{
    $('#'+namePopover).popover('hide');
}

$(function () {
    $('.bubbleInfo').each(function () {
        // options
        var distance = 10;
        var time = 250;
        var hideDelay = 500;

        var hideDelayTimer = null;

        // tracker
        var beingShown = false;
        var shown = false;

        var trigger = $('.trigger', this);
        var popup = $('.popup', this).css('opacity', 0);

        // set the mouseover and mouseout on both element
        $([trigger.get(0), popup.get(0)]).click(function () {
         
            // don't trigger the animation again if we're being shown, or already visible
            if (beingShown || shown) {
                return;
            } else {
                beingShown = true;

                // reset position of popup box
                popup.css({
                    display: 'block' // brings the popup back in to view
                })

                // (we're using chaining on the popup) now animate it's opacity and position
                .animate({
                    top: '-=' + distance + 'px',
                    opacity: 1
                }, time, 'swing', function () {
                    // once the animation is complete, set the tracker variables
                    beingShown = false;
                    shown = true;
                });
            }
        })
    });
});

/*
.mouseout(function () {
            // reset the timer if we get fired again - avoids double animations
            if (hideDelayTimer) clearTimeout(hideDelayTimer);

            // store the timer so that it can be cleared in the mouseover if required
            hideDelayTimer = setTimeout(function () {
                hideDelayTimer = null;
                popup.animate({
                    top: '-=' + distance + 'px',
                    opacity: 0
                }, time, 'swing', function () {
                    // once the animate is complete, set the tracker variables
                    shown = false;
                    // hide the popup entirely after the effect (opacity alone doesn't do the job)
                    popup.css('display', 'none');
                });
            }, hideDelay);
        });
*/