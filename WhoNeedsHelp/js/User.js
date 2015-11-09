var Help;
(function (Help) {
    var User = (function () {
        function User(id, name) {
            this.Id = id;
            this.Name = name;
        }
        return User;
    })();
    Help.User = User;
})(Help || (Help = {}));
