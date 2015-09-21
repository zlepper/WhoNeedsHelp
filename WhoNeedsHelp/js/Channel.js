var Help;
(function (Help) {
    var Channel = (function () {
        function Channel(id, channelName) {
            this.counting = false;
            this.outOfTime = false;
            this.timing = false;
            this.TimeLeft = 300;
            this.Id = id;
            this.ChannelName = channelName;
        }
        return Channel;
    })();
    Help.Channel = Channel;
})(Help || (Help = {}));
