function Me() {
    User.call(this, null, null);

    /**
     * Tells if the user is logged in
     * @type {Boolean}
     */
    this.LoggedIn = false;
}