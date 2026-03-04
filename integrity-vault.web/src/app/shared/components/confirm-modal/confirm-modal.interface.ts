// Import the enum defining allowed button styles for the confirm modal.
import { ConfirmButtonStyle } from '../../enums/button-style.enum';

// Represent the button on the confirm modal.
export interface IConfirmButton {
  label: string; // The text to be displayed in the button.
  style?: ConfirmButtonStyle;  // The optional style of the button.
  result: boolean;  // If the button will lead to a positive or negative outcomes.
}