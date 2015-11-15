"use strict";

/**
 * Descripes a user 
 * @param {Number} id The id of the user on the server 
 * @param {String} name The name of the user
 * @returns {} 
 * @class 
 */
function User(id, name) {
    /**
     * The id of the user
     * @type {Number}
     */
    this.Id = id;

    /**
     * The name of the user
     * @type {String}
     */
    this.Name = name;
}