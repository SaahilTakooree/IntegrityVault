// Import dependencies.
import { Component, Input, Output, EventEmitter } from "@angular/core"; // Import Angular core module.
import { CommonModule } from "@angular/common"; // Import CommonModule for common directives.
import { ISidebarItem } from "./sidebar.interface"; // Import SidebarItem Interface.


// Define the component decorator.
@Component({
  selector: "app-sidebar",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./sidebar.html",
  styleUrls: ["./sidebar.scss"]
})


// Define the SidebarComponent class.
export class SidebarComponent {
  // Inputs: data coming into the component.
  @Input() activeLink : string = ""; // Input for currently active link.
  @Input() isCollapsed: boolean = false; // Input to check if sidebar is collapsed.

  // Outputs: events the component emits to parent.
  @Output() linkClicked = new EventEmitter<string>(); // Output event when link is clicked.
  @Output() closeSidebar = new EventEmitter<void>(); // Output event when sidebar is closed.

  // Variable to store the sidebar items.
  private _sidebarItems: ISidebarItem[] = [];

  // Check if label does not exceed more than 15 character.:
  @Input()
  set sidebarItems(items: ISidebarItem[]) {
    // Check each label
    for (const item of items) {
      if (item.label.length > 15) {
        throw new Error(
          `Sidebar label "${item.label}" exceeds 15 characters. This is not allowed.`
        );
      }
    }
    this._sidebarItems = items;
  }
  get sidebarItems(): ISidebarItem[] {
    return this._sidebarItems;
  }


  // Emit closeSidebar event.
  onClose() {
    this.closeSidebar.emit();
  }

  // Emit linkClicked event with link.
  navigate(link?: string) {
    if (link) {
      this.linkClicked.emit(link);
    }
  }
}