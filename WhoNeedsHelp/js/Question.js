var Help;
(function (Help) {
    var Question = (function () {
        function Question(id, user, questionText) {
            this.Id = id;
            this.User = user;
            this.Text = questionText;
        }
        return Question;
    })();
    Help.Question = Question;
})(Help || (Help = {}));
//# sourceMappingURL=Question.js.map