"use strict";

/**
 * Descripes a question that has been asked
 * @param {Number} id 
 * @param {User} User 
 * @param {String} text 
 * @returns {} 
 * @class 
 */
function Question(id, user, text) {
    /**
     * The id of the question in the database
     * @type {Number}
     */
    this.Id = id;
    /**
     * The user who asked this question
     * @type {User}
     */
    this.User = user;
    /**
     * The question text if there is any
     * @type {String}
     */
    this.Text = text;
}