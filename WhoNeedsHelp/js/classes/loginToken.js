"use strict";
/**
 * Defines a token that is used to automatically login
 * @param {Number} id The id of the token
 * @param {String} key The of the token
 * @returns {} 
 * @class
 */
function LoginToken(id, key) {
    /**
     * The id of the token
     * @type {Number}
     */
    this.id = id;

    /**
     * The key of the token
     * @type {Number}
     */
    this.key = key;
}