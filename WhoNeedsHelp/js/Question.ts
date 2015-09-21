module Help {
    export class Question {
        Id: number;
        User: User;
        Text: string;
        Class: string;

        constructor(id: number, user: User, questionText: string) {
            this.Id = id;
            this.User = user;
            this.Text = questionText;
        }
    }
}