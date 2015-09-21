module Help {
    export class Channel {
        Id: number;
        ChatMessages: { [id: number]: ChatMessage };
        Questions: { [id: number]: Question };
        ChannelName: string;
        Users: { [id: number]: User };
        QuestionState: QuestionState;
        HaveQuestion: boolean;
        IsAdmin: boolean;
        Text: string;
        MessageText: string;
        counting = false;
        outOfTime = false;
        timing = false;
        TimeLeft = 300;
        intervalCont: any;

        constructor(id: number, channelName: string) {
            this.Id = id;
            this.ChannelName = channelName;
        }
    }
}