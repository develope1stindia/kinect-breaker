$(function() {

    var con = $.hubConnection(),
        hub = con.createHubProxy("kinectGame");

    con.start().done(function() {

        document.onkeydown = function keyboardController(e) {
            if (!e) {
                e = window.event;
            }

            switch (e.keyCode) {
            case 37:
                $('#left-hand-side').css('color', 'green');
                hub.invoke('groupMove', -1);

                break;
            case 39:
                $('#right-hand-side').css('color', 'green');
                hub.invoke('groupMove', 1);
                break;
            }
        };
        document.onkeyup = function keyboardController(e) {
            if (!e) {
                e = window.event;
            }
            $('#left-hand-side, #right-hand-side').css('color', '#F03060');
        };
    });
});