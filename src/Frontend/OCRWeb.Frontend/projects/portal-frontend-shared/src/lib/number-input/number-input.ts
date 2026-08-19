import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

/**
 * Generic Material number field bound directly to a caller-owned FormControl. Owns no text —
 * label/hint/errorText all come from the consuming app's own label/message files.
 */
@Component({
  selector: 'lib-number-input',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './number-input.html',
  styleUrl: './number-input.scss'
})
export class NumberInput {
  readonly control = input.required<FormControl<number>>();
  readonly label = input.required<string>();
  readonly placeholder = input<string | null>(null);
  readonly min = input<number | null>(null);
  readonly max = input<number | null>(null);
  readonly step = input<number | null>(null);
  readonly errorText = input<string | null>(null);
  readonly hint = input<string | null>(null);
}
