import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { LoginFormValidationErrors } from "../../../shared/types/login-form-validation-errors.type";
import { AuthService } from "../../../core/services/auth.service";


@Component({
  selector: "app-login",
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: "./login.html",
  styleUrls: ["./login.scss"]
})


export class Login {
  usernameOrEmail : string = "";
  password: string = "";
  showPassword = false;
  loading = false;
  errors: LoginFormValidationErrors = {};

  private readonly _authService = inject(AuthService);

  onLogin() {
    this.errors = {};

    if (!this.usernameOrEmail.trim())
      this.errors.usernameOrEmail = "Username or email is required."

    if (!this.password)
      this.errors.password = "Password is required."

    if (this.errors.usernameOrEmail || this.errors.password)
      return

    this.loading = true

    this._authService.login(this.usernameOrEmail.trim(), this.password).subscribe({
      next : () => {
        this.loading = false;
      },
      error : (err) => {
        this.loading = false

        if (err.status === 401)
          this.errors.api = "Invalid credentials. Please try again.";
        else if (err.status === 0)
          this.errors.api = "Unable to reach the server. Check your connection.";
        else
          this.errors.api = "Something went wrong. Please try again later.";
      }
    });
  }
}