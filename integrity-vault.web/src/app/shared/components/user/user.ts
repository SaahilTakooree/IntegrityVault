import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
export type UserRole = 'doctor'|'patient'|'admin'|'external_provider'|'superadmin';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user.html',
})
export class UserFormComponent {
  @Input() role: UserRole = 'admin';
  @Input() hospitals: {id:any; name:string}[] = [];
  @Input() showHospitalSelect = false; // true only for SuperAdmin
  showPassword = false;
}