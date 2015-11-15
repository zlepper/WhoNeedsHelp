"use strict";

/**
 * Descripes a single chat message
 * @class 
 * @returns {} 
 */
function ChatMessage() {
    /**
     * The id of this chat message in the database
     * @type {Number}
     */
    this.Id = null;
    /**
     * The text in the chat message
     * @type {String}
     */
    this.Text = null;
    /**
     * The user who said this
     * @type {User}
     */
    this.User = null;
}