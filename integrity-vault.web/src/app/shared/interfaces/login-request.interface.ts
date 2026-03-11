// Interface to represent the payload sent when trying to login.
export interface LoginRequest {
    usernameOrEmail : string; // Username or email sent when trying to login.
    password : string; // Password sent when trying to login.
    ipAddress : string; // IP address of the client making the login request.
}