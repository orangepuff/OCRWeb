import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatRadioModule } from '@angular/material/radio';
import { RadioOption } from './radio-option';

/**
 * Generic radio group bound directly to a caller-owned FormControl. Owns no text —
 * label and every option's label come from the consuming app's own label files.
 */
@Component({
  selector: 'lib-radio-group',
  imports: [ReactiveFormsModule, MatRadioModule],
  templateUrl: './radio-group.html',
  styleUrl: './radio-group.scss'
})
export class RadioGroup {
  readonly control = input.required<FormControl<string>>();
  readonly label = input.required<string>();
  readonly options = input.required<RadioOption[]>();
}
