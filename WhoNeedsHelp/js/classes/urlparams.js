/**
 * Handle api usage
 * @param {Object} params 
 * @returns {} 
 * @class
 */
function UrlParams(params) {
    /**
     * User id
     */
    this.uid = params.uid;

    /**
     * password, should be a key of some sort, maybe a GUID
     */
    this.upass = params.upass;

    /**
     * User name, the actual displayname
     */
    this.uname = params.uname;

    /**
     * A unique key each channel should have the designates that this is actually a teacher connecting. 
     * If ommited to connecting client is considered a student
     * If not valid then connecting client is considered a student
     */
    this.teacherToken = params.teacherToken;

    /**
     * Channel name
     */
    this.cname = params.cname;

    /**
     * Channel id
     */
    this.cid = params.cid;
    /**
     * Indicates if the data is valid
     */
    this.isValid = this.validate();

}

/**
 * Validates that the parameters of the request are valid and can be used
 * @returns {Boolean} 
 */
UrlParams.prototype.validate = function() {
    if (helpers.isNullOrWhitespace(this.uid)) return false;
    if (helpers.isNullOrWhitespace(this.upass)) return false;
    if (helpers.isNullOrWhitespace(this.uname)) return false;
    if (helpers.isNullOrWhitespace(this.cname)) return false;
    if (helpers.isNullOrWhitespace(this.cid)) return false;
    return true;
}