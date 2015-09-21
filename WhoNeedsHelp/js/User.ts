module Help {
    export class User {
        Name: string;
        Id: number;

        constructor(id: number, name: string) {
            this.Id = id;
            this.Name = name;
        }
    }
}