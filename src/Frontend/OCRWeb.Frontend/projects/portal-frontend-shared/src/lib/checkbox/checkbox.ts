import { Component, input, model } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';

/**
 * Generic Material checkbox bound directly to a caller-owned FormControl. Owns no text —
 * label comes from the consuming app's own label/message files.
 *
 * Set `indeterminate` to true for select-all header checkboxes where only some rows are selected.
 */
@Component({
  selector: 'lib-checkbox',
  imports: [ReactiveFormsModule, MatCheckboxModule],
  templateUrl: './checkbox.html',
  styleUrl: './checkbox.scss'
})
export class Checkbox {
  readonly control = input.required<FormControl<boolean>>();
  readonly label = input.required<string>();
  readonly indeterminate = model(false);
}
