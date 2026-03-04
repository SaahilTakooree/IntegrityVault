import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TopbarComponent } from '../../../shared/components/topbar/topbar';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal/confirm-modal';
import { ConfirmButtonStyle } from '../../../shared/enums/button-style.enum'; // Import confirm model button enum.
import { UserFormComponent } from '../../../shared/components/user/user';

@Component({
  selector: 'app-superadmin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent, TopbarComponent, ConfirmModalComponent, UserFormComponent],
  templateUrl: './super-admin.html',
  styleUrls: ['./super-admin.scss']
})
export class SuperadminDashboardComponent {
  activeLink = 'hospitals';
  activeWallet = '0x0000000000000000000000000000000000000000';
  get walletShort() { return this.activeWallet.slice(0,6)+'...'+this.activeWallet.slice(-4); }

  // Make enum available to template.
  ConfirmButtonStyle = ConfirmButtonStyle;
  
  isCollapsed: boolean = false;

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }


  handleDeleteResult(confirmed: boolean) {
    this.closeModals();

    if (confirmed) {
      console.log('Deleting:', this.deleteTarget);
    }
  }

  hospitals: any[] = [{id: 1, name: "123", walletAddress: "1234564"}];
  admins: any[]    = [];
  get noHospitals() { return this.hospitals.length === 0; }

  showHospitalModal = false;
  showAdminModal    = false;
  showDeleteModal   = false;
  editingHospital: any = null;
  editingAdmin: any    = null;
  deleteTarget = '';

  onNavigate(link: string) { this.activeLink = link; }
  openHospitalModal(item?: any) { this.editingHospital = item??null; this.showHospitalModal = true; }
  openAdminModal(item?: any)    { this.editingAdmin    = item??null; this.showAdminModal    = true; }
  openDeleteModal(name: string) { this.deleteTarget    = name;       this.showDeleteModal   = true; }
  closeModals() { this.showHospitalModal=false; this.showAdminModal=false; this.showDeleteModal=false; }
}