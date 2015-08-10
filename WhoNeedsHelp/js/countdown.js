angular.module("Help")
    .filter("countdown", function() {
        return function (num) {
            if (num >= 0) {
                var min = parseInt(num / 60);
                min = (min < 10 ? "0" : "") + min;

                var sec = parseInt(num % 60);
                sec = (sec < 10 ? "0" : "") + sec;

                return min + ":" + sec;
            } else {
                var min = parseInt(num / 60);
                if (min == 0) {
                    min = "-0" + min;
                } else {
                    if (min > -10) {
                        var m = Math.abs(min);
                        min = "-0" + m;
                    }

                }
                var sec = Math.abs(parseInt(num % 60));

                return min + ":" + sec;
            }
        }
    });