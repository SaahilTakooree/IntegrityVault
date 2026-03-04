// Import dependencies.
import { Component, Input, Output, EventEmitter } from '@angular/core'; // Import Angular core module.
import { CommonModule } from '@angular/common'; // Import CommonModule for common directives.
import { IConfirmButton } from './confirm-modal.interface'; // Import confirm model Interface.
import { ConfirmButtonStyle } from '../../enums/button-style.enum'; // Import confirm model button enum.

// Define the component decorator.
@Component({
    selector: 'app-confirm-modal',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './confirm-modal.html',
    styleUrls: ['./confirm-modal.scss']
})


// Define the ConfirmModalComponent class.
export class ConfirmModalComponent {
    // Make enum available to template.
    ConfirmButtonStyle = ConfirmButtonStyle;

    // Inputs: data coming into the component.
    @Input() show = false; // Input to show the modal or not.
    @Input() title = ''; // Input for the title of the modal.
    @Input() message = ''; // Input for what the message is going to be.
    @Input() buttons: IConfirmButton[] = []; // Input for the type of button that modal is going to have.

    // Outputs: events the component emits to parent.
    @Output() closed = new EventEmitter<void>(); // Output event when modal is closed.
    @Output() action = new EventEmitter<boolean>(); // Output event for action what was taken.

    // Emit closed event.
    close() {
        this.closed.emit();
    }

    // Emit on action event.
    onAction(result: boolean) {
        this.action.emit(result);
    }
}