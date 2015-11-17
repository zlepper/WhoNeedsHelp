"use strict";
/**
 * Defines a token that is used to automatically login
 * @param {Number} id The id of the token
 * @param {String} key The of the token
 * @returns {} 
 * @class
 */
function LoginToken(id, key, longer) {
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

    /**
     * Indicates if the user should stay logged in for a longer period of time
     */
    this.longer = longer;
}