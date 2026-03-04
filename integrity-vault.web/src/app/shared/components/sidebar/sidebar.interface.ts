// Represents a single item in the modular sidebar component.
export interface ISidebarItem {
    label: string; // The text to be displayed for the sidebar item.
    icon?: string; // Optional icon name or path to be displayed next to the label.
    link?: string; // Optional route or url to navigate to the item when clicked.
    children?: ISidebarItem[] // Optional nested sidebar items for a submenu.
}