module Help {
    export class LoginOptions {
        Name: string;
        Email: string;
        Password: string;
        Passwordcopy: string;
        StayLoggedIn: boolean;

        constructor() {
            this.Name = "";
            this.Email = "";
            this.Password = "";
            this.Passwordcopy = "";
        }
    }
}