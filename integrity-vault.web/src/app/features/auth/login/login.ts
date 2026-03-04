import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule,],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})

export class Login {
  email: string = '';
  password: string = '';
  errorMessage: string = '';

  private _authService = inject(AuthService);

  onSubmit() {
    this._authService.login(this.email, this.password).subscribe({
      next: user => {
        alert(`Login successful! Role: ${user.role}`);
        this.errorMessage = "";
      },
      error: err => {
        this.errorMessage = err.error || 'Login failed';
        alert(`Login failed: ${this.errorMessage}`);
      }
    });
  }
}
